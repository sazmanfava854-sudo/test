#!/usr/bin/env bash
# Zip دیباگ: Debug build + PDB + appsettings با لاگ SSO
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_ZIP="${1:-$ROOT/../RayvarzResend-26-debug.zip}"
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

export PATH="${HOME}/.dotnet:${PATH}"

echo "Publishing win-x64 Debug (PDB included)..."
dotnet publish "$ROOT/RayvarzResend.Web/RayvarzResend.Web.csproj" \
  -c Debug -r win-x64 --self-contained false \
  -p:DebugType=portable \
  -o "$STAGE/RayvarzResend" /nologo

find "$STAGE/RayvarzResend" -maxdepth 1 -type f -name 'appsettings.*.json' -delete 2>/dev/null || true

cp "$ROOT/RayvarzResend.Web/appsettings.json" "$STAGE/RayvarzResend/appsettings.json"

STAGE="$STAGE" python3 -c "
import json, os
from pathlib import Path
stage = os.environ['STAGE']
p = Path(stage) / 'RayvarzResend' / 'appsettings.json'
data = json.loads(p.read_text(encoding='utf-8'))
data.setdefault('Logging', {}).setdefault('LogLevel', {})['Default'] = 'Debug'
data['Logging']['LogLevel']['RayvarzResend.Web'] = 'Debug'
data['Logging']['LogLevel']['RayvarzResend.Web.Services.MashhadSsoApiClient'] = 'Debug'
data['Logging']['LogLevel']['RayvarzResend.Web.Services.SsoAuthService'] = 'Debug'
sh = data.setdefault('Auth', {}).setdefault('Sso', {})
sh['DebugSigning'] = True
sh['ApiSecretConcatOrder'] = 'SecretTime'
p.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
"

for f in test-mashhad-sso-loginkey.ps1 sso-loginkey-print-and-send.ps1 sso-loginkey-curl-only.ps1 \
  run-test-mashhad-sso-loginkey.cmd run-sso-loginkey-print-and-send.cmd run-sso-loginkey-curl-only.cmd; do
  cp "$ROOT/scripts/$f" "$STAGE/RayvarzResend/$f"
done

cat > "$STAGE/RayvarzResend/start-debug.bat" << 'EOF'
@echo off
cd /d "%~dp0"
set ASPNETCORE_ENVIRONMENT=Development
echo RayvarzResend v26 DEBUG (PDB + verbose SSO logs)
echo Auth:Sso:DebugSigning=true in appsettings.json
echo.
echo SSO debug URLs (after start):
echo   http://localhost:5088/api/auth/sso-outbound-map
echo   http://localhost:5088/api/auth/sso-signing-preview
echo   http://localhost:5088/api/auth/sso-loginkey-check
echo   http://localhost:5088/api/auth/sso-loginkey-probe
echo.
RayvarzResend.Web.exe --urls http://0.0.0.0:5088
EOF

cat > "$STAGE/RayvarzResend/DEBUG-README.txt" << 'EOF'
RayvarzResend v26 — نسخه DEBUG (غیر پابلیش)

- Build: Debug + فایل .pdb (اتصال Visual Studio / dnSpy)
- appsettings: LogLevel=Debug, Auth:Sso:DebugSigning=true
- start-debug.bat را اجرا کنید (نه start.bat نسخه Release)

APIهای SSO (مرورگر یا curl):
  GET /api/auth/sso-outbound-map
  GET /api/auth/sso-signing-preview
  GET /api/auth/sso-loginkey-check
  GET /api/auth/sso-loginkey-probe
  GET /api/auth/sso-loginkey-probe?apiName=...&clientId=...&secret=...  (فقط دیباگ محلی)

ورود با دیباگ:
  /auth/login?debug=1

اسکریپت curl (کنار exe):
  run-sso-loginkey-curl-only.cmd -SecretKey ... -ProbeSwaps
EOF

rm -f "$OUT_ZIP"
(cd "$STAGE" && zip -r -q "$OUT_ZIP" RayvarzResend)
echo "Created $OUT_ZIP"
unzip -l "$OUT_ZIP" | rg '\.pdb|DEBUG-README|start-debug' || true
