#!/usr/bin/env bash
# بسته توسعه لوکال — سورس کامل برای کار در Cursor/Visual Studio
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.7.0-dev}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"

rm -rf "$STAGE"
mkdir -p "$STAGE"

# سورس backend (بدون bin/obj)
mkdir -p "$STAGE/src"
for proj in HRPerformance.Domain HRPerformance.Application HRPerformance.Infrastructure HRPerformance.API; do
  cp -r "$ROOT/src/$proj" "$STAGE/src/"
  rm -rf "$STAGE/src/$proj/bin" "$STAGE/src/$proj/obj"
done

# بهینه‌سازی Build
cp "$ROOT/Directory.Build.props" "$ROOT/global.json" "$ROOT/nuget.config" "$STAGE/"
mkdir -p "$STAGE/.vscode"
cp "$ROOT/.vscode/settings.json" "$STAGE/.vscode/"

# Database + docs
cp -r "$ROOT/database" "$STAGE/"
cp -r "$ROOT/docs" "$STAGE/"
cp "$ROOT/app-CONNECTION_SETUP.txt" "$STAGE/" 2>/dev/null || true

# Frontend source (بدون node_modules)
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
cp "$ROOT/scripts/"*.bat "$ROOT/scripts/"*.sh "$ROOT/scripts/"*.ps1 "$STAGE/scripts/" 2>/dev/null || true

cp "$ROOT/README.md" "$STAGE/" 2>/dev/null || true

cat > "$STAGE/README-LOCAL.txt" << 'EOF'
HR Performance — نسخه توسعه لوکال (سورس کامل)

این ZIP برای کار روی سیستم خودتان است — مثل v2.0.0-simple.

شامل:
  HRPerformance.sln
  src/ (4 پروژه: Domain, Application, Infrastructure, API)
  frontend/hr-performance-web
  database/

پیش‌نیاز: .NET 8 SDK + SQL Server + (اختیاری) Node.js برای UI

─── راه‌اندازی اولین بار ───
1) Extract کنید — ترجیحاً C:\Projects\HRPerformance (نه Downloads)
2) database/01 تا 15 را روی SQL Server اجرا کنید
3) src\HRPerformance.API\appsettings.Development.json — پسورد SQL
4) scripts\restore-packages.bat  (فقط اولین بار — ممکن است چند دقیقه)
5) start-local.bat  یا  HRPerformance.sln را در Cursor باز کنید

─── اجراهای بعدی ───
  start-local.bat  → بدون Build مجدد (--no-build)
  یا F5 در Cursor/Visual Studio

─── توسعه UI ───
  cd frontend\hr-performance-web
  npm install
  npm run dev

لاگین: admin / Admin@123
EOF

cat > "$STAGE/DEV-WORKFLOW.txt" << 'EOF'
چرا اولین بار کند است؟
  فقط اولین Restore/Build چند دقیقه طول می‌کشد (دانلود NuGet).
  بعد از آن اجرا با --no-build است — چند ثانیه.

چرا publish نداریم؟
  شما سورس می‌خواهید — در Cursor/VS کار کنید.
  پوشه app/ فقط برای deploy است، نه توسعه.

اگر Build دوباره کند شد:
  - پوشه را از Downloads خارج کنید
  - آنتی‌ویروس را برای این پوشه استثنا کنید
  - scripts\restore-packages.bat را یک بار اجرا کنید
  - فقط src\HRPerformance.API را Build کنید، نه کل solution
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance — DEV (source)
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
.NET 8 SDK required

شامل: src/ + sln + frontend + database
بدون: app/ publish — برای توسعه لوکال
EOF

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r -q "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT ($(du -h "$OUT" | cut -f1))"
unzip -l "$OUT" | tail -3
