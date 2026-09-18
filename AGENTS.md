# Jump Jump
Unity 6000.6.0f1, URP 17.6.0. Project: Jumper/.
Plan changes, back up existing files, and run tests after each feature.
JumpRules.cs contains pure landing, charge and scoring rules; JumpTests.Run executes before every build.
JumpGame.cs constructs real 3D geometry and a native Canvas HUD at runtime.
BuildGame.BuildWebGL regenerates Assets/JumpJump.unity and configures URP and WebGL.
Run bash scripts/build-webgl.sh; build log: /tmp/jumper-build.log.
Run node --test scripts/*.test.mjs for bundle validator regression tests.
Never read secret values. Publishing uses the existing Unity and butler secrets in
StevenLi-phoenix/unity_flappy_bird via its publish-jumper.yml workflow.
It checks out this repository and publishes only the jump-jump:html5 channel.
Verify cloud completion and actual live rendering separately.
