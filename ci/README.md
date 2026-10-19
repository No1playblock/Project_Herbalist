# Automatic Windows builds

The `develop` branch is the source for Windows builds. Unity Build Automation (UBA) builds the game when a commit is pushed, then `publish-uba-release.sh` packages the Windows player and publishes one GitHub **prerelease** per successful build. The script leaves a release in draft state if the upload fails, so an empty release is not advertised. A retry with the same build number reuses that draft.

## One-time Unity Cloud configuration

Use the `Project_Herbalist` Unity Cloud project in the `playblock2000` organization (organization ID `14569754047186`, project ID `d9bb9af2-97f1-43ab-a58d-18a464708f57`). These IDs match the local `ProjectSettings.asset`. In the Unity Dashboard:

1. In Build Automation > Settings > Source control, select GitHub and connect `No1playblock/Project_Herbalist`. A fine-grained GitHub token restricted to this repository needs **Contents: Read and write** for Release publishing and **Webhooks: Read and write** for auto-build. Unity's documented source-control minimum is Contents read-only plus Webhooks read/write; the publishing script also needs Contents write. The connection must be able to read Git LFS objects.
2. Create a build target for **Windows desktop 64-bit**, branch `develop`, Unity **6000.6.0f1** (or auto-detect from `ProjectSettings/ProjectVersion.txt`). Use the project root as the project path and the enabled scenes from `ProjectSettings/EditorBuildSettings.asset`.
3. Enable **Auto-build** and leave **Auto-cancel** off. Auto-cancel would skip intermediate pushes, while this pipeline keeps each successful build.
4. In Advanced Settings, set **Post-Build Script Path** to `ci/publish-uba-release.sh`.
5. Add a build-target environment variable named `GITHUB_RELEASE_TOKEN` containing that token, or a separate repository-scoped token with **Contents: Read and write**. Create and enter tokens directly in GitHub and Unity Cloud; never commit them or paste them into build logs.
6. Save the target configuration and push this CI change to `develop`. The first successful build should create a prerelease with a tag like `uba-develop-123-abcdef12` and a `Project_Herbalist-Windows-*.zip` asset. Unzip it before running the `.exe` so its `_Data` directory and DLLs remain alongside the executable.

The post-build script requires UBA's Windows builder, `UNITY_PLAYER_PATH`, `UCB_BUILD_NUMBER`, `SCM_BRANCH`, `BUILD_REVISION`, and the installed Cygwin `jq` and 7-Zip tools. Unity documents these tools as present on its Windows builders. It publishes only from `develop`. A GitHub Release asset must be under 2 GiB; if the ZIP reaches that limit, split the upload or choose another artifact store.

Release titles show the build time in KST, the merged PR title (or direct commit subject), and the short commit ID. The release body identifies the `develop` commit and explains which ZIP to download. `UCB_BUILD_NUMBER` remains in the tag and release body for tracing UBA logs; it is a build counter, not a game version, and may restart when a build target is recreated. Choose the most recently published prerelease for the newest `develop` build rather than the largest build number.

## Verification

Check the UBA build history for a successful Windows build, then confirm that the matching GitHub prerelease contains a ZIP with the executable, `_Data` folder, and runtime DLLs. The script does not run for a failed Unity build. Keep the Release token out of the source repository. If the UBA run fails during packaging or upload, inspect its post-build log; the draft release can be reused when the same build is retried.
