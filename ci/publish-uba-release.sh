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
release_date="$(TZ=Asia/Seoul date '+%Y-%m-%d %H:%M KST')"
commit_summary="${short_commit}"
if commit_json="$(github_curl "${api}/commits/${commit}" 2>/dev/null)"; then
    message="$(jq -r '.commit.message // empty' <<<"${commit_json}" 2>/dev/null || true)"
    message="${message//$'\r'/}"
    summary="${message%%$'\n'*}"
    if [[ "${summary}" == "Merge pull request"* ]]; then
        summary="${message##*$'\n'}"
    fi
    if [[ -n "${summary}" ]]; then
        commit_summary="${summary}"
    fi
fi

release_name="Windows | ${release_date} | ${commit_summary} (${short_commit})"
release_body="$(printf '변경 내용: %s\n기준 브랜치: develop\n빌드 시각: %s\nUnity 빌드 번호: %s (게임 버전이 아닌 빌드 횟수)\n기준 커밋: https://github.com/%s/commit/%s\n\n최신 develop 빌드는 릴리스 목록에서 게시 시각이 가장 늦은 항목을 선택하세요. 아래 Assets에서 Project_Herbalist-Windows-*.zip 파일을 다운로드하고 압축을 푼 다음 .exe를 실행하세요. _Data 폴더와 DLL 파일은 실행 파일 옆에 두세요.' \
    "${commit_summary}" "${release_date}" "${build_number}" "${repository}" "${commit}")"

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
    echo "Reusing release ${tag}."
else
    payload="$(jq -n \
        --arg tag "${tag}" \
        --arg commit "${commit}" \
        --arg name "${release_name}" \
        --arg body "${release_body}" \
        '{tag_name:$tag,target_commitish:$commit,name:$name,body:$body,draft:true,prerelease:true,generate_release_notes:false}')"
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

publish_payload="$(jq -n --arg name "${release_name}" --arg body "${release_body}" \
    '{draft:false,prerelease:true,name:$name,body:$body}')"
github_curl --request PATCH --header 'Content-Type: application/json' \
    --data "${publish_payload}" \
    "${api}/releases/${release_id}" >/dev/null

echo "Published https://github.com/${repository}/releases/tag/${tag}"
