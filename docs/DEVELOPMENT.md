# Development and releases

Open Jumper/ in Unity **6000.6.0f1**, open Assets/JumpJump.unity, and press Play. The scene creates its camera, geometry and interface at runtime.

## Local builds

Run these commands from the repository root:

    bash scripts/build-webgl.sh
    node --test scripts/*.test.mjs
    bash scripts/test-desktop.sh

The WebGL output is Jumper/Build/WebGL; serve that directory over HTTP. The desktop test script builds a macOS player and checks fifteen landings, bounded platforms, pause, results and retry in landscape and portrait. Screenshot fixtures in /tmp/jumper-*.png do not change saved best scores.

Every Unity build runs 38 gameplay/audio checks. Node tests cover WebGL validation, release version matching, archive contents, executable permissions and incomplete-build rejection.

Editor batch entry points:

| Method | Target | Output |
| --- | --- | --- |
| BuildGame.BuildWebGL | WebGL | Jumper/Build/WebGL |
| BuildGame.BuildDesktop | Local macOS QA | Jumper/Build/Jump Jump.app |
| BuildGame.BuildMacOS | macOS Universal (Intel + Apple Silicon) | Jumper/Build/macOS/Jump Jump.app |
| BuildGame.BuildWindows | Windows x64 | Jumper/Build/Windows/Jump Jump.exe |
| BuildGame.BuildLinux | Linux x64 | Jumper/Build/Linux/Jump Jump.x86_64 |

Use the Unity editor with the relevant platform module installed:

    Unity -batchmode -nographics -projectPath "$PWD/Jumper"       -executeMethod BuildGame.BuildMacOS -quit -logFile /tmp/jumper-release.log
    bash scripts/package-release.sh macos Jumper/Build/macOS /tmp/jumper-release

For Windows use BuildGame.BuildWindows, windows and Jumper/Build/Windows. For Linux use BuildGame.BuildLinux, linux and Jumper/Build/Linux. The package scripts include the full runtime and data folders, not just the executable.

## Desktop releases

The Desktop Release workflow in .github/workflows/release.yml runs on pushed v* tags. Manual dispatch takes an **existing version tag**; it builds that tag's source. It does not create a tag.

1. Update bundleVersion in Jumper/ProjectSettings/ProjectSettings.asset, commit and push.
2. Create and push a matching tag, for example v1.1.0 for bundleVersion 1.1.0.
3. Wait for all three platform builds to pass. The publishing job then creates or updates the GitHub Release.

BuildGame preserves the serialized version; it does not overwrite it. The workflow rejects a tag whose version does not match the source. It uses the existing UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD repository secrets and GitHub's scoped token for release uploads. BUTLER_API_KEY is only used by the separate WebGL/itch.io workflow.

Release files:

- JumpJump-macOS.tar.gz — Universal .app bundle, preserving executable permissions.
- JumpJump-Windows-x64.zip — executable, UnityPlayer.dll and game data.
- JumpJump-Linux-x64.tar.gz — executable, UnityPlayer.so and game data.
- SHA256SUMS.txt — checksums for all three archives.

Players use the Mono backend, built with Unity 6000.6.0f1 in platform-specific GameCI images. macOS downloads are not Developer ID signed/notarized, and Windows downloads are not Authenticode signed. Rerunning the same tag replaces its release attachments after all three builds succeed.

## Source map

- JumpSession.cs: phases, charge, score, combo and run statistics.
- JumpWorld.cs: themed rooftops, character, clouds, bounded effects and materials.
- JumpHud.cs: menus and responsive HUD using static TextMeshPro atlases.
- JumpAudio.cs: synthesized music and feedback sounds.
- JumpGame.cs: input, animation, camera and desktop runtime checks.
- BuildGame.cs: repeatable scene generation and platform builds.

Bungee and Lato fonts retain their bundled SIL Open Font Licenses. TextMeshPro essential resources retain their bundled notices.
