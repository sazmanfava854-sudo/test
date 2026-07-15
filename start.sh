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

if [[ ! -f "src/HRPerformance.API/obj/project.assets.json" ]]; then
  echo ""
  echo "اولین اجرا: در حال restore پکیج‌های NuGet..."
  dotnet restore HRPerformance.sln --disable-parallel --verbosity minimal
fi

echo ""
echo "🚀 در حال اجرا..."
echo "   Application → http://localhost:5000"
echo "   Swagger     → http://localhost:5000/swagger"
echo ""
echo "برای توقف: Ctrl+C"
echo ""

dotnet run --project src/HRPerformance.API/HRPerformance.API.csproj --launch-profile http
