#!/usr/bin/env bash
set -euo pipefail
project_dir="$(dirname "$(realpath "$0")")"
export DOTNET_CLI_HOME="${project_dir}/.dotnet"
export NUGET_PACKAGES="${project_dir}/.nuget"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet build "${project_dir}/ProbablyStolenMixer.csproj" -c Release "$@"
