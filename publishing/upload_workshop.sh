#!/usr/bin/env bash
set -euo pipefail
ROOT="$(dirname "$(dirname "$(realpath "$0")")")"
python "${ROOT}/publishing/prepare_workshop.py"
if [[ $# -ne 1 ]]; then
    printf '%s\n' 'Usage: bash publishing/upload_workshop.sh YOUR_STEAM_LOGIN_NAME' >&2
    exit 2
fi
if [[ ! -x "${ROOT}/.tools/steamcmd/steamcmd.sh" ]]; then
    printf '%s\n' 'SteamCMD not found; see publishing/README.md.' >&2
    exit 1
fi
# Never put a password on the command line; SteamCMD will prompt interactively.
# Account/config caches live under ignored .tools, not in the source repository.
export HOME="${ROOT}/.tools/steam-home"
mkdir -p "${HOME}"
cd "${ROOT}/.tools/steamcmd"
exec ./steamcmd.sh +login "$1" +workshop_build_item "${ROOT}/.tools/steamcmd/workshop.vdf" +quit
