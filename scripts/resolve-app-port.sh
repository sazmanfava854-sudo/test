#!/usr/bin/env bash
# انتخاب پورت آزاد — فقط عدد پورت را چاپ می‌کند
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PORTS=(5000 5280 5080 5180 5050)

is_listening() {
  local port="$1"
  if command -v lsof >/dev/null 2>&1; then
    lsof -ti tcp:"$port" -sTCP:LISTEN >/dev/null 2>&1
    return $?
  fi
  if command -v ss >/dev/null 2>&1; then
    ss -ltn "( sport = :$port )" | grep -q LISTEN
    return $?
  fi
  return 1
}

for port in "${PORTS[@]}"; do
  if ! is_listening "$port"; then
    echo "$port"
    exit 0
  fi
  echo "پورت $port اشغال است — تلاش برای آزادسازی..." >&2
  bash "$ROOT_DIR/free-port.sh" "$port" >&2 || true
  sleep 0.4
  if ! is_listening "$port"; then
    echo "پورت $port آزاد شد." >&2
    echo "$port"
    exit 0
  fi
done

echo "هیچ پورت آزادی یافت نشد." >&2
exit 1
