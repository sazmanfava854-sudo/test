#!/usr/bin/env bash
# آزاد کردن پورت — فقط dotnet
set -euo pipefail

PORT="${1:-5000}"

if command -v lsof >/dev/null 2>&1; then
  PIDS=$(lsof -ti tcp:"$PORT" -sTCP:LISTEN 2>/dev/null || true)
  if [[ -n "$PIDS" ]]; then
    for pid in $PIDS; do
      name=$(ps -p "$pid" -o comm= 2>/dev/null || echo unknown)
      if [[ "$name" == *dotnet* ]]; then
        echo "[Port $PORT] killing PID $pid ($name)"
        kill -9 "$pid" 2>/dev/null || true
      else
        echo "[Port $PORT] in use by $name (PID $pid) — not killed"
      fi
    done
  fi
fi

sleep 0.5

if command -v lsof >/dev/null 2>&1 && lsof -ti tcp:"$PORT" -sTCP:LISTEN >/dev/null 2>&1; then
  exit 1
fi

exit 0
