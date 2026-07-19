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

echo ""
echo "بررسی پورت 5000..."
bash "$ROOT_DIR/scripts/free-port-5000.sh"

echo ""
echo "🚀 در حال اجرا..."
echo "   Application → http://localhost:5000"
echo "   Swagger     → http://localhost:5000/swagger"
echo ""
echo "برای توقف: Ctrl+C"
echo ""

dotnet run --project src/HRPerformance.API/HRPerformance.API.csproj --launch-profile http
