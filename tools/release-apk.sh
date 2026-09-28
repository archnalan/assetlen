#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# ASSETLEN — build a release APK of the native head and say what signed it.
#
# Android refuses to install an unsigned APK, so there is no such thing as an
# unsigned build: with no keystore supplied, .NET Android falls back to a
# per-machine debug key. That is fine for handing the file to a reader
# directly, and useless for Play — so this script always prints the signer
# rather than letting you assume.
#
# Usage:  bash tools/release-apk.sh [api-base-url]
#         AssetlenKeystore=... AssetlenKeyAlias=... bash tools/release-apk.sh
# Needs:  the android workload. Run from the repo root.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail

PROJ="assetlen.Maui/assetlen.Maui.csproj"
TFM="net10.0-android"
API_BASE="${1:-}"

c_head=$'\033[1m'; c_warn=$'\033[33m'; c_ok=$'\033[32m'; c_off=$'\033[0m'

[ -f "$PROJ" ] || { echo "Run from the repo root (no $PROJ here)."; exit 1; }

ARGS=(publish "$PROJ" -f "$TFM" -c Release --nologo)
[ -n "$API_BASE" ] && ARGS+=("-p:AssetlenApiBaseUrl=$API_BASE")

# Signing material, if the caller supplied any. Absent these the build still
# succeeds and the debug key is used; the summary below makes that obvious.
if [ -n "${AssetlenKeystore:-}" ]; then
    ARGS+=("-p:AndroidSigningKeyStore=$AssetlenKeystore")
    ARGS+=("-p:AndroidSigningKeyAlias=${AssetlenKeyAlias:-assetlen}")
    ARGS+=("-p:AndroidSigningStorePass=${AssetlenStorePass:-}")
    ARGS+=("-p:AndroidSigningKeyPass=${AssetlenKeyPass:-${AssetlenStorePass:-}}")
fi

echo "${c_head}Building $TFM (Release)...${c_off}"
dotnet "${ARGS[@]}" || exit 1

APK=$(find "assetlen.Maui/bin/Release/$TFM" -name "*-Signed.apk" -o -name "*.apk" 2>/dev/null \
      | grep -v "unsigned" | sort | tail -1)
[ -n "$APK" ] || { echo "Build reported success but no APK was produced."; exit 1; }

echo
echo "${c_head}APK${c_off}       $APK"
echo "${c_head}Size${c_off}      $(du -h "$APK" | cut -f1)"

# Report the signer. A debug-signed APK installs fine but can never go to Play,
# and its key is per-machine — so this line is the one that matters.
# apksigner is a JVM tool and the Android SDK does not put a JDK on PATH; the
# build itself finds one, so locate the same one rather than asking the caller.
if [ -z "${JAVA_HOME:-}" ]; then
    for j in "/c/Program Files/Android/openjdk/jdk-21"* "/c/Program Files (x86)/Android/openjdk/jdk-17"*; do
        [ -x "$j/bin/java.exe" ] && export JAVA_HOME="$j" && break
    done
fi

APKSIGNER=$(ls "/c/Program Files (x86)/Android/android-sdk/build-tools/"*/apksigner.bat 2>/dev/null | sort -V | tail -1)
if [ -n "$APKSIGNER" ]; then
    echo
    echo "${c_head}Signer${c_off}"
    "$APKSIGNER" verify --print-certs "$APK" 2>/dev/null | sed 's/^/  /'
    if "$APKSIGNER" verify --print-certs "$APK" 2>/dev/null | grep -qi "CN=Android Debug"; then
        echo "  ${c_warn}This is the per-machine DEBUG key.${c_off}"
        echo "  ${c_warn}Fine for sideloading. Play will reject it, and the key is not backed up.${c_off}"
    else
        echo "  ${c_ok}Signed with a supplied release key.${c_off}"
    fi
fi
