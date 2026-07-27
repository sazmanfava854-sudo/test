let currentFiche = null;
let config = null;

const $ = (id) => document.getElementById(id);

const categoryLabels = {
  Income: 'درآمد',
  DutyNosazi: 'نوسازی',
  DutySenfi: 'صنفی',
  Unknown: 'نامشخص'
};

function branchFromRegion(regionStr) {
  const r = parseInt(regionStr, 10);
  if (Number.isNaN(r)) return null;
  if (r === 218) return 218;
  if (r >= 1 && r <= 12) return 200 + r;
  return null;
}

function applyBranchFromFiche(f) {
  const region = f.dutyRegion || f.incomeRegion;
  const branchId = region ? branchFromRegion(region) : null;
  if (!branchId) return;
  const match = config.branches.find(b => b.id === branchId);
  if (match) {
    $('branch').value = branchId;
    syncFundFromBranch();
  }
}

function syncFundFromBranch() {
  const branchId = parseInt($('branch').value);
  const item = config.branches.find(b => b.id === branchId);
  if (item) $('fund').value = item.fund;
}

function syncBranchFromFund() {
  const fund = parseInt($('fund').value);
  const item = config.branches.find(b => b.fund === fund);
  if (item) $('branch').value = item.id;
}

function getPayload(resetStatus) {
  return {
    fiche: currentFiche,
    branch: parseInt($('branch').value),
    fund: parseInt($('fund').value),
    docDate: $('docDate').value,
    resetStatus: !!resetStatus
  };
}

async function parseJsonResponse(res) {
  const text = await res.text();
  if (!text || !text.trim()) {
    throw new Error(`پاسخ خالی از سرور (HTTP ${res.status}). برنامه dotnet run را چک کنید و connection string را در appsettings.json تنظیم کنید.`);
  }
  try {
    return JSON.parse(text);
  } catch {
    throw new Error(`پاسخ نامعتبر از سرور (HTTP ${res.status}): ${text.slice(0, 300)}`);
  }
}

function formatDiagnostics(d) {
  if (!d) return '';
  const lines = [
    '--- Diagnostics ---',
    `Category: ${d.category || '-'}`,
    `Stage: ${d.stage || '-'}`,
    `ElapsedMs: ${d.elapsedMs ?? '-'}`,
    `PostUrl: ${d.postUrl || '-'}`,
    `HasWsAddressingHeader: ${d.hasWsAddressingHeader}`,
    `WsAddressingTo: ${d.wsAddressingTo || '-'}`,
    `EnvelopeStyle: ${d.envelopeStyle || '-'}`,
    `SoapAction: ${d.soapAction || '-'}`,
    `ContentType: ${d.contentType || '-'}`,
    `ProxyMode: ${d.proxyMode || '-'}`,
    `RequestBodyBytes: ${d.requestBodyBytes ?? '-'}`,
    `HttpStatusCode: ${d.httpStatusCode ?? '-'}`,
    `ResponseBodyBytes: ${d.responseBodyBytes ?? '-'}`
  ];
  if (d.likelyCause) lines.push(`LikelyCause: ${d.likelyCause}`);
  if (d.hint) lines.push(`Hint: ${d.hint}`);
  if (d.exceptionChain?.length) {
    lines.push('ExceptionChain:');
    d.exceptionChain.forEach((x) => lines.push(`  - ${x}`));
  }
  return lines.join('\n') + '\n';
}

function updateSendButton(f) {
  const btn = $('btnSend');
  if (!f) {
    btn.disabled = true;
    btn.title = 'ابتدا فیش را دریافت کنید';
    return;
  }
  if (f.existsInRayvarz) {
    btn.disabled = true;
    btn.title = 'فیش در رایورز تکراری است';
    return;
  }
  if (f.payable <= 0) {
    btn.disabled = true;
    btn.title = 'مبلغ قابل پرداخت صفر است';
    return;
  }
  if (!f.rows?.length) {
    btn.disabled = true;
    btn.title = 'ردیف IncmNo یافت نشد';
    return;
  }
  btn.disabled = false;
  btn.title = config?.dryRun
    ? 'DryRun فعال — SOAP ساخته می‌شود ولی به MSB POST نمی‌شود'
    : 'ارسال SaveDocument به MSB';
}

function showSendResult(data) {
  $('resultSection').hidden = false;
  let msg = `Success: ${data.success}\nMessage: ${data.message || '-'}\nDryRun: ${data.dryRun}\n`;
  if (data.pursuitDocNo) msg += `PursuitDocNo: ${data.pursuitDocNo}\n`;
  if (data.verifiedInRayvarz !== undefined) msg += `VerifiedInRayvarz: ${data.verifiedInRayvarz}\n`;
  if (data.docNotSentError) msg += `DocNotSent: ${data.docNotSentError}\n`;
  if (data.warning) msg += `Warning: ${data.warning}\n`;
  if (data.soapResponse) {
    const preview = data.soapResponse.length > 3500
      ? data.soapResponse.slice(0, 3500) + '\n...(truncated)'
      : data.soapResponse;
    msg += `\n--- SoapResponse ---\n${preview}\n`;
  }
  msg += formatDiagnostics(data.diagnostics);
  $('resultBox').textContent = msg;
}

function bnkAcntNoSource(f) {
  if (f.bnkAcntNoSource) return f.bnkAcntNoSource;
  if (f.category === 'Income') return 'کد نوسازی — Base_NosaziCode (فیش درآمد)';
  if (f.category === 'DutyNosazi' || f.category === 'DutySenfi') return 'کد نوسازی — Duty_Fiche.OtherFields (فیش نوسازی/صنفی)';
  return 'کد نوسازی';
}

function buildMappingRows(f) {
  const branch = config.branches.find(b => b.id === parseInt($('branch').value));
  const fund = $('fund').value;
  const docDate = $('docDate').value;
  const sourceId = config.sourceSystemId ?? null;

  return [
    { field: 'TransactionId (سند)', source: 'Income_Fiche / Duty_Fiche → NidFiche (GUID)', value: f.nidFiche || '-' },
    { field: 'SourceId (ردیف)', source: 'appsettings → Rayvarz:SourceSystemId (خالی = NULL)', value: sourceId ?? 'NULL' },
    { field: 'Id (ردیف)', source: 'همان NidFiche — شناسه تراکنش فیش', value: f.nidFiche || '-' },
    { field: 'RowDocNo (هدر)', source: 'FicheNo — فقط در DocumentItem', value: f.ficheNo },
    { field: 'RefRowDocNo (دیتیل)', source: 'appsettings RefRowDocNoInDetail (پیش‌فرض headerDocRow=1 مثل شهرسازی)', value: (config?.refRowDocNoInDetail === 'ficheNo' ? '(FicheNo)' : '1') },
    { field: 'Ref2', source: 'Income_Fiche.BillID / Duty_Fiche.BillID', value: f.billId || '-' },
    { field: 'Ref3', source: 'Income_Fiche.PaymentID / Duty_Fiche.PaymentID', value: f.paymentId || '-' },
    { field: 'BnkAcntNo (کد نوسازی)', source: bnkAcntNoSource(f), value: f.bnkAcntNo || '-' },
    { field: 'منطقه فیش (راهنما)', source: 'نوسازی/صنفی: OtherFields → منطقه | درآمد: Base_NosaziCode.CI_City', value: (f.dutyRegion || f.incomeRegion) ? `منطقه ${f.dutyRegion || f.incomeRegion} → branch=${branchFromRegion(f.dutyRegion || f.incomeRegion) || '?'}` : '(نامشخص)' },
    { field: 'Fund', source: 'انتخاب منطقه', value: fund },
    { field: 'branch', source: 'انتخاب شعبه', value: branch ? `${branch.id} — ${branch.name}` : $('branch').value },
    { field: 'DocDate / ActDate / Due', source: 'ورودی تاریخ سند (فرم)', value: docDate },
    { field: 'RowDate', source: 'BankPaymentDate → PaymentDate → PrintDate → ExportDate', value: f.rowDate || '-' },
    { field: 'DocTyp / DocTypDsc', source: 'نوع فیش', value: `${f.docTyp} — ${f.docDsc}` },
    { field: 'DocRow', source: 'شماره ردیف سند (ثابت ۱)', value: '1' },
    { field: 'IncmRow', source: 'شماره ردیف درآمد (۱، ۲، ۳…)', value: `${(f.rows || []).length} ردیف` },
    { field: 'Qty (دیتیل)', source: 'Payable — مبلغ کل فیش (در هر ردیف)', value: Number(f.payable).toLocaleString() },
    { field: 'Val (دیتیل)', source: 'Income_Calculation / Duty_FicheSub — مبلغ همان ردیف', value: (f.rows || []).map(r => Number(r.val).toLocaleString()).join(' + ') || '-' },
    { field: 'Bank', source: 'ConfirmBankCode — فقط اگر پرداخت شده', value: f.bankCode || '(خالی — NULL)' },
    { field: 'RefreconstructionNo', source: 'Sh_RequestInfo.NidWorkItem (درآمد)', value: f.refReconstructionNo || '(NULL)' }
  ];
}

function renderMappingTable(f) {
  const rows = buildMappingRows(f);
  $('mappingTable').innerHTML = rows.map(r => `
    <div class="mapping-row">
      <div class="mapping-field">${r.field}</div>
      <div class="mapping-source">${r.source}</div>
      <div class="mapping-value">${r.value}</div>
    </div>
  `).join('');
}

function renderFiche(f) {
  $('ficheSection').hidden = false;
  const statusClass = f.existsInRayvarz ? 'status-err' : (f.statusMessage === 'آماده ارسال' ? 'status-ok' : 'status-warn');

  $('ficheSummary').innerHTML = `
    <div class="stat-card">
      <span class="stat-label">شماره فیش</span>
      <span class="stat-value">${f.ficheNo}</span>
    </div>
    <div class="stat-card">
      <span class="stat-label">نوع</span>
      <span class="stat-value">${categoryLabels[f.category] || f.category}</span>
    </div>
    <div class="stat-card">
      <span class="stat-label">مبلغ قابل پرداخت</span>
      <span class="stat-value money">${Number(f.payable).toLocaleString()} ریال</span>
    </div>
    <div class="stat-card">
      <span class="stat-label">کد نوسازی (BnkAcntNo)</span>
      <span class="stat-value">${f.bnkAcntNo || '-'}</span>
      <span class="stat-hint">${bnkAcntNoSource(f)}</span>
    </div>
    <div class="stat-card">
      <span class="stat-label">وضعیت</span>
      <span class="stat-value"><span class="status-pill ${statusClass}">${f.statusMessage}</span></span>
    </div>
    <div class="stat-card">
      <span class="stat-label">در رایورز</span>
      <span class="stat-value">${f.existsInRayvarz ? 'بله — تکراری' : 'خیر'}</span>
    </div>
  `;

  renderMappingTable(f);

  const tbody = $('rowsTable').querySelector('tbody');
  tbody.innerHTML = '';
  (f.rows || []).forEach((r, i) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `<td>${i + 1}</td><td>${r.incmNo}</td><td>${r.incmRowDsc}</td><td>${Number(r.val).toLocaleString()}</td>`;
    tbody.appendChild(tr);
  });
}

async function init() {
  try {
    const res = await fetch('/api/config');
    config = await parseJsonResponse(res);
  } catch (e) {
    $('configBadge').textContent = 'خطا در اتصال به API — dotnet run را اجرا کنید';
    alert(e.message);
    return;
  }
  const badge = $('configBadge');
  const envLabel = 'رایورز ITC (safa_shahrsazi_v2)';
  badge.textContent = config.dryRun
    ? `${envLabel} | DryRun فعال — POST نمی‌زند | ${config.serviceUrl}`
    : `⚠ ${envLabel} | ارسال واقعی | ${config.serviceUrl}`;
  if (!config.dryRun) {
    badge.style.background = 'rgba(220, 53, 69, 0.35)';
  }

  const branchSel = $('branch');
  const fundSel = $('fund');
  config.branches.forEach(b => {
    const optBranch = document.createElement('option');
    optBranch.value = b.id;
    optBranch.textContent = `${b.id} — ${b.name}`;
    branchSel.appendChild(optBranch);

    const optFund = document.createElement('option');
    optFund.value = b.fund;
    optFund.textContent = `${b.fund} — ${b.name}`;
    fundSel.appendChild(optFund);
  });

  branchSel.onchange = () => { syncFundFromBranch(); if (currentFiche) renderMappingTable(currentFiche); };
  fundSel.onchange = () => { syncBranchFromFund(); if (currentFiche) renderMappingTable(currentFiche); };
  $('docDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  syncFundFromBranch();

  const today = new Date();
  $('docDate').value = `140${today.getFullYear() - 2020}/${String(today.getMonth() + 1).padStart(2, '0')}/${String(today.getDate()).padStart(2, '0')}`;

  if (config.uiVersion !== '3' || !config.features?.rayvarzPostTest) {
    console.warn('Backend قدیمی — دکمه‌های تست POST ممکن است 404 بدهند. git pull و dotnet run مجدد');
  }
  if (!config.features?.rayvarzPostMinimalSave) {
    const btn = $('btnPostMinimalSave');
    if (btn) btn.hidden = true;
  }
}

function showRayvarzTestWaiting(title) {
  $('resultSection').hidden = false;
  $('resultBox').textContent =
    `${title}\n\nدر حال اتصال به MSB…\n(ممکن است ۱۵ تا ۱۲۰ ثانیه طول بکشد — صبر کنید)`;
  $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function bindClick(id, handler) {
  const el = $(id);
  if (!el) {
    console.error(`دکمه #${id} در HTML یافت نشد — index.html را به‌روز کنید.`);
    return false;
  }
  el.addEventListener('click', handler);
  return true;
}

function setupEventHandlers() {
  const required = ['btnPing', 'btnPostTest', 'btnLoad', 'btnPreview', 'btnSend'];
  const missing = required.filter((id) => !$(id));
  if (missing.length) {
    alert(`فایل index.html قدیمی است یا ناقص.\nدکمه‌های گم‌شده: ${missing.join(', ')}\nاز شاخه rayvarz-resend دوباره کپی کنید.`);
  }

  bindClick('btnLoad', async () => {
  const value = $('identifierValue').value.trim();
  if (!value) return alert('شناسه فیش را وارد کنید');

  $('btnLoad').disabled = true;
  try {
    const res = await fetch('/api/fiche/load', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        identifierType: $('identifierType').value,
        identifierValue: value,
        branch: parseInt($('branch').value),
        docDate: $('docDate').value
      })
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || data.detail || data.title || `خطا (HTTP ${res.status})`);

    currentFiche = data;
    applyBranchFromFiche(data);
    renderFiche(data);
    const canSend = !data.existsInRayvarz && data.payable > 0 && data.rows?.length > 0;
    $('btnPreview').disabled = false;
    updateSendButton(data);
    $('resultSection').hidden = true;
    $('xmlSection').hidden = true;
  } catch (e) {
    alert(e.message);
    currentFiche = null;
    $('ficheSection').hidden = true;
    $('btnPreview').disabled = true;
    updateSendButton(null);
  } finally {
    $('btnLoad').disabled = false;
  }
  });

  bindClick('btnPreview', async () => {
  if (!currentFiche) return;
  $('btnPreview').disabled = true;
  try {
    const res = await fetch('/api/fiche/preview', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(getPayload(false))
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || data.detail || data.title || `خطا (HTTP ${res.status})`);
    $('xmlSection').hidden = false;
    $('xmlBox').textContent = data.xml;
    $('xmlSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnPreview').disabled = false;
  }
  });

  bindClick('btnSend', async () => {
  if (!currentFiche) return;
  if (currentFiche.existsInRayvarz) return alert('فیش تکراری است');
  if (!confirm(`ارسال فیش ${currentFiche.ficheNo} به رایورز؟`)) return;

  $('btnSend').disabled = true;
  try {
    const res = await fetch('/api/fiche/send', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(getPayload(true))
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || data.detail || data.title || `خطا (HTTP ${res.status})`);

    $('resultSection').hidden = false;
    showSendResult(data);

    if (data.dryRun) {
      alert('توجه: DryRun فعال است — چیزی به رایورز ارسال نشد، فقط XML ساخته شد.');
    } else if (data.success && data.verifiedInRayvarz === false) {
      alert('هشدار: ارسال تأیید نشد — فیش در incmdocsys نیست. پاسخ SOAP و DocNotSent را ببینید.');
    } else if (!data.success) {
      alert('ارسال ناموفق — Message و پاسخ SOAP را بررسی کنید.');
    } else if (data.success && data.verifiedInRayvarz) {
      alert('فیش در رایورز ثبت شد (VerifiedInRayvarz=true).');
    }

    if (data.previewXml || data.soapResponse) {
      $('xmlSection').hidden = false;
      $('xmlBox').textContent = data.soapResponse || data.previewXml;
    }
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnSend').disabled = false;
    updateSendButton(currentFiche);
  }
  });

  bindClick('btnPing', async () => {
  $('btnPing').disabled = true;
  showRayvarzTestWaiting('Rayvarz Ping (GET ?wsdl)');
  try {
    const res = await fetch('/api/rayvarz-ping');
    if (res.status === 404) {
      throw new Error('API /api/rayvarz-ping یافت نشد — برنامه را از شاخه rayvarz-resend دوباره build و dotnet run کنید.');
    }
    const data = await parseJsonResponse(res);
    $('resultSection').hidden = false;
    const head = [
      `Rayvarz Ping`,
      `Ok: ${data.ok}`,
      `Url: ${data.url}`,
      `StatusCode: ${data.statusCode ?? '-'}`,
      `ElapsedMs: ${data.elapsedMs}`,
      data.error ? `Error: ${data.error}` : '',
      data.inner ? `Inner: ${data.inner}` : '',
      data.hint ? `Hint: ${data.hint}` : '',
      data.warning ? `Warning: ${data.warning}` : ''
    ].filter(Boolean).join('\n');
    $('resultBox').textContent = head + '\n' + formatDiagnostics(data.diagnostics);
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
    if (!data.ok) alert('Ping ناموفق — اتصال TCP/HTTP برقرار نشد. POST Test را هم چک کنید.');
    else if (data.warning) alert(data.warning);
    else alert('Ping موفق — WSDL در دسترس است.');
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnPing').disabled = false;
  }
  });

  bindClick('btnPostTest', async () => {
  $('btnPostTest').disabled = true;
  showRayvarzTestWaiting('Rayvarz POST Test (بدون ثبت سند)');
  try {
    const res = await fetch('/api/rayvarz-post-test');
    if (res.status === 404) {
      throw new Error('API /api/rayvarz-post-test یافت نشد — نسخه قدیمی backend است. git pull rayvarz-resend و dotnet run مجدد.');
    }
    const data = await parseJsonResponse(res);
    $('resultSection').hidden = false;
    const head = [
      `Rayvarz POST Test (بدون ثبت سند)`,
      `Ok: ${data.ok}`,
      `Url: ${data.url}`,
      `StatusCode: ${data.statusCode ?? '-'}`,
      `ElapsedMs: ${data.elapsedMs}`,
      data.error ? `Error: ${data.error}` : '',
      data.inner ? `Inner: ${data.inner}` : '',
      data.hint ? `Hint: ${data.hint}` : '',
      data.bodyPreview ? `BodyPreview: ${data.bodyPreview}` : ''
    ].filter(Boolean).join('\n');
    $('resultBox').textContent = head + '\n' + formatDiagnostics(data.diagnostics);
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
    if (data.ok) alert('POST تا MSB رسید (پاسخ HTTP گرفت) — مسیر POST باز است.');
    else alert('POST هم قطع شد — مسیر POST به MSB بسته است؛ با IT مجوز IP/فایروال را چک کنید.');
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnPostTest').disabled = false;
  }
  });

  bindClick('btnPostMinimalSave', async () => {
  $('btnPostMinimalSave').disabled = true;
  showRayvarzTestWaiting('Rayvarz SaveDocument حداقلی (ممکن است Fault — سند واقعی نیست)');
  try {
    const res = await fetch('/api/rayvarz-post-minimal-save');
    if (res.status === 404) {
      throw new Error('API /api/rayvarz-post-minimal-save یافت نشد — git pull و dotnet run مجدد.');
    }
    const data = await parseJsonResponse(res);
    $('resultSection').hidden = false;
    const head = [
      `Rayvarz SaveDocument حداقلی`,
      `Ok: ${data.ok}`,
      `Url: ${data.url}`,
      `StatusCode: ${data.statusCode ?? '-'}`,
      `ElapsedMs: ${data.elapsedMs}`,
      data.error ? `Error: ${data.error}` : '',
      data.inner ? `Inner: ${data.inner}` : '',
      data.hint ? `Hint: ${data.hint}` : '',
      data.bodyPreview ? `BodyPreview: ${data.bodyPreview}` : ''
    ].filter(Boolean).join('\n');
    $('resultBox').textContent = head + '\n' + formatDiagnostics(data.diagnostics);
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
    if (data.ok) alert('SaveDocument حداقلی تا MSB رسید — اگر ارسال فیش واقعی reset می‌شود، محتوای فیش/WAF را بررسی کنید.');
    else alert('SaveDocument حداقلی هم reset شد — SoapVersion=soap11 و empty-header را در appsettings امتحان کنید.');
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnPostMinimalSave').disabled = false;
  }
  });
}

setupEventHandlers();
init();
