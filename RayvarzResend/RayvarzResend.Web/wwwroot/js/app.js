let currentFiche = null;
let config = null;
let unsentItems = [];

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
  if (r === 218 || r === 80) return 218;
  if (r >= 1 && r <= 12) return 200 + r;
  if (r >= 201 && r <= 212) return r;
  return null;
}

/** همان منطق تب تکی: branch ۲۰۱–۲۱۲ / ۲۱۸ → منطقه ۱–۱۲ / ۲۱۸ برای فیلتر SQL */
function branchIdToDistrict(branchId) {
  const id = parseInt(branchId, 10);
  if (!id) return '';
  if (id === 218) return '218';
  if (id >= 201 && id <= 212) return String(id - 200);
  if (id >= 1 && id <= 12) return String(id);
  return '';
}

/** پر کردن کمبو منطقه/شعبه از config.branches — مشترک بین ارسال تکی و دسته‌ای */
function fillBranchSelect(selectEl, { includeAll = false, allLabel = 'همه مناطق' } = {}) {
  if (!selectEl || !config?.branches) return;
  selectEl.innerHTML = '';
  if (includeAll) {
    const all = document.createElement('option');
    all.value = '';
    all.textContent = allLabel;
    selectEl.appendChild(all);
  }
  config.branches.forEach((b) => {
    const opt = document.createElement('option');
    opt.value = b.id;
    opt.textContent = b.name;
    selectEl.appendChild(opt);
  });
}

function applyBranchFromFiche(f) {
  if (f.resolvedDistrictBranch) {
    const branchId = f.resolvedDistrictBranch;
    const match = config.branches.find(b => b.id === branchId);
    if (match) {
      $('branch').value = branchId;
      if (f.suggestedFund) $('fund').value = f.suggestedFund;
      else syncFundFromBranch();
      return true;
    }
  }
  const region = f.dutyRegion || f.incomeRegion;
  const branchId = region ? branchFromRegion(region) : null;
  if (!branchId) return false;
  const match = config.branches.find(b => b.id === branchId);
  if (!match) return false;
  $('branch').value = branchId;
  syncFundFromBranch();
  return true;
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

function formatShamsiDisplay(yyyymmdd) {
  if (!yyyymmdd) return '-';
  const d = String(yyyymmdd).replace(/\D/g, '');
  if (d.length < 8) return String(yyyymmdd);
  return `${d.slice(0, 4)}/${d.slice(4, 6)}/${d.slice(6, 8)}`;
}

function setupMainTabs() {
  const tabs = document.querySelectorAll('.main-tab');
  const panels = {
    unsent: $('tabUnsent'),
    single: $('tabSingle')
  };

  tabs.forEach((tab) => {
    tab.addEventListener('click', () => {
      const key = tab.dataset.tab;
      tabs.forEach((t) => {
        const active = t === tab;
        t.classList.toggle('active', active);
        t.setAttribute('aria-selected', active ? 'true' : 'false');
      });
      Object.entries(panels).forEach(([name, panel]) => {
        if (!panel) return;
        const show = name === key;
        panel.hidden = !show;
        panel.classList.toggle('active', show);
      });
    });
  });
}

function getSingleFicheKind() {
  return $('singleFicheKind')?.value || 'Income';
}

/** همان منطق سرور IdentifierDetector — فقط برای نمایش راهنما */
function detectIdentifierType(value) {
  const v = (value || '').trim();
  if (!v) return null;
  if (v.includes('/')) return 'FicheNo';
  if (/^\d+$/.test(v) && v.length >= 20) return 'BillPaymentKey';
  return 'FicheNo';
}

const identifierTypeLabels = {
  FicheNo: 'شماره فیش (FicheNo)',
  BillPaymentKey: 'BillID + PaymentID'
};

function updateIdentifierHint() {
  const hint = $('identifierHint');
  const input = $('identifierValue');
  if (!hint || !input) return;
  const t = detectIdentifierType(input.value);
  if (!t) {
    hint.hidden = true;
    hint.textContent = '';
    return;
  }
  hint.hidden = false;
  hint.textContent = `تشخیص سیستم: ${identifierTypeLabels[t]}`;
}

function isTahatorIncomeFiche(f) {
  const g = f?.incomeAccountGroup;
  return g === 157 || g === 158;
}

let datePickersReady = false;

function initDatePickers() {
  if (typeof jalaliDatepicker === 'undefined') {
    console.warn('jalaliDatepicker load نشد — CDN را چک کنید');
    return;
  }
  if (!datePickersReady) {
    jalaliDatepicker.startWatch({
      time: false,
      autoShow: false,
      autoHide: true,
      hideAfterChange: true,
      persianDigits: false,
      zIndex: 2500,
      separatorChars: { date: '/', between: ' ', time: ':' }
    });
    datePickersReady = true;
  }

  document.querySelectorAll('.btn-date-icon').forEach((btn) => {
    if (btn.dataset.bound === '1') return;
    btn.dataset.bound = '1';
    btn.addEventListener('click', (e) => {
      e.preventDefault();
      const input = document.getElementById(btn.dataset.for || '');
      if (input) jalaliDatepicker.show(input);
    });
  });

  document.querySelectorAll('input[data-jdp]').forEach((input) => {
    if (input.dataset.jdpBound === '1') return;
    input.dataset.jdpBound = '1';
    input.addEventListener('click', () => jalaliDatepicker.show(input));
  });
}

function getSelectedUnsentFicheNos() {
  return Array.from(document.querySelectorAll('.unsent-row-check:checked'))
    .map((el) => el.dataset.ficheNo)
    .filter(Boolean);
}

function updateUnsentSendButton() {
  const btn = $('btnUnsentSend');
  const planBtn = $('btnUnsentPlan');
  if (!btn) return;
  const selected = getSelectedUnsentFicheNos().length;
  btn.disabled = selected === 0;
  if (planBtn) planBtn.disabled = selected === 0;
  btn.textContent = selected > 0
    ? `ارسال ${selected} فیش انتخاب‌شده`
    : 'ارسال انتخاب‌شده‌ها';
}

function renderUnsentTable(items) {
  unsentItems = items || [];
  const section = $('unsentResultsSection');
  const tbody = $('unsentTable')?.querySelector('tbody');
  const countLabel = $('unsentCountLabel');
  const selectAll = $('unsentSelectAll');
  if (!section || !tbody) return;

  if (!unsentItems.length) {
    section.hidden = false;
    tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;color:var(--text-muted)">موردی یافت نشد</td></tr>';
    if (countLabel) countLabel.textContent = '۰ مورد';
    if (selectAll) selectAll.checked = false;
    updateUnsentSendButton();
    return;
  }

  section.hidden = false;
  tbody.innerHTML = unsentItems.map((item) => `
    <tr>
      <td class="col-check"><input type="checkbox" class="unsent-row-check" data-fiche-no="${item.ficheNo}" /></td>
      <td>${item.subKindLabel || (item.isTahator ? 'تهاتر' : '-')}</td>
      <td>${item.bnkAcntNo || '-'}</td>
      <td>${item.billId || '-'}</td>
      <td>${item.paymentId || '-'}</td>
      <td>${formatShamsiDisplay(item.bankPaymentDate)}</td>
      <td>${formatShamsiDisplay(item.paymentDate)}</td>
      <td>${item.ficheNo}</td>
      <td>${Number(item.payable || 0).toLocaleString()}</td>
    </tr>
  `).join('');

  if (countLabel) countLabel.textContent = `${unsentItems.length.toLocaleString('fa-IR')} مورد`;
  if (selectAll) selectAll.checked = false;

  document.querySelectorAll('.unsent-row-check').forEach((cb) => {
    cb.addEventListener('change', () => {
      const all = document.querySelectorAll('.unsent-row-check');
      const checked = document.querySelectorAll('.unsent-row-check:checked');
      if (selectAll) selectAll.checked = all.length > 0 && checked.length === all.length;
      updateUnsentSendButton();
    });
  });
  updateUnsentSendButton();
}

function formatShamsiInput(yyyymmdd) {
  if (!yyyymmdd) return '';
  const d = String(yyyymmdd).replace(/\D/g, '');
  if (d.length < 8) return String(yyyymmdd);
  return `${d.slice(0, 4)}/${d.slice(4, 6)}/${d.slice(6, 8)}`;
}

function applyFicheDatesToForm(f) {
  $('docDate').value = formatShamsiInput(f.rayvarzDocDate);
  $('actDate').value = formatShamsiInput(f.rayvarzActDate);
  $('dueDate').value = formatShamsiInput(f.rayvarzDueDate);
}

function getPayload(resetStatus) {
  return {
    fiche: currentFiche,
    branch: parseInt($('branch').value),
    fund: parseInt($('fund').value),
    docDate: $('docDate').value,
    actDate: $('actDate').value,
    dueDate: $('dueDate').value,
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

function ficheStatusClass(f) {
  if (f.canSend) return 'status-ok';
  if (f.existsInRayvarz || f.blockReason) return 'status-err';
  return 'status-warn';
}

function updateSendButton(f) {
  const btn = $('btnSend');
  const previewBtn = $('btnPreview');
  if (!f) {
    btn.disabled = true;
    btn.title = 'ابتدا فیش را دریافت کنید';
    if (previewBtn) previewBtn.disabled = true;
    return;
  }
  if (isTahatorIncomeFiche(f)) {
    btn.disabled = false;
    if (previewBtn) previewBtn.disabled = true;
    btn.title = config?.tahator?.dryRun ?? config?.dryRun
      ? 'تهاتر — DryRun فعال'
      : 'ارسال جفت تهاتر (۱۵۷+۱۵۸) به رایورز';
    return;
  }
  if (!f.canSend) {
    btn.disabled = true;
    if (previewBtn) previewBtn.disabled = true;
    btn.title = f.blockReason || f.statusMessage || 'قابل ارسال نیست';
    return;
  }
  btn.disabled = false;
  if (previewBtn) previewBtn.disabled = false;
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
  const actDate = $('actDate').value;
  const dueDate = $('dueDate').value;
  const sourceId = config.sourceSystemId ?? null;

  return [
    { field: 'TransactionId (سند)', source: 'newGuidPerSend (پیش‌فرض) یا NidFiche از config', value: f.nidFiche ? `${f.nidFiche} → GUID جدید در XML` : '-' },
    { field: 'SourceId (ردیف)', source: 'appsettings → Rayvarz:SourceSystemId (خالی = NULL)', value: sourceId ?? 'NULL' },
    { field: 'Id (ردیف)', source: 'همان NidFiche — شناسه تراکنش فیش', value: f.nidFiche || '-' },
    { field: 'RowDocNo (هدر)', source: 'FicheNo — فقط در DocumentItem', value: f.ficheNo },
    { field: 'RefRowDocNo (دیتیل)', source: 'نوسازی/صنفی: 0 | درآمد: از config', value: (f.category === 'DutyNosazi' || f.category === 'DutySenfi') ? '0' : (config?.refRowDocNoInDetail === 'ficheNo' ? '(FicheNo)' : '1') },
    { field: 'Ref2', source: 'Income_Fiche.BillID / Duty_Fiche.BillID', value: f.billId || '-' },
    { field: 'Ref3', source: 'Income_Fiche.PaymentID / Duty_Fiche.PaymentID', value: f.paymentId || '-' },
    { field: 'BnkAcntNo (کد نوسازی)', source: bnkAcntNoSource(f), value: f.bnkAcntNo || '-' },
    { field: 'منطقه فیش (راهنما)', source: 'نوسازی/صنفی: OtherFields → منطقه | درآمد: Base_NosaziCode.District', value: (f.dutyRegion || f.incomeRegion) ? `منطقه ${f.dutyRegion || f.incomeRegion} → branch=${branchFromRegion(f.dutyRegion || f.incomeRegion) || '?'}` : '(نامشخص)' },
    { field: 'Fund', source: 'انتخاب منطقه', value: fund },
    { field: 'branch', source: 'انتخاب شعبه', value: branch ? `${branch.id} — ${branch.name}` : $('branch').value },
    { field: 'DocDate', source: 'Income_Fiche / Duty_Fiche: PaymentDate → BankPaymentDate', value: docDate || '-' },
    { field: 'ActDate / RowDate', source: 'وضعیت=1 → PaymentDate؛ وگرنه BankPaymentDate (با fallback)', value: actDate || '-' },
    { field: 'Due', source: 'BankPaymentDate → PaymentDate', value: dueDate || '-' },
    { field: 'شعبه (nosazo)', source: 'BillID/PaymentID → DistrickBranch', value: f.resolvedDistrictBranch ? `${f.resolvedDistrictBranch} (Fund پیشنهادی: ${f.suggestedFund || '-'})` : (f.dutyRegion || f.incomeRegion || '-') },
    { field: 'DocTyp / DocTypDsc', source: 'نوع فیش', value: `${f.docTyp} — ${f.docDsc}` },
    { field: 'DocRow', source: 'شماره ردیف سند (ثابت ۱)', value: '1' },
    { field: 'IncmRow', source: 'شماره ردیف درآمد (۱، ۲، ۳…)', value: `${(f.rows || []).length} ردیف` },
    { field: 'Qty (دیتیل)', source: 'نوسازی/صنفی: PayablePrice کل فیش (در هر ردیف یکسان) | درآمد: Val همان ردیف', value: (f.category === 'DutyNosazi' || f.category === 'DutySenfi') ? Number(f.payable).toLocaleString() : (f.rows || []).map(r => Number(r.val).toLocaleString()).join(' / ') },
    { field: 'Val (دیتیل)', source: 'جمع Val باید = Payable؛ نوسازی = Payable − سایر ردیف‌ها', value: (() => { const sum = (f.rows || []).reduce((a, r) => a + Number(r.val), 0); return `${(f.rows || []).map(r => Number(r.val).toLocaleString()).join(' + ')} = ${sum.toLocaleString()} (Payable: ${Number(f.payable).toLocaleString()})`; })() },
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
  const statusClass = ficheStatusClass(f);
  const alertHtml = f.blockReason
    ? `<div class="fiche-alert fiche-alert-err" role="alert">${f.blockReason}</div>`
    : '';

  $('ficheSummary').innerHTML = `
    ${alertHtml}
    <div class="stat-card">
      <span class="stat-label">شماره فیش</span>
      <span class="stat-value">${f.ficheNo}</span>
    </div>
    <div class="stat-card">
      <span class="stat-label">نوع</span>
      <span class="stat-value">${isTahatorIncomeFiche(f) ? 'تهاتر (درآمد)' : (categoryLabels[f.category] || f.category)}</span>
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
    alert(e.message);
    return;
  }

  const branchSel = $('branch');
  const fundSel = $('fund');
  fillBranchSelect(branchSel);
  fillBranchSelect($('unsentDistrict'), { includeAll: true });
  config.branches.forEach(b => {
    const optFund = document.createElement('option');
    optFund.value = b.fund;
    optFund.textContent = `${b.fund} — ${b.name}`;
    fundSel.appendChild(optFund);
  });

  branchSel.onchange = () => { syncFundFromBranch(); if (currentFiche) renderMappingTable(currentFiche); };
  fundSel.onchange = () => { syncBranchFromFund(); if (currentFiche) renderMappingTable(currentFiche); };
  $('docDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  $('actDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  $('dueDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  $('identifierValue')?.addEventListener('input', updateIdentifierHint);
  syncFundFromBranch();
  setupMainTabs();
  initDatePickers();
  window.addEventListener('load', initDatePickers);
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
  const required = ['btnLoad', 'btnPreview', 'btnSend'];
  const missing = required.filter((id) => !$(id));
  if (missing.length) {
    alert(`فایل index.html قدیمی است یا ناقص.\nدکمه‌های گم‌شده: ${missing.join(', ')}\nاز شاخه rayvarz-resend دوباره کپی کنید.`);
  }

  bindClick('btnLoad', async () => {
  const kind = getSingleFicheKind();
  const value = $('identifierValue').value.trim();
  if (!value) return alert('شناسه فیش را وارد کنید');

  $('btnLoad').disabled = true;
  try {
    const res = await fetch('/api/fiche/load', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ficheKind: kind,
        identifierValue: value,
        branch: parseInt($('branch').value),
        docDate: $('docDate').value
      })
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || data.detail || data.title || `خطا (HTTP ${res.status})`);

    currentFiche = data;
    if (kind === 'Income' && data.category !== 'Income') {
      throw new Error('نوع فیش با «درآمد» انتخاب‌شده مطابقت ندارد');
    }
    if (kind === 'Duty' && data.category !== 'DutyNosazi' && data.category !== 'DutySenfi') {
      throw new Error('نوع فیش با «نوسازی و صنفی» انتخاب‌شده مطابقت ندارد');
    }
    updateIdentifierHint();
    applyBranchFromFiche(data);
    applyFicheDatesToForm(data);
    renderFiche(data);
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
  if (!isTahatorIncomeFiche(currentFiche) && !currentFiche.canSend) {
    return alert(currentFiche.blockReason || currentFiche.statusMessage || 'این فیش قابل پیش‌نمایش نیست');
  }
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
  if (!isTahatorIncomeFiche(currentFiche) && !currentFiche.canSend) {
    return alert(currentFiche.blockReason || currentFiche.statusMessage || 'این فیش قابل ارسال نیست');
  }

  if (isTahatorIncomeFiche(currentFiche)) {
    const dry = config?.tahator?.dryRun ?? config?.dryRun;
    const warn = dry
      ? `DryRun فعال — تهاتر ${currentFiche.ficheNo} فقط SOAP می‌سازد. ادامه؟`
      : `ارسال تهاتر ${currentFiche.ficheNo} به رایورز؟`;
    if (!confirm(warn)) return;
    $('btnSend').disabled = true;
    try {
      const res = await fetch('/api/tahator/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ficheNo: currentFiche.ficheNo, branch: 0, fund: 0 })
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      $('resultSection').hidden = false;
      showTahatorSendResult(data);
      $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
      if (data.previewXml || data.soapResponse) {
        $('xmlSection').hidden = false;
        $('xmlBox').textContent = data.soapResponse || data.previewXml;
      }
      alert(tahatorSendAlertMessage(data));
    } catch (e) {
      alert(e.message);
    } finally {
      $('btnSend').disabled = false;
      updateSendButton(currentFiche);
    }
    return;
  }

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
      alert(data.message || data.docNotSentError || 'ارسال ناموفق — Message و پاسخ SOAP را بررسی کنید.');
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

  const selectAll = $('unsentSelectAll');
  if (selectAll) {
    selectAll.addEventListener('change', () => {
      document.querySelectorAll('.unsent-row-check').forEach((cb) => {
        cb.checked = selectAll.checked;
      });
      updateUnsentSendButton();
    });
  }

  bindClick('btnUnsentSearch', async () => {
    const fromDate = ($('unsentFromDate')?.value || '').trim();
    const toDate = ($('unsentToDate')?.value || '').trim();
    const ficheNo = ($('unsentFicheNo')?.value || '').trim();
    const billId = ($('unsentBillId')?.value || '').trim();
    const paymentId = ($('unsentPaymentId')?.value || '').trim();
    const district = branchIdToDistrict($('unsentDistrict')?.value);

    if ((fromDate && !toDate) || (!fromDate && toDate)) {
      return alert('هر دو تاریخ از و تا را وارد کنید یا هر دو را خالی بگذارید');
    }
    if (!ficheNo && !billId && !paymentId && !district && !fromDate) {
      return alert('حداقل یکی از شماره فیش، شناسه قبض، شناسه پرداخت، منطقه یا بازه تاریخ را وارد کنید');
    }

    const btn = $('btnUnsentSearch');
    btn.disabled = true;
    $('unsentResultBox').hidden = true;
    try {
      const res = await fetch('/api/unsent/search', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ficheKind: $('unsentFicheKind').value,
          ficheNo,
          fromDate: fromDate || null,
          toDate: toDate || null,
          billId,
          paymentId,
          district
        })
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      renderUnsentTable(data.items || []);
      if (data.truncated) {
        alert(`بیش از ${data.count} مورد یافت شد — فقط ${data.count} مورد اول نمایش داده شد. فیلترها را محدودتر کنید.`);
      }
    } catch (e) {
      alert(e.message);
      renderUnsentTable([]);
    } finally {
      btn.disabled = false;
    }
  });

  bindClick('btnUnsentPlan', async () => {
    const selected = getSelectedUnsentFicheNos();
    if (!selected.length) return alert('حداقل یک فیش انتخاب کنید');

    const btn = $('btnUnsentPlan');
    btn.disabled = true;
    const box = $('unsentResultBox');
    box.hidden = false;
    box.textContent = 'در حال بررسی مسیر ارسال هر فیش…';

    try {
      const res = await fetch('/api/unsent/plan-batch', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ficheKind: $('unsentFicheKind').value,
          ficheNos: selected,
          resetStatus: true
        })
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);

      const lines = (data.items || []).map((p) =>
        `${p.ficheNo} → ${p.sendPath}${p.canSend ? ' ✓' : ' ✗'} | ${p.detail || ''}${p.blockReason ? ' — ' + p.blockReason : ''}${p.tahatorPairFicheNo ? ' | جفت: ' + p.tahatorPairFicheNo : ''}`
      );
      box.textContent = [
        '=== برنامه ارسال دسته‌ای (بدون ارسال واقعی) ===',
        'برای هر فیش: Income=درآمدی | Tahator=تهاتر ۱۵۷+۱۵۸ | Duty=نوسازی/صنفی',
        '',
        ...lines
      ].join('\n');
    } catch (e) {
      box.textContent = e.message;
      alert(e.message);
    } finally {
      updateUnsentSendButton();
    }
  });

  bindClick('btnUnsentSend', async () => {
    const selected = getSelectedUnsentFicheNos();
    if (!selected.length) return alert('حداقل یک فیش انتخاب کنید');

    const dry = config?.dryRun;
    const kind = $('unsentFicheKind').value;
    const kindLabel = kind === 'Duty' ? 'نوسازی/صنفی' : 'شهرسازی';
    const warn = dry
      ? `DryRun فعال — ${selected.length} فیش ${kindLabel} فقط SOAP می‌سازد. ادامه؟`
      : `ارسال ${selected.length} فیش ${kindLabel} به رایورز؟`;
    if (!confirm(warn)) return;

    const btn = $('btnUnsentSend');
    btn.disabled = true;
    const box = $('unsentResultBox');
    box.hidden = false;
    box.textContent = `در حال ارسال ${selected.length} فیش…\n\nصبر کنید…`;

    try {
      const res = await fetch('/api/unsent/send-batch', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ficheKind: kind,
          ficheNos: selected,
          resetStatus: true
        })
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);

      const lines = (data.results || []).map((r) =>
        `${r.ficheNo} [${r.sendPath || '-'}]: ${r.skipped ? 'SKIP' : (r.success ? 'OK' : 'FAIL')} — ${r.message || ''}${r.docNotSentError ? ' | DocNotSent: ' + r.docNotSentError : ''}`
      );
      box.textContent = [
        '=== نتیجه ارسال دسته‌ای ===',
        `کل: ${data.total} | موفق: ${data.succeeded} | رد: ${data.skipped} | ناموفق: ${data.failed}`,
        `DryRun: ${data.dryRun}`,
        '',
        ...lines
      ].join('\n');
      box.scrollIntoView({ behavior: 'smooth', block: 'start' });
      alert(unsentBatchSendAlertMessage(data));

      $('btnUnsentSearch').click();
    } catch (e) {
      box.textContent = e.message;
      alert(e.message);
    } finally {
      updateUnsentSendButton();
    }
  });
}

function showTahatorSendResult(data) {
  $('resultBox').textContent = formatTahatorSend(data);
}

function formatTahatorCheck(d) {
  const snap = d.snapshot;
  const f = d.fiche;
  const pairLines = (d.pairMembers || []).map(m =>
    `  ${m.incomeAccountGroup} ${m.ficheNo}: DocTyp=${m.docTyp} Branch=${m.branch} Fund=${m.fund} | Header=${m.existsInAccountingDocHeader} Rayvarz=${m.existsInRayvarz} NeedsSend=${m.needsSend}`
  );
  return [
    '=== بررسی جفت تهاتر (۱۵۷+۱۵۸) ===',
    `FicheNo ورودی: ${d.ficheNo}`,
    d.pair ? `جفت: ۱۵۷=${d.pair.amountFicheNo} | ۱۵۸=${d.pair.incomeFicheNo}` : 'جفت: —',
    pairLines.length ? ['--- وضعیت هر فیش ---', ...pairLines].join('\n') : '',
    `NeedsSend (حداقل یکی از جفت): ${d.needsSend}`,
    f ? `فیش ورودی — Payable: ${Number(f.payable || 0).toLocaleString()}` : '',
    d.pendingStoredSnapshot
      ? `Snapshot Pending: Id=${d.pendingStoredSnapshot.snapshotId}`
      : 'Snapshot Pending: —',
    d.docNotSentError ? `DocNotSent: ${d.docNotSentError}` : 'DocNotSent: —',
    `پیام: ${d.message || ''}`,
    snap ? [
      '',
      '--- Snapshot فعلی (فیش ورودی) ---',
      `EumFicheStatus: ${snap.eumFicheStatus}`,
      `ExportPermanentDate: ${snap.exportPermanentDate || ''}`,
      `PaymentBreakDate: ${snap.paymentBreakDate || ''}`,
      `PaymentDate: ${snap.paymentDate || ''}`
    ].join('\n') : ''
  ].filter(Boolean).join('\n');
}

function formatTahatorSend(d) {
  const resultLines = (d.ficheResults || []).map(r =>
    `  ${r.incomeAccountGroup} ${r.ficheNo}: Success=${r.success} Skipped=${r.skipped}${r.skipReason ? ' (' + r.skipReason + ')' : ''} DocTyp=${r.docTyp} Branch=${r.branch}/${r.fund}${r.soapMessage ? ' — ' + r.soapMessage : ''}${r.docNotSentError ? ' | DocNotSent: ' + r.docNotSentError : ''}`
  );
  return [
    '=== نتیجه ارسال جفت تهاتر ===',
    `FicheNo ورودی: ${d.ficheNo}`,
    d.pair ? `جفت: ۱۵۷=${d.pair.amountFicheNo} → ۱۵۸=${d.pair.incomeFicheNo}` : '',
    `Success: ${d.success}`,
    `Skipped: ${d.skipped}`,
    d.skipReason ? `SkipReason: ${d.skipReason}` : '',
    `DryRun: ${d.dryRun}`,
    d.docNotSentError ? `DocNotSent: ${d.docNotSentError}` : '',
    resultLines.length ? ['--- هر فیش ---', ...resultLines].join('\n') : '',
    d.triggerDate ? `تاریخ تریگر: ${d.triggerDate}` : '',
    `پیام: ${d.message || ''}`,
    '',
    '--- مراحل ---',
    ...(d.steps || [])
  ].filter(Boolean).join('\n');
}

function unsentBatchSendAlertMessage(data) {
  const lines = [];
  if (data.dryRun) {
    lines.push('⚠ DryRun فعال است — SOAP ساخته می‌شود ولی POST واقعی به رایورز زده نمی‌شود.');
    lines.push('برای ارسال واقعی: Rayvarz:DryRun=false در appsettings و Restart سرویس.');
  }
  lines.push(
    `ارسال دسته‌ای تمام شد — موفق: ${data.succeeded}، ناموفق: ${data.failed}، رد: ${data.skipped}`
  );

  const results = data.results || [];
  const problems = results.filter((r) => r.skipped || !r.success);
  if (problems.length) {
    lines.push('');
    lines.push('علت خطا / رد:');
    problems.forEach((r) => {
      const path = r.sendPath ? `[${r.sendPath}] ` : '';
      if (r.skipped) {
        lines.push(`  • ${r.ficheNo} ${path}رد شد — ${r.message || r.skipReason || 'بدون جزئیات'}`);
      } else {
        const detail = [r.message, r.docNotSentError ? `DocNotSent: ${r.docNotSentError}` : '']
          .filter(Boolean)
          .join(' | ');
        lines.push(`  • ${r.ficheNo} ${path}ناموفق — ${detail || 'بدون جزئیات'}`);
      }
    });
  }

  const successes = results.filter((r) => r.success && !r.skipped);
  if (successes.length && problems.length) {
    lines.push('');
    lines.push(`موفق (${successes.length}): ${successes.map((r) => r.ficheNo).join(', ')}`);
  }

  lines.push('');
  lines.push('جزئیات کامل در باکس «نتیجه» پایین صفحه.');
  return lines.join('\n');
}

function tahatorSendAlertMessage(data) {
  const lines = [];
  if (data.dryRun) {
    lines.push('⚠ DryRun فعال است — SOAP ساخته می‌شود ولی POST واقعی به رایورز زده نمی‌شود.');
    lines.push('برای ارسال واقعی: Rayvarz:DryRun=false در appsettings و Restart سرویس.');
  }
  if (data.message) lines.push(data.message);
  if (data.ficheResults?.length) {
    lines.push('');
    lines.push('وضعیت هر فیش:');
    data.ficheResults.forEach((r) => {
      const st = r.skipped
        ? `رد شد (${r.skipReason || 'Skip'})`
        : r.success ? 'موفق' : `ناموفق — ${r.soapMessage || r.docNotSentError || 'بدون جزئیات'}`;
      lines.push(`  • ${r.ficheNo} (گروه ${r.incomeAccountGroup}): ${st}`);
    });
  }
  if (data.docNotSentError) lines.push(`\nDocNotSent: ${data.docNotSentError}`);
  if (!data.success && !data.skipped && data.steps?.length) {
    const last = data.steps.slice(-5).join('\n');
    lines.push('\nآخرین مراحل:\n' + last);
  }
  lines.push('\nجزئیات کامل در باکس «نتیجه» پایین صفحه.');
  return lines.join('\n');
}

setupEventHandlers();
init();
