#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WEB="$ROOT/frontend/hr-performance-web"
WWW="$ROOT/src/HRPerformance.API/wwwroot"

echo "[UI] npm install..."
(cd "$WEB" && npm ci --silent)

echo "[UI] npm run build..."
(cd "$WEB" && npm run build)

echo "[UI] copy to wwwroot (clean assets)..."
rm -rf "$WWW/assets"
mkdir -p "$WWW"
cp -r "$WEB/dist/"* "$WWW/"

echo "[UI] OK — $(ls "$WWW/assets" | wc -l) asset file(s)"
