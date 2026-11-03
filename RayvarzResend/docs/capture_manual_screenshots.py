#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Capture UI screenshots for user manual via static wwwroot + mocked API."""

from __future__ import annotations

import json
import os
import threading
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import urlparse

WWW = Path(__file__).resolve().parents[1] / "RayvarzResend.Web" / "wwwroot"
OUT = Path("/opt/cursor/artifacts/manual-screenshots")
PORT = 8765

MOCK_USER = {
    "id": 1,
    "username": "demo.user",
    "domain": "demo-user",
    "displayName": "کاربر نمونه",
    "firstName": "کاربر",
    "lastName": "نمونه",
    "isAdmin": False,
    "isActive": True,
    "canAccessUnsentFiches": True,
    "canAccessInstallment": True,
    "canAccessInstallmentCheckDate": True,
    "canAccessFicheDateChange": True,
    "canAccessBankInquiryConfirm": True,
    "canAccessCostCenter": True,
}

MOCK_CONFIG = {
    "releaseVersion": 58,
    "releaseDisplayName": "نسخه آخر",
    "dryRun": True,
    "branches": [
        {"id": 201, "name": "منطقه 1", "fund": 200201012},
        {"id": 202, "name": "منطقه 2", "fund": 200202012},
        {"id": 102, "name": "شعبه مرکز", "fund": 0},
    ],
    "installment": {"dryRun": True},
    "installmentCheckDate": {"dryRun": True},
    "ficheDateChange": {
        "dryRun": True,
        "defaultStatus": 1,
        "statusLabels": {"1": "صدور قطعی", "4": "ابطال"},
        "dutyStatusLabels": {"1": "تأیید", "2": "ابطال"},
    },
    "costCenter": {"dryRun": True},
    "bankInquiryConfirm": {"dryRun": True, "serviceConfigured": True},
    "tahator": {"dryRun": True},
    "unsentBatch": {"chunkSize": 4, "sendDelayMs": 2000},
}


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(WWW), **kwargs)

    def log_message(self, fmt, *args):
        pass

    def do_GET(self):
        path = urlparse(self.path).path
        if path == "/api/auth/me":
            return self._json(200, MOCK_USER)
        if path == "/api/auth/mode":
            return self._json(
                200,
                {
                    "allowHybridLocalLogin": True,
                    "preferSsoLogin": False,
                    "loginPath": "/login.html",
                },
            )
        if path == "/api/config":
            return self._json(200, MOCK_CONFIG)
        if path.startswith("/api/fiche-date/account-groups"):
            return self._json(200, {"items": []})
        if path.startswith("/api/rule/"):
            return self._json(200, {"open": False, "state": {}})
        return super().do_GET()

    def do_POST(self):
        path = urlparse(self.path).path
        if path.startswith("/api/"):
            return self._json(200, {"ok": True})
        self.send_error(404)

    def _json(self, code: int, obj):
        body = json.dumps(obj, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)


def run_server():
    httpd = ThreadingHTTPServer(("127.0.0.1", PORT), Handler)
    threading.Thread(target=httpd.serve_forever, daemon=True).start()
    return httpd


def capture():
    from playwright.sync_api import sync_playwright

    OUT.mkdir(parents=True, exist_ok=True)
    base = f"http://127.0.0.1:{PORT}"
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(viewport={"width": 1440, "height": 900}, locale="fa-IR")
        page = context.new_page()

        page.goto(f"{base}/login.html", wait_until="networkidle")
        page.wait_for_timeout(400)
        page.screenshot(path=str(OUT / "01-login.png"), full_page=True)
        print(OUT / "01-login.png")

        page.goto(f"{base}/management/", wait_until="networkidle")
        page.wait_for_selector("#hubGrid:not([hidden])", timeout=15000)
        page.wait_for_timeout(400)
        page.screenshot(path=str(OUT / "02-management-hub.png"), full_page=True)
        print(OUT / "02-management-hub.png")

        tabs = [
            ("03-rayvarz-single.png", f"{base}/?tab=unsent", None),
            ("04-rayvarz-bulk.png", f"{base}/?tab=unsent", "bulk"),
            ("05-installment-single.png", f"{base}/?tab=installment", "single"),
            ("06-installment-excel.png", f"{base}/?tab=installment", "excel"),
            ("07-check-date.png", f"{base}/?tab=checkDate", None),
            ("08-fiche-date.png", f"{base}/?tab=ficheDate", None),
            ("09-bank-inquiry.png", f"{base}/?tab=bankInquiry", None),
            ("10-cost-center.png", f"{base}/?tab=costCenter", None),
        ]

        for filename, url, submode in tabs:
            page.goto(url, wait_until="networkidle")
            page.wait_for_selector("body.app-authenticated", timeout=15000)
            page.wait_for_timeout(600)
            if submode == "bulk":
                page.locator('[data-rayvarz-mode="bulk"]').click()
                page.wait_for_selector("#rayvarzBulkPanel:not([hidden])", timeout=5000)
            elif submode == "excel":
                page.locator('[data-installment-mode="excel"]').click()
                page.wait_for_selector("#installmentExcelPanel:not([hidden])", timeout=5000)
            elif submode == "single" and "installment" in filename:
                page.locator('[data-installment-mode="single"]').click()
                page.wait_for_selector("#installmentSinglePanel:not([hidden])", timeout=5000)
            page.wait_for_timeout(400)
            path = OUT / filename
            page.screenshot(path=str(path), full_page=True)
            print(path)

        browser.close()


if __name__ == "__main__":
    os.chdir(WWW)
    srv = run_server()
    try:
        capture()
    finally:
        srv.shutdown()
