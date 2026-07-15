#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

export PATH="${HOME}/.dotnet:${PATH:-}"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"

echo "=========================================="
echo "  HR Performance System"
echo "=========================================="

if ! command -v dotnet >/dev/null 2>&1; then
  echo "❌ dotnet یافت نشد. ابتدا .NET 9 SDK را نصب کنید."
  exit 1
fi

if ! command -v npm >/dev/null 2>&1; then
  echo "❌ npm یافت نشد. ابتدا Node.js را نصب کنید."
  exit 1
fi

if [ ! -d "node_modules" ] || [ ! -d "frontend/hr-performance-web/node_modules" ]; then
  echo "📦 نصب وابستگی‌ها (اولین بار)..."
  npm run setup
fi

echo ""
echo "🚀 در حال اجرا..."
echo "   Backend  → http://localhost:5000"
echo "   Swagger  → http://localhost:5000/swagger"
echo "   Frontend → http://localhost:3000"
echo ""
echo "برای توقف: Ctrl+C"
echo ""

npm run dev
