#!/usr/bin/env bash
# Zip سورس غیرپابلیش — بدون bin/obj/publish
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_ZIP="${1:-$ROOT/../RayvarzResend-source.zip}"
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

DEST="$STAGE/RayvarzResend"
mkdir -p "$DEST"
(
  cd "$ROOT"
  find . -type f \
    ! -path './bin/*' \
    ! -path './obj/*' \
    ! -path '*/bin/*' \
    ! -path '*/obj/*' \
    ! -path './publish/*' \
    ! -path '*/publish/*' \
    ! -path './.vs/*' \
    ! -name '*.user' \
    -print0
) | while IFS= read -r -d '' rel; do
  dest_file="$DEST/${rel#./}"
  mkdir -p "$(dirname "$dest_file")"
  cp "$ROOT/${rel#./}" "$dest_file"
done

cp "$ROOT/DELIVERY-SOURCE.md" "$DEST/DELIVERY-SOURCE.md"

rm -f "$OUT_ZIP"
(cd "$STAGE" && zip -r -q "$OUT_ZIP" RayvarzResend)
echo "Created $OUT_ZIP ($(du -h "$OUT_ZIP" | cut -f1))"
echo "--- login ---"
unzip -p "$OUT_ZIP" RayvarzResend/RayvarzResend.Web/wwwroot/login.html | grep -F 'ورود با کد ملی' || true
echo "--- files ---"
unzip -l "$OUT_ZIP" | grep -E 'login.html|app.js|TahatorResendService.cs|AccountingDocWriter.cs|appsettings.json|DELIVERY-SOURCE.md' || true
