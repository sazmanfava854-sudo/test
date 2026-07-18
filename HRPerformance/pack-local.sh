#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.5.0-simple-local}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"

rm -rf "$STAGE"
mkdir -p "$STAGE"

# Multi-project backend
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
cp "$ROOT/scripts/init-database.sh" "$STAGE/scripts/" 2>/dev/null || true

cp "$ROOT/README.md" "$STAGE/" 2>/dev/null || true

cat > "$STAGE/README-LOCAL.txt" << 'EOF'
HR Performance — نسخه لوکال (معماری v2.0.0-simple + فیلتر تاریخ MIS)

پیش‌نیاز: .NET 8 SDK + SQL Server

1) database/01 تا 11 را روی SQL Server اجرا کنید
2) src/HRPerformance.API/appsettings.Development.json را تنظیم کنید
3) start-local.bat (ویندوز) یا start.sh (لینوکس)
4) http://localhost:5050

سینک MIS (فقط دستی از UI):
  تنظیمات > دریافت از MIS > انتخاب بازه تاریخ > دریافت داده

توسعه UI (اختیاری):
  cd frontend/hr-performance-web
  npm install && npm run dev
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance System — SIMPLE LOCAL
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
.NET: 8 SDK required

شامل:
- HRPerformance.sln (4 پروژه: Domain, Application, Infrastructure, API)
- Directory.Build.props, global.json, nuget.config
- database/ (01-11)
- docs/
- frontend/hr-performance-web (سورس)
- start-local.bat / start.sh
EOF

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT"
ls -lh "$OUT"
unzip -l "$OUT" | tail -3
