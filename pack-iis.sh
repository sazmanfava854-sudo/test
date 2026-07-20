#!/usr/bin/env bash
# ZIP آماده IIS — win-x64 publish + wwwroot
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="${1:-2.9.5-iis}"
STAGE="/tmp/HRPerformance-${VERSION}"
OUT="$ROOT/releases/HRPerformance-${VERSION}.zip"

echo "==> Building UI ..."
bash "$ROOT/scripts/build-ui.sh"

rm -rf "$STAGE"
mkdir -p "$STAGE"

echo "==> dotnet publish (win-x64, IIS) ..."
dotnet publish "$ROOT/src/HRPerformance.API/HRPerformance.API.csproj" \
  -c Release \
  -r win-x64 \
  --self-contained false \
  -o "$STAGE" \
  -v minimal

mkdir -p "$STAGE/logs" "$STAGE/uploads"
cp "$ROOT/src/HRPerformance.API/appsettings.Production.example.json" "$STAGE/appsettings.Production.json"
cp "$ROOT/iis-fix-permissions.ps1" "$ROOT/iis-fix-permissions.bat" "$STAGE/" 2>/dev/null || true
cp "$ROOT/iis-bind-site-5050.ps1" "$ROOT/iis-bind-site-5050.bat" "$STAGE/" 2>/dev/null || true
cp "$ROOT/LOGIN.txt" "$STAGE/" 2>/dev/null || true
cp -r "$ROOT/database" "$STAGE/"

cat > "$STAGE/IIS-SETUP.txt" << 'EOF'
HR Performance — استقرار IIS
============================

■ Physical Path در IIS = همین پوشه (جایی که HRPerformance.API.dll و web.config هست)

■ محیط IIS
  web.config → ASPNETCORE_ENVIRONMENT = Production
  بنابراین appsettings.Development.json خوانده نمی‌شود.

■ کجا تنظیمات را وارد کنید؟
  ┌─────────────────────────────────────────────────────────────┐
  │ 1) appsettings.Production.json  ← پیشنهادی برای IIS       │
  │    (همین پوشه، کنار web.config)                             │
  │                                                             │
  │ 2) appsettings.json          ← مقادیر پایه / fallback     │
  │                                                             │
  │ 3) web.config                ← Connection String اختیاری    │
  │    environmentVariables / ConnectionStrings__DefaultConnection │
  └─────────────────────────────────────────────────────────────┘

■ appsettings.Development.json
  فقط برای توسعه لوکال (start-local.bat) است.
  روی IIS استفاده نمی‌شود مگر ASPNETCORE_ENVIRONMENT=Development بگذارید.

■ فیلدهای مهم در appsettings.Production.json
  ConnectionStrings:DefaultConnection  → SQL Server برنامه
  HrIntegration:Server / UserId / Password → اتصال MIS
  Jwt:Key                              → کلید امن (۳۲+ کاراکتر)
  Cors:Origins                         → آدرس سایت IIS

■ دیتابیس
  database\01 تا 16 را در SSMS اجرا کنید.

■ دسترسی پوشه (PowerShell Admin)
  iis-fix-permissions.bat

■ پورت ثابت 5050 (بدون انتخاب پویا)
  در IIS پورت از resolve-app-port استفاده نمی‌شود.
  PowerShell Admin:
    iis-bind-site-5050.bat
  یا در IIS Manager → Site → Bindings → http → Port = 5050

  Cors در appsettings.Production.json:
    http://localhost:5050
    http://YOUR_SERVER:5050

■ تست
  http://YOUR_SERVER:5050/api/health
  http://YOUR_SERVER:5050/          ← UI (wwwroot)

لاگین: admin / Admin@123
EOF

cat > "$STAGE/VERSION.txt" << EOF
HR Performance — IIS Ready
Version: ${VERSION}
Build Date: $(date +%Y-%m-%d)
Platform: win-x64 + .NET 8 Hosting Bundle

Edit: appsettings.Production.json (same folder as web.config)
EOF

if [ ! -f "$STAGE/wwwroot/index.html" ]; then
  echo "ERROR: wwwroot missing in publish output"
  exit 1
fi

mkdir -p "$ROOT/releases"
rm -f "$OUT"
(cd /tmp && zip -r -q "$OUT" "HRPerformance-${VERSION}")

echo "Created: $OUT ($(du -h "$OUT" | cut -f1))"
unzip -l "$OUT" | grep -E 'web.config|appsettings|wwwroot/index' | head -12
