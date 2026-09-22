#!/usr/bin/env bash
# یک Zip تحویل: فقط پوشه RayvarzResend با exe خودکفا + یک appsettings.json
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_ZIP="${1:-$ROOT/../RayvarzResend-25.zip}"
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

export PATH="${HOME}/.dotnet:${PATH}"

echo "Publishing win-x64 (framework-dependent — .NET 8 روی سرور)..."
dotnet publish "$ROOT/RayvarzResend.Web/RayvarzResend.Web.csproj" \
  -c Release -r win-x64 --self-contained false \
  -o "$STAGE/RayvarzResend" /nologo

# فقط یک appsettings.json کنار exe
find "$STAGE/RayvarzResend" -maxdepth 1 -type f -name 'appsettings.*.json' -delete 2>/dev/null || true

cp "$ROOT/RayvarzResend.Web/appsettings.json" "$STAGE/RayvarzResend/appsettings.json"

cat > "$STAGE/RayvarzResend/start.bat" << 'EOF'
@echo off
cd /d "%~dp0"
echo RayvarzResend v25 — نسخه آخر
echo Settings: %cd%\appsettings.json
echo.
RayvarzResend.Web.exe --urls http://0.0.0.0:5088
EOF

cat > "$STAGE/RayvarzResend/README.txt" << 'EOF'
RayvarzResend v25 — نسخه آخر (تهاتر + Accounting_Doc)

نصب روی سرور ویندوز
--------------------
1) Zip را باز کنید. فقط یک پوشه دارید: RayvarzResend
2) ConnectionStrings را در همین فایل ویرایش کنید:
     RayvarzResend\appsettings.json
   فقط همین یک فایل تنظیمات وجود دارد.
   appsettings.Production.json را اگر از قبل دارید حذف کنید.
3) .NET 8 Runtime/Hosting روی سرور کافی است (SDK لازم نیست).
4) start.bat را اجرا کنید (یا RayvarzResend.Web.exe)
5) مرورگر: http://localhost:5088
6) GET /api/config
     releaseVersion = 25
     accountingDoc.dryRun = false
     dryRun = false

Accounting_DocHeader / Accounting_DocDetails بعد از ارسال موفق به رایورز
در همین نسخه ثبت می‌شود (اگر DryRun=false باشد).

سورس روی GitHub است — داخل Zip نیست:
https://github.com/sazmanfava854-sudo/test/tree/cursor/tahator-accounting-doc-ffcb
EOF

# Zip با root = RayvarzResend (یک پوشه)
rm -f "$OUT_ZIP"
(cd "$STAGE" && zip -r -q "$OUT_ZIP" RayvarzResend)
echo "Created $OUT_ZIP ($(du -h "$OUT_ZIP" | cut -f1))"
echo "--- appsettings (must be exactly one) ---"
unzip -l "$OUT_ZIP" | rg 'appsettings' || true
echo "--- exe ---"
unzip -l "$OUT_ZIP" | rg 'RayvarzResend.Web.exe|start.bat|README.txt' || true
