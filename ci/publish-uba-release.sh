#!/usr/bin/env bash
set -euo pipefail
set +x
umask 077

# Unity Build Automation runs this as a Windows builder post-build script.
repository="No1playblock/Project_Herbalist"
api="https://api.github.com/repos/${repository}"

: "${GITHUB_RELEASE_TOKEN:?Set GITHUB_RELEASE_TOKEN in the UBA build target environment variables.}"
build_number="${UCB_BUILD_NUMBER:-${BUILD_NUMBER:-}}"
commit="${BUILD_REVISION:-${GIT_COMMIT:-}}"
branch="${SCM_BRANCH:-${GIT_BRANCH:-}}"
: "${build_number:?UBA did not provide a build number.}"
: "${commit:?UBA did not provide the source revision.}"
: "${branch:?UBA did not provide the source branch.}"
: "${UNITY_PLAYER_PATH:?UBA did not provide UNITY_PLAYER_PATH.}"

echo "::mask-value::${GITHUB_RELEASE_TOKEN}"

if [[ "${branch#refs/heads/}" != "develop" ]]; then
    echo "Skipping GitHub Release for branch ${branch}."
    exit 0
fi

if [[ "${BUILDER_OS:-}" != "WINDOWS" ]]; then
    echo "Expected a Windows UBA builder, got ${BUILDER_OS:-unset}." >&2
    exit 1
fi

if [[ ! -f "${UNITY_PLAYER_PATH}" || "${UNITY_PLAYER_PATH,,}" != *.exe ]]; then
    echo "Windows player executable is missing: ${UNITY_PLAYER_PATH}" >&2
    exit 1
fi

command -v jq >/dev/null || { echo "jq is required." >&2; exit 1; }
seven_zip="$(command -v 7z || command -v 7z.exe || true)"
if [[ -z "${seven_zip}" && -x '/cygdrive/c/Program Files/7-Zip/7z.exe' ]]; then
    seven_zip='/cygdrive/c/Program Files/7-Zip/7z.exe'
fi
[[ -n "${seven_zip}" ]] || { echo "7-Zip is required on the UBA Windows builder." >&2; exit 1; }

github_curl() {
    printf 'header = "Authorization: Bearer %s"\n' "${GITHUB_RELEASE_TOKEN}" |
        curl --config - --silent --show-error --fail --location \
            --header 'Accept: application/vnd.github+json' \
            --header 'X-GitHub-Api-Version: 2022-11-28' "$@"
}

tag="uba-develop-${build_number}-${commit:0:8}"
asset_name="Project_Herbalist-Windows-${tag}.zip"
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "${temp_dir}"' EXIT
archive="${temp_dir}/${asset_name}"
player_dir="$(dirname "${UNITY_PLAYER_PATH}")"

# Archive the exe, *_Data directory, UnityPlayer.dll and other files together.
(cd "${player_dir}" && "${seven_zip}" a -tzip -mx=6 "$(cygpath -wa "${archive}")" . >/dev/null)
[[ -s "${archive}" ]] || { echo "Build archive was not created." >&2; exit 1; }
if (( $(stat -c '%s' "${archive}") >= 2147483648 )); then
    echo "GitHub Releases accepts files smaller than 2 GiB; archive is too large." >&2
    exit 1
fi

release_url="${api}/releases/tags/${tag}"
if release_json="$(github_curl "${release_url}" 2>/dev/null)"; then
    echo "Reusing draft release ${tag}."
else
    payload="$(jq -n \
        --arg tag "${tag}" \
        --arg commit "${commit}" \
        --arg number "${build_number}" \
        '{tag_name:$tag,target_commitish:$commit,name:("Windows build " + $number),body:("Unity Build Automation Windows build\nCommit: " + $commit),draft:true,prerelease:true,generate_release_notes:false}')"
    release_json="$(github_curl --request POST --header 'Content-Type: application/json' \
        --data "${payload}" "${api}/releases")"
fi

release_id="$(jq -er '.id' <<<"${release_json}")"
# Windows jq may emit CRLF; command substitution removes LF but retains CR.
release_id="${release_id//$'\r'/}"
[[ "${release_id}" =~ ^[0-9]+$ ]] || { echo "GitHub returned an invalid release ID." >&2; exit 1; }
upload_url="https://uploads.github.com/repos/${repository}/releases/${release_id}/assets?name=${asset_name}"

if ! jq -e --arg name "${asset_name}" \
    '[.assets[]? | select(.name == $name and .state == "uploaded")] | length > 0' \
    <<<"${release_json}" >/dev/null; then
    upload_json="$(github_curl --request POST \
        --header 'Content-Type: application/zip' \
        --data-binary "@${archive}" \
        --url "${upload_url}")"
    jq -e '.state == "uploaded"' <<<"${upload_json}" >/dev/null
fi

github_curl --request PATCH --header 'Content-Type: application/json' \
    --data '{"draft":false,"prerelease":true}' \
    "${api}/releases/${release_id}" >/dev/null

echo "Published https://github.com/${repository}/releases/tag/${tag}"
