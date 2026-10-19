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

short_commit="${commit:0:8}"
commit_summary="${short_commit}"
related_pr="없음"
if commit_json="$(github_curl "${api}/commits/${commit}" 2>/dev/null)"; then
    message="$(jq -r '.commit.message // empty' <<<"${commit_json}" 2>/dev/null || true)"
    message="${message//$'\r'/}"
    summary="${message%%$'\n'*}"
    if [[ "${summary}" =~ ^Merge\ pull\ request\ \#([0-9]+) ]]; then
        related_pr="https://github.com/${repository}/pull/${BASH_REMATCH[1]}"
        summary="${message##*$'\n'}"
    fi
    if [[ -n "${summary}" ]]; then
        commit_summary="${summary}"
    fi
fi

build_tag="uba-develop-${build_number}-${short_commit}"
asset_name="Project_Herbalist-Windows-${build_tag}.zip"
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

# The asset name identifies a retry even after the draft's build tag becomes vN.
# Assign vN only when publishing, so a failed draft cannot consume a public version.
find_release_and_version() {
    release_json=""
    max_version=0
    local page=1 releases count version existing
    while :; do
        releases="$(github_curl "${api}/releases?per_page=100&page=${page}")"
        count="$(jq 'length' <<<"${releases}")"
        version="$(jq '[.[] | select(.draft == false) | (.name // "") | select(test("^v[1-9][0-9]*$")) | ltrimstr("v") | tonumber] | max // 0' <<<"${releases}")"
        version="${version//$'\r'/}"
        if (( version > max_version )); then max_version="${version}"; fi
        existing="$(jq -c --arg tag "${build_tag}" --arg asset "${asset_name}" \
            'first(.[] | select(.tag_name == $tag or any(.assets[]?; .name == $asset))) // empty' <<<"${releases}")"
        if [[ -n "${existing}" ]]; then release_json="${existing}"; fi
        if (( count < 100 )); then break; fi
        ((page += 1))
    done
}

find_release_and_version
if [[ -n "${release_json}" ]]; then
    if jq -e '.draft == false' <<<"${release_json}" >/dev/null; then
        echo "Release already published for ${build_tag}."
        exit 0
    fi
    echo "Reusing draft release ${build_tag}."
else
    payload="$(jq -n \
        --arg tag "${build_tag}" \
        --arg commit "${commit}" \
        '{tag_name:$tag,target_commitish:$commit,name:$tag,body:"",draft:true,prerelease:true,generate_release_notes:false}')"
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

# GitHub rejects a duplicate tag if another build publishes at the same time.
for attempt in 1 2 3 4 5; do
    find_release_and_version
    if [[ -n "${release_json}" ]] && jq -e '.draft == false' <<<"${release_json}" >/dev/null; then
        echo "Release already published for ${build_tag}."
        exit 0
    fi
    version="v$((max_version + 1))"
    published_at="$(TZ=Asia/Seoul date '+%Y-%m-%d %H:%M KST')"
    release_body="$(printf '변경 내용: %s\n관련 PR: %s\n기준 브랜치: develop\n게시 시각: %s' \
        "${commit_summary}" "${related_pr}" "${published_at}")"
    publish_payload="$(jq -n --arg version "${version}" --arg body "${release_body}" \
        '{tag_name:$version,name:$version,body:$body,draft:false,prerelease:true}')"
    if published_json="$(github_curl --request PATCH --header 'Content-Type: application/json' \
        --data "${publish_payload}" "${api}/releases/${release_id}" 2>/dev/null)"; then
        jq -e --arg version "${version}" \
            '.draft == false and .tag_name == $version and .name == $version' \
            <<<"${published_json}" >/dev/null
        echo "Published https://github.com/${repository}/releases/tag/${version}"
        exit 0
    fi
done

echo "Could not publish a unique release version after five attempts." >&2
exit 1
