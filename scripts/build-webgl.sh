#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1-arm64/Unity.app/Contents/MacOS/Unity}"
"$unity_editor" -batchmode -nographics -projectPath "$repo_root/Jumper" -buildTarget WebGL -executeMethod BuildGame.BuildWebGL -quit -logFile /tmp/jumper-build.log
node "$repo_root/scripts/check-webgl.mjs" "$repo_root/Jumper/Build/WebGL"
