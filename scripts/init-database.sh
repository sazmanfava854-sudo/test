#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DB_DIR="$ROOT_DIR/database"
SERVER="${SQL_SERVER:-localhost}"

if ! command -v sqlcmd >/dev/null 2>&1; then
  echo "❌ sqlcmd یافت نشد. SQL Server Command Line Tools را نصب کنید."
  echo "   یا اسکریپت‌های database/ را دستی در SSMS اجرا کنید."
  exit 1
fi

echo "🗄️  ایجاد دیتابیس روی سرور: $SERVER"

run_script() {
  local file="$1"
  local db="${2:-}"
  echo "   → $(basename "$file")"
  if [ -n "$db" ]; then
    sqlcmd -S "$SERVER" -d "$db" -b -i "$file"
  else
    sqlcmd -S "$SERVER" -b -i "$file"
  fi
}

run_script "$DB_DIR/01_CreateDatabase.sql"
run_script "$DB_DIR/02_Tables.sql" "HRPerformanceDB"
run_script "$DB_DIR/03_ForeignKeys.sql" "HRPerformanceDB"
run_script "$DB_DIR/04_Indexes.sql" "HRPerformanceDB"
run_script "$DB_DIR/05_Views.sql" "HRPerformanceDB"
run_script "$DB_DIR/06_StoredProcedures.sql" "HRPerformanceDB"
run_script "$DB_DIR/07_Triggers.sql" "HRPerformanceDB"
run_script "$DB_DIR/08_SeedData.sql" "HRPerformanceDB"

echo "✅ دیتابیس با موفقیت آماده شد."
