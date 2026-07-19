#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.5.1-simple-local}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"
PUBLISH_DIR="$STAGE/app"

rm -rf "$STAGE"
mkdir -p "$STAGE"

echo "==> Publishing pre-built app (no build needed on Windows)..."
dotnet publish "$ROOT/src/HRPerformance.API/HRPerformance.API.csproj" \
  -c Release \
  -o "$PUBLISH_DIR" \
  --no-self-contained \
  -v minimal

# Multi-project source (no bin/obj)
mkdir -p "$STAGE/src"
for proj in HRPerformance.Domain HRPerformance.Application HRPerformance.Infrastructure HRPerformance.API; do
  cp -r "$ROOT/src/$proj" "$STAGE/src/"
  rm -rf "$STAGE/src/$proj/bin" "$STAGE/src/$proj/obj"
done

# Build optimizations
cp "$ROOT/Directory.Build.props" "$ROOT/global.json" "$ROOT/nuget.config" "$STAGE/"
mkdir -p "$STAGE/.vscode"
cp "$ROOT/.vscode/settings.json" "$STAGE/.vscode/"

# Database + docs
cp -r "$ROOT/database" "$STAGE/"
cp -r "$ROOT/docs" "$STAGE/"

# Frontend source (no node_modules / dist)
mkdir -p "$STAGE/frontend/hr-performance-web"
for item in package.json package-lock.json tsconfig.json tsconfig.app.json tsconfig.node.json vite.config.ts index.html public src; do
  if [ -e "$ROOT/frontend/hr-performance-web/$item" ]; then
    cp -r "$ROOT/frontend/hr-performance-web/$item" "$STAGE/frontend/hr-performance-web/"
  fi
done

# Solution + scripts
cp "$ROOT/HRPerformance.sln" "$STAGE/"
cp "$ROOT/start.bat" "$ROOT/start-local.bat" "$ROOT/start.sh" "$ROOT/run-api.bat" "$STAGE/" 2>/dev/null || true
cp "$ROOT/publish-iis.bat" "$ROOT/publish-iis.ps1" "$ROOT/iis-fix-permissions.bat" "$STAGE/" 2>/dev/null || true
mkdir -p "$STAGE/scripts"
cp "$ROOT/scripts/"*.bat "$ROOT/scripts/"*.sh "$STAGE/scripts/" 2>/dev/null || true

cp "$ROOT/README.md" "$STAGE/" 2>/dev/null || true

cat > "$STAGE/README-LOCAL.txt" << 'EOF'
HR Performance — نسخه لوکال (بدون نیاز به Build)

پیش‌نیاز: .NET 8 Runtime/SDK + SQL Server

1) database/01 تا 11 را روی SQL Server اجرا کنید
2) app/appsettings.Development.json را تنظیم کنید (پسورد SQL و MIS)
3) دوبار کلیک روی start-local.bat
4) http://localhost:5050

⚡ پوشه app/ از قبل Build شده — start-local.bat بدون انتظار اجرا می‌شود.

سینک MIS (فقط دستی از UI):
  تنظیمات > دریافت از MIS > انتخاب بازه تاریخ > دریافت داده

توسعه سورس (اختیاری):
  scripts\build-once.bat
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance System — SIMPLE LOCAL (PRE-BUILT)
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
.NET: 8 Runtime required

شامل:
- app/ (از قبل Build شده — بدون انتظار)
- HRPerformance.sln (4 پروژه برای توسعه)
- Directory.Build.props, global.json, nuget.config
- database/ (01-11)
- start-local.bat
EOF

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT"
ls -lh "$OUT"
unzip -l "$OUT" | tail -3
