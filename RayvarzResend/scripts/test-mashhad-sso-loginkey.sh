#!/usr/bin/env bash
# تست مستقل loginKey مطابق سند «اتصال فنی به درگاه احراز هویت غیرحضوری» (صفحه ۲۰–۲۶)
# بدون اجرای RayvarzResend — فقط curl + SHA256(SecretKey + requestTime)
#
# apiName  = نام کاربری کاربردی برنامه (از پورتال شناسه شهروندی) — نه نام نمایشی مثل FinancialAssistant
# ClientId = شناسه کاربردی برنامه (همان ClientID ثبت‌شده)
# Secret   = SecretKey همان کاربردی برنامه
#
# Usage:
#   ./test-mashhad-sso-loginkey.sh <apiName> <ClientId> <SecretKey> [State]
#   MASHHAD_SSO_API_NAME=... MASHHAD_SSO_CLIENT_ID=... MASHHAD_SSO_SECRET=... ./test-mashhad-sso-loginkey.sh
#
# Optional: MASHHAD_SSO_BASE_URL  (default https://login.mashhad.ir)
#           MASHHAD_SSO_USER_TYPE (default 0)
#           MASHHAD_SSO_DOMAIN_ID (default 0)
#           MASHHAD_SSO_HASH_UPPER=1  → apiSecret/Hash با حروف بزرگ (پیش‌فرض: lower)

set -euo pipefail

BASE="${MASHHAD_SSO_BASE_URL:-https://login.mashhad.ir}"
API_NAME="${1:-${MASHHAD_SSO_API_NAME:-}}"
CLIENT_ID="${2:-${MASHHAD_SSO_CLIENT_ID:-}}"
SECRET="${3:-${MASHHAD_SSO_SECRET:-}}"
# SSO 1.0.2: ClientId in loginKey body = apiName (username); omit ClientId arg to use apiName
if [[ -z "$CLIENT_ID" ]]; then
  CLIENT_ID="$API_NAME"
  echo "ClientId not set — using ApiName per SSO doc 1.3.2."
fi
STATE="${4:-test}"
USER_TYPE="${MASHHAD_SSO_USER_TYPE:-0}"
DOMAIN_ID="${MASHHAD_SSO_DOMAIN_ID:-0}"

if [[ -z "$API_NAME" || -z "$CLIENT_ID" || -z "$SECRET" ]]; then
  echo "Usage: $0 <apiName> <ClientId> <SecretKey> [State]" >&2
  echo "" >&2
  echo "  apiName  = نام کاربری کاربردی برنامه (ردیف ۱ جدول هدر — از مدیر SSO)" >&2
  echo "  ClientId = شناسه کاربردی برنامه" >&2
  echo "  Secret   = SecretKey همان کاربردی (در چت/تیکت نفرستید)" >&2
  exit 1
fi

if ! command -v curl >/dev/null; then
  echo "curl is required." >&2
  exit 1
fi

echo "== 1) GET getCurrentTime =="
TIME_JSON=$(curl -sS --max-time 30 "${BASE}/api/Authentication/getCurrentTime")
echo "$TIME_JSON"

REQUEST_TIME=""
if command -v jq >/dev/null; then
  REQUEST_TIME=$(echo "$TIME_JSON" | jq -r '.Data // empty')
else
  REQUEST_TIME=$(echo "$TIME_JSON" | sed -n 's/.*"Data"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' | head -1)
fi

if [[ -z "$REQUEST_TIME" ]]; then
  echo "Failed to parse requestTime from getCurrentTime." >&2
  exit 2
fi

echo ""
echo "requestTime: $REQUEST_TIME"

RAW="${SECRET}${REQUEST_TIME}"
if command -v openssl >/dev/null; then
  HASH_LOWER=$(printf '%s' "$RAW" | openssl dgst -sha256 | awk '{print $2}')
else
  HASH_LOWER=$(printf '%s' "$RAW" | sha256sum | awk '{print $1}')
fi

if [[ "${MASHHAD_SSO_HASH_UPPER:-0}" == "1" ]]; then
  HASH=$(echo "$HASH_LOWER" | tr '[:lower:]' '[:upper:]')
else
  HASH="$HASH_LOWER"
fi

if [[ "${MASHHAD_SSO_HASH_UPPER:-0}" == "1" ]]; then
  echo "apiSecret/Hash = SHA256(SecretKey+requestTime) hex upper"
else
  echo "apiSecret/Hash = SHA256(SecretKey+requestTime) hex lower"
fi
echo "(length ${#HASH})"

BODY=$(cat <<EOF
{"Time":"${REQUEST_TIME}","Hash":"${HASH}","ClientId":"${CLIENT_ID}","State":"${STATE}","UserType":${USER_TYPE},"DomainID":${DOMAIN_ID}}
EOF
)

echo ""
echo "== 2) POST loginKey =="
echo "Headers: apiName=$API_NAME, requestTime=$REQUEST_TIME, apiSecret=<hash>"
RESP=$(curl -sS --max-time 30 -X POST "${BASE}/api/Authentication/loginKey" \
  -H "apiName: ${API_NAME}" \
  -H "requestTime: ${REQUEST_TIME}" \
  -H "apiSecret: ${HASH}" \
  -H "Content-Type: application/json" \
  -d "$BODY")
echo "$RESP"

if command -v jq >/dev/null; then
  CODE=$(echo "$RESP" | jq -r '.ErrorCode // empty')
  MSG=$(echo "$RESP" | jq -r '.ErrorMessage // empty')
  LK=$(echo "$RESP" | jq -r '.Data.loginKey // empty')
  if [[ "$CODE" == "0" && -n "$LK" ]]; then
    echo ""
    echo "OK — loginKey received. Next: ${BASE%/}/Authentication/Start/${LK}"
    exit 0
  fi
  echo ""
  echo "Result: ErrorCode=$CODE ${MSG:+$MSG}"
  if [[ "$CODE" == "403" ]]; then
    echo "403 → هدر apiName / ClientId / apiSecret با ثبت پورتال برای این کاربردی برنامه یکی نیست (قبل از هش هم رد می‌شود)."
  fi
  exit 3
fi

exit 0
