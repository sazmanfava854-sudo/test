#!/usr/bin/env bash
# آزاد کردن پورت 5050 در صورت اشغال بودن (معمولاً نمونه قبلی dotnet run)
set -euo pipefail

PORT=5050
PIDS=""

if command -v lsof >/dev/null 2>&1; then
  PIDS="$(lsof -ti "tcp:${PORT}" -sTCP:LISTEN 2>/dev/null || true)"
elif command -v fuser >/dev/null 2>&1; then
  PIDS="$(fuser "${PORT}/tcp" 2>/dev/null | tr ' ' '\n' | grep -E '^[0-9]+$' || true)"
else
  echo "lsof/fuser یافت نشد؛ بررسی پورت ${PORT} رد شد."
  exit 0
fi

if [[ -z "${PIDS// }" ]]; then
  exit 0
fi

for pid in $PIDS; do
  name="$(ps -p "$pid" -o comm= 2>/dev/null || echo unknown)"
  echo "[Port ${PORT}] در حال استفاده توسط PID ${pid} (${name}) - در حال آزادسازی..."
  if kill -9 "$pid" 2>/dev/null; then
    echo "پروسه ${pid} متوقف شد."
  else
    echo "خطا: امکان توقف PID ${pid} نیست."
    exit 1
  fi
done

sleep 1
