#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1-arm64/Unity.app/Contents/MacOS/Unity}"
build_log=/tmp/jumper-desktop-build.log
"$unity_editor" -batchmode -nographics -projectPath "$repo_root/Jumper" \
  -executeMethod BuildGame.BuildDesktop -quit -logFile "$build_log" &
build_pid=$!
trap 'kill "$build_pid" 2>/dev/null || true' INT TERM
while kill -0 "$build_pid" 2>/dev/null; do
  echo "Desktop build: $SECONDS seconds elapsed; progress in $build_log"
  sleep 10
done
wait "$build_pid"
grep -q BUILD_AND_TESTS_PASSED "$build_log"
player="$repo_root/Jumper/Build/Jump Jump.app/Contents/MacOS/Jump Jump"
for size in landscape portrait; do
  width=1280
  height=800
  if [ "$size" = portrait ]; then width=540; height=900; fi
  log="/tmp/jumper-$size-qa.log"
  "$player" --qa -screen-width "$width" -screen-height "$height" -screen-fullscreen 0 -logFile "$log" > "/tmp/jumper-$size-start.log" 2>&1 &
  player_pid=$!
  finished=0
  for attempt in $(seq 1 60); do
    if ! kill -0 "$player_pid" 2>/dev/null; then finished=1; break; fi
    sleep 1
  done
  if [ "$finished" = 0 ]; then
    kill "$player_pid" 2>/dev/null || true
    echo "QA timeout: $size. See $log" >&2
    exit 1
  fi
  wait "$player_pid"
  grep -q JUMPER_RUNTIME_QA_PASSED "$log"
  echo "$size runtime QA passed; screenshots are in /tmp/jumper-*.png"
done
