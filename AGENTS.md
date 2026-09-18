# Jump Jump
Unity 6000.6.0f1, URP 17.6.0. Project: Jumper/.
Plan changes, back up existing files, and run tests after each feature.
JumpRules.cs contains pure landing, charge and scoring rules; JumpTests.Run executes before every build.
JumpGame.cs constructs real 3D geometry and a native Canvas HUD at runtime.
BuildGame.BuildWebGL regenerates Assets/JumpJump.unity and configures URP and WebGL.
Run bash scripts/build-webgl.sh; build log: /tmp/jumper-build.log.
Run node --test scripts/*.test.mjs for bundle validator regression tests.
Never read secret values. CI runs in StevenLi-phoenix/unity-wechat-jumper.
.github/workflows/ci.yml validates on pushes and PRs, builds WebGL on trusted branches,
and publishes main to stevenli-phoenix-work/jump-jump:html5.
Required repository secrets: UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD, BUTLER_API_KEY.
GitHub secrets cannot be read back or copied from another repository via gh.
Do not modify the Flappy Bird repository to run this project.
Verify cloud completion and actual live rendering separately.

Version 1.1 presentation:
- JumpSession is the pure run-state model; JumpTests has 38 rules/audio checks.
- JumpWorld owns all geometry, cached materials, bounded platform history and effects.
- JumpHud uses static TextMeshPro SDF atlases generated from the bundled Bungee/Lato fonts.
  Do not revert to dynamic legacy Text: score changes invalidated other glyph meshes.
- JumpAudio generates bounded PCM and music; tests cover finite samples, envelopes and headroom.
- scripts/test-desktop.sh runs landscape and portrait native fixtures.
- --qa is desktop-only, runs in the background and never saves best scores.
- Browser checks must use Chrome Work and explicitly announce when browser use ends.
- The user confirmed itch.io works; focus on game quality rather than repeatedly checking cloud status.
