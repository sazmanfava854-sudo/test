#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"
export PATH="${HOME}/.dotnet:${PATH:-}"

echo "IoT Recommendation — starting web app..."
dotnet restore IoTRecommendation.sln --verbosity minimal
dotnet run --project IoTRecommendation.Web/IoTRecommendation.Web.csproj
