#!/usr/bin/env bash
# نسخه فقط-اجرا — بدون سورس، بدون sln، بدون Build روی ویندوز
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.6.0-run}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"
PUBLISH_DIR="$STAGE/app"

rm -rf "$STAGE"
mkdir -p "$STAGE"

echo "==> Publishing app..."
dotnet publish "$ROOT/src/HRPerformance.API/HRPerformance.API.csproj" \
  -c Release \
  -o "$PUBLISH_DIR" \
  --no-self-contained \
  -v minimal

cp "$ROOT/app-CONNECTION_SETUP.txt" "$STAGE/"
cp "$ROOT/app-CONNECTION_SETUP.txt" "$PUBLISH_DIR/"
cp -r "$ROOT/database" "$STAGE/"
cp "$ROOT/start-local.bat" "$ROOT/start.bat" "$ROOT/run-api.bat" "$STAGE/"

cat > "$STAGE/README-LOCAL.txt" << 'EOF'
HR Performance — نسخه اجرا (بدون Build)

⚡ این ZIP سورس کد ندارد — هیچ Buildی لازم نیست.

پیش‌نیاز: .NET 8 Runtime (یا SDK) + SQL Server

1) database/01 تا 11 را روی SQL Server اجرا کنید
2) app/appsettings.Development.json — پسورد SQL را تنظیم کنید
3) دوبار کلیک: start-local.bat
4) http://localhost:5000/api/health  (باید Connected باشد)
5) لاگین: admin / Admin@123

❌ dotnet build یا HRPerformance.sln باز نکنید — لازم نیست.
EOF

cat > "$STAGE/START_HERE.txt" << 'EOF'
فقط start-local.bat را اجرا کنید.
Build لازم نیست.
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance — RUN ONLY (no source)
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
.NET 8 Runtime required

شامل: app/ + database/ + start-local.bat
بدون: src/, sln, frontend — بدون Build
EOF

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r -q "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT ($(du -h "$OUT" | cut -f1))"
unzip -l "$OUT" | tail -3
