#!/usr/bin/env bash
# Zip تحویل: یک appsettings در deploy/win-x64 + سورس بدون bin/obj/publish
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_ZIP="${1:-$ROOT/../RayvarzResend-25.zip}"
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

export PATH="${HOME}/.dotnet:${PATH}"

echo "Publishing win-x64..."
dotnet publish "$ROOT/RayvarzResend.Web/RayvarzResend.Web.csproj" \
  -c Release -r win-x64 --self-contained false \
  -o "$STAGE/deploy/win-x64" /nologo

# حذف فایل‌های appsettings اضافی کنار exe (فقط appsettings.json بماند)
find "$STAGE/deploy/win-x64" -maxdepth 1 -type f -name 'appsettings.*.json' -delete 2>/dev/null || true

echo "Staging source (no bin/obj/publish)..."
mkdir -p "$STAGE/source"
tar -C "$ROOT" -cf - \
  --exclude='./bin' --exclude='./obj' --exclude='./publish' \
  --exclude='**/bin' --exclude='**/obj' --exclude='**/publish' \
  . | tar -xf - -C "$STAGE/source"

# سورس: فقط یک appsettings در RayvarzResend.Web
find "$STAGE/source" -name 'appsettings.*.json' ! -name 'appsettings.json' -delete 2>/dev/null || true
rm -f "$STAGE/source/RayvarzResend.Web/appsettings.Development.json.example" 2>/dev/null || true

cat > "$STAGE/DEPLOY-README.txt" << 'EOF'
RayvarzResend — نسخه آخر

=== سرور ویندوز (فقط این پوشه) ===
  deploy/win-x64/
    RayvarzResend.Web.exe
    appsettings.json   ← تنها فایل تنظیمات (ConnectionStrings و DryRun را اینجا ویرایش کنید)

appsettings.Production.json استفاده نمی‌شود — اگر روی سرور دارید، حذف کنید.

=== توسعه ===
  source/RayvarzResend.Web/appsettings.json

تأیید: GET /api/config → accountingDoc.dryRun = false
EOF

rm -f "$OUT_ZIP"
(cd "$STAGE" && zip -r -q "$OUT_ZIP" DEPLOY-README.txt deploy source)
echo "Created $OUT_ZIP"
unzip -l "$OUT_ZIP" | rg 'appsettings' || true
