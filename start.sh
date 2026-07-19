#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

export PATH="${HOME}/.dotnet:${PATH:-}"

echo "=========================================="
echo "  HR Performance System"
echo "=========================================="

if ! command -v dotnet >/dev/null 2>&1; then
  echo "❌ dotnet یافت نشد. .NET 8 SDK را نصب کنید."
  exit 1
fi

APP_PORT="$(bash "$ROOT_DIR/scripts/resolve-app-port.sh")"
export ASPNETCORE_URLS="http://localhost:${APP_PORT}"
export ASPNETCORE_ENVIRONMENT=Development

echo ""
echo "🚀 در حال اجرا..."
echo "   Application → http://localhost:${APP_PORT}"
echo "   Swagger     → http://localhost:${APP_PORT}/swagger"
echo ""
echo "برای توقف: Ctrl+C"
echo ""

dotnet run --project src/HRPerformance.API/HRPerformance.API.csproj --no-launch-profile
