#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
export DOTNET_CLI_TELEMETRY_OPTOUT=1
if command -v dotnet >/dev/null 2>&1; then
  dotnet run --project Tests/Melman.Tests.csproj
elif [[ -x /workspace/tooling/dotnet/dotnet ]]; then
  export DOTNET_CLI_HOME=/workspace/tooling/dotnet-home
  /workspace/tooling/dotnet/dotnet run --project Tests/Melman.Tests.csproj
else
  echo 'Install .NET SDK 8 to run the core tests.' >&2
  exit 1
fi
