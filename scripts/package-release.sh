#!/usr/bin/env bash
set -euo pipefail
platform="${1:?platform required}"
source_root="$(cd "${2:?build directory required}" && pwd)"
mkdir -p "${3:?archive directory required}"
archive_root="$(cd "$3" && pwd)"
case "$platform" in
  macos)
    test -s "$source_root/Jump Jump.app/Contents/Info.plist"
    test -s "$source_root/Jump Jump.app/Contents/MacOS/Jump Jump"
    chmod +x "$source_root/Jump Jump.app/Contents/MacOS/Jump Jump"
    COPYFILE_DISABLE=1 tar -czf "$archive_root/JumpJump-macOS.tar.gz" -C "$source_root" 'Jump Jump.app'
    ;;
  windows)
    test -s "$source_root/Jump Jump.exe"
    test -s "$source_root/UnityPlayer.dll"
    test -d "$source_root/Jump Jump_Data"
    (cd "$source_root" && zip -qr "$archive_root/JumpJump-Windows-x64.zip" .)
    ;;
  linux)
    test -s "$source_root/Jump Jump.x86_64"
    test -s "$source_root/UnityPlayer.so"
    test -d "$source_root/Jump Jump_Data"
    chmod +x "$source_root/Jump Jump.x86_64"
    tar -czf "$archive_root/JumpJump-Linux-x64.tar.gz" -C "$source_root" .
    ;;
  *) echo "Unsupported platform: $platform" >&2; exit 1 ;;
esac
echo "RELEASE_PACKAGE_OK: $platform"
