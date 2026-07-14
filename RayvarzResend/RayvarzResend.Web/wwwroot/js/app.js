let currentFiche = null;
let config = null;

const $ = (id) => document.getElementById(id);

const categoryLabels = {
  Income: 'درآمد',
  DutyNosazi: 'نوسازی',
  DutySenfi: 'صنفی',
  Unknown: 'نامشخص'
};

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
  const sourceId = config.sourceSystemId || '11111';

  return [
    { field: 'TransactionId (سند)', source: 'Income_Fiche / Duty_Fiche → NidFiche (GUID)', value: f.nidFiche || '-' },
    { field: 'SourceId (ردیف)', source: 'appsettings → Rayvarz:SourceSystemId', value: sourceId },
    { field: 'Id (ردیف)', source: 'همان NidFiche — شناسه تراکنش فیش', value: f.nidFiche || '-' },
    { field: 'RowDocNo (هدر)', source: 'FicheNo — فقط در DocumentItem', value: f.ficheNo },
    { field: 'RefRowDocNo (دیتیل)', source: 'DocRow هدر (۱) — ارجاع به ردیف سند', value: '1' },
    { field: 'Ref2', source: 'Income_Fiche.BillID / Duty_Fiche.BillID', value: f.billId || '-' },
    { field: 'Ref3', source: 'Income_Fiche.PaymentID / Duty_Fiche.PaymentID', value: f.paymentId || '-' },
    { field: 'BnkAcntNo (کد نوسازی)', source: bnkAcntNoSource(f), value: f.bnkAcntNo || '-' },
    { field: 'منطقه فیش (راهنما)', source: 'OtherFields → منطقه (فقط نوسازی/صنفی)', value: f.dutyRegion ? `منطقه ${f.dutyRegion} → branch=${200 + parseInt(f.dutyRegion)}` : '(درآمد — از شعبه فرم)' },
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
  const envLabel = config.isProduction ? 'وب‌سرویس اصلی (Production)' : 'وب‌سرویس تست';
  badge.textContent = config.dryRun
    ? `${envLabel} | DryRun فعال — POST نمی‌زند | ${config.serviceUrl}`
    : `⚠ ${envLabel} | ارسال واقعی | ${config.serviceUrl}`;
  if (config.isProduction && !config.dryRun) {
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
}

$('btnLoad').onclick = async () => {
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
    if (data.dutyRegion) {
      const branchId = 200 + parseInt(data.dutyRegion, 10);
      const match = config.branches.find(b => b.id === branchId);
      if (match) {
        $('branch').value = branchId;
        syncFundFromBranch();
      }
    }
    renderFiche(data);
    const canSend = !data.existsInRayvarz && data.payable > 0 && data.rows?.length > 0;
    $('btnPreview').disabled = false;
    $('btnSend').disabled = !canSend;
    $('resultSection').hidden = true;
    $('xmlSection').hidden = true;
  } catch (e) {
    alert(e.message);
    currentFiche = null;
    $('ficheSection').hidden = true;
    $('btnPreview').disabled = true;
    $('btnSend').disabled = true;
  } finally {
    $('btnLoad').disabled = false;
  }
};

$('btnPreview').onclick = async () => {
  if (!currentFiche) return;
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
};

$('btnSend').onclick = async () => {
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
    let msg = `Success: ${data.success}\nMessage: ${data.message || '-'}\nDryRun: ${data.dryRun}\n`;
    if (data.pursuitDocNo) msg += `PursuitDocNo: ${data.pursuitDocNo}\n`;
    if (data.verifiedInRayvarz !== undefined) msg += `VerifiedInRayvarz: ${data.verifiedInRayvarz}\n`;
    if (data.docNotSentError) msg += `DocNotSent: ${data.docNotSentError}\n`;
    $('resultBox').textContent = msg;

    if (data.previewXml || data.soapResponse) {
      $('xmlSection').hidden = false;
      $('xmlBox').textContent = data.soapResponse || data.previewXml;
    }
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnSend').disabled = false;
  }
};

init();
