#!/usr/bin/env bash
# بسته توسعه لوکال — سورس + wwwroot از پیش build + app/ publish
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.7.0-dev}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"

echo "==> Building UI into src/HRPerformance.API/wwwroot ..."
bash "$ROOT/scripts/build-ui.sh"

rm -rf "$STAGE"
mkdir -p "$STAGE"

# سورس backend (بدون bin/obj) — شامل wwwroot
mkdir -p "$STAGE/src"
for proj in HRPerformance.Domain HRPerformance.Application HRPerformance.Infrastructure HRPerformance.API; do
  cp -r "$ROOT/src/$proj" "$STAGE/src/"
  rm -rf "$STAGE/src/$proj/bin" "$STAGE/src/$proj/obj"
done

if [ ! -f "$STAGE/src/HRPerformance.API/wwwroot/index.html" ]; then
  echo "ERROR: src/HRPerformance.API/wwwroot/index.html missing after UI build"
  exit 1
fi

echo "==> Publishing app/ (includes wwwroot) ..."
dotnet publish "$ROOT/src/HRPerformance.API/HRPerformance.API.csproj" \
  -c Release \
  -o "$STAGE/app" \
  --no-self-contained \
  -v minimal

if [ ! -f "$STAGE/app/wwwroot/index.html" ]; then
  echo "ERROR: app/wwwroot/index.html missing after publish"
  exit 1
fi

# appsettings Development alongside publish output
cp "$ROOT/src/HRPerformance.API/appsettings.Development.json" "$STAGE/app/" 2>/dev/null || true
cp "$ROOT/src/HRPerformance.API/appsettings.json" "$STAGE/app/" 2>/dev/null || true

# بهینه‌سازی Build
cp "$ROOT/Directory.Build.props" "$ROOT/global.json" "$ROOT/nuget.config" "$STAGE/"
mkdir -p "$STAGE/.vscode"
cp "$ROOT/.vscode/settings.json" "$ROOT/.vscode/launch.json" "$ROOT/.vscode/tasks.json" "$STAGE/.vscode/" 2>/dev/null || cp "$ROOT/.vscode/settings.json" "$STAGE/.vscode/"

# Database + docs
cp -r "$ROOT/database" "$STAGE/"
cp -r "$ROOT/docs" "$STAGE/"
cp "$ROOT/app-CONNECTION_SETUP.txt" "$ROOT/LOGIN.txt" "$STAGE/" 2>/dev/null || true

# Frontend source (بدون node_modules)
mkdir -p "$STAGE/frontend/hr-performance-web"
for item in package.json package-lock.json tsconfig.json tsconfig.app.json tsconfig.node.json vite.config.ts index.html public src; do
  if [ -e "$ROOT/frontend/hr-performance-web/$item" ]; then
    cp -r "$ROOT/frontend/hr-performance-web/$item" "$STAGE/frontend/hr-performance-web/"
  fi
done

# Solution + scripts
cp "$ROOT/HRPerformance.sln" "$STAGE/"
cp "$ROOT/start.bat" "$ROOT/start-local.bat" "$ROOT/start-published.bat" "$ROOT/start.sh" "$ROOT/run-api.bat" "$STAGE/" 2>/dev/null || true
cp "$ROOT/publish-iis.bat" "$ROOT/publish-iis.ps1" "$ROOT/iis-fix-permissions.bat" "$STAGE/" 2>/dev/null || true
mkdir -p "$STAGE/scripts"
cp "$ROOT/scripts/"*.bat "$ROOT/scripts/"*.sh "$ROOT/scripts/"*.ps1 "$STAGE/scripts/" 2>/dev/null || true

cp "$ROOT/README.md" "$STAGE/" 2>/dev/null || true

cat > "$STAGE/README-LOCAL.txt" << 'EOF'
HR Performance — نسخه توسعه لوکال (سورس + Publish)

شامل:
  HRPerformance.sln + src/ (با wwwroot از پیش build)
  app/ — خروجی dotnet publish (wwwroot + DLL) — بدون Build
  frontend/hr-performance-web — سورس UI
  database/

─── اجرای سریع (بدون Build) ───
  start-published.bat  → از app\ با wwwroot آماده

─── توسعه با سورس ───
  start-local.bat  → dotnet build + run از src\

مسیر wwwroot:
  src\HRPerformance.API\wwwroot\   (سورس)
  app\wwwroot\                     (publish)

پیش‌نیاز: .NET 8 SDK + SQL Server

1) Extract — ترجیحاً C:\Projects\HRPerformance
2) database/01 تا 16 روی SQL Server
3) appsettings.Development.json — پسورد SQL (در src\... و app\)
4) scripts\restore-packages.bat (فقط برای start-local.bat)
5) start-published.bat  یا  start-local.bat

لاگین: admin / Admin@123
EOF

cat > "$STAGE/DEV-WORKFLOW.txt" << 'EOF'
دو روش اجرا:

A) start-published.bat
   - از app\ اجرا می‌شود
   - wwwroot داخل app\wwwroot\ است
   - Build لازم نیست

B) start-local.bat
   - از src\ build می‌گیرد
   - wwwroot در src\HRPerformance.API\wwwroot\

اگر UI قدیمی بود:
  scripts\build-ui.bat
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance — DEV (source + publish)
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
.NET 8 SDK required

شامل:
  src/HRPerformance.API/wwwroot/  (UI build)
  app/wwwroot/                      (publish)

اجرا بدون Build: start-published.bat
توسعه: start-local.bat
EOF

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r -q "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT ($(du -h "$OUT" | cut -f1))"
unzip -l "$OUT" | tail -3
