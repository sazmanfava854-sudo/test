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
  if (f.resolvedDistrictBranch) {
    const branchId = f.resolvedDistrictBranch;
    const match = config.branches.find(b => b.id === branchId);
    if (match) {
      $('branch').value = branchId;
      if (f.suggestedFund) $('fund').value = f.suggestedFund;
      else syncFundFromBranch();
      return;
    }
  }
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
    { field: 'منطقه فیش (راهنما)', source: 'نوسازی/صنفی: OtherFields → منطقه | درآمد: Base_NosaziCode.CI_City', value: (f.dutyRegion || f.incomeRegion) ? `منطقه ${f.dutyRegion || f.incomeRegion} → branch=${branchFromRegion(f.dutyRegion || f.incomeRegion) || '?'}` : '(نامشخص)' },
    { field: 'Fund', source: 'انتخاب منطقه', value: fund },
    { field: 'branch', source: 'انتخاب شعبه', value: branch ? `${branch.id} — ${branch.name}` : $('branch').value },
    { field: 'DocDate', source: 'nosazo.vb: امروز شمسی (CurrentShamsiDateString)', value: docDate || '-' },
    { field: 'ActDate / RowDate', source: 'وضعیت=1 → PaymentDate وگرنه BankPaymentDate', value: actDate || '-' },
    { field: 'Due', source: 'nosazo.vb: همان امروز شمسی (Ref DUE)', value: dueDate || '-' },
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
  $('actDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  $('dueDate').onchange = () => { if (currentFiche) renderMappingTable(currentFiche); };
  syncFundFromBranch();
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
    applyFicheDatesToForm(data);
    renderFiche(data);
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

  bindClick('btnTahatorCheck', async () => {
    const ficheNo = ($('tahatorFicheNo')?.value || '').trim();
    if (!ficheNo) return alert('شماره فیش تهاتر را وارد کنید (تک‌کد).');
    const btn = $('btnTahatorCheck');
    btn.disabled = true;
    showTahatorWaiting('بررسی Accounting_DocHeader / DocNotSent…');
    try {
      const res = await fetch('/api/tahator/check', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ficheNo })
      });
      if (res.status === 404) {
        throw new Error('API /api/tahator/check یافت نشد — pull و restart کنید.');
      }
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      showTahatorResult(formatTahatorCheck(data));
      alert(data.message || 'بررسی انجام شد');
    } catch (e) {
      alert(e.message);
    } finally {
      btn.disabled = false;
    }
  });

  bindClick('btnTahatorSend', async () => {
    const ficheNo = ($('tahatorFicheNo')?.value || '').trim();
    if (!ficheNo) return alert('شماره فیش تهاتر را وارد کنید (تک‌کد).');
    const dry = config?.tahator?.dryRun ?? config?.dryRun;
    const warn = dry
      ? `DryRun فعال است — برای ${ficheNo} فقط SOAP ساخته می‌شود؛ POST واقعی زده نمی‌شود. ادامه؟`
      : `ارسال تهاتر ${ficheNo} به رایورز؟`;
    if (!confirm(warn)) return;

    const btn = $('btnTahatorSend');
    btn.disabled = true;
    showTahatorWaiting('ارسال تهاتر به رایورز… ممکن است طول بکشد');
    try {
      const res = await fetch('/api/tahator/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ficheNo,
          branch: 0,
          fund: 0
          // تاریخ SOAP تهاتر روی سرور = امروز — تاریخ فرم ورود فیش عادی را نفرست
        })
      });
      if (res.status === 404) {
        throw new Error('API /api/tahator/send یافت نشد — pull و restart کنید.');
      }
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      showTahatorResult(formatTahatorSend(data));
      if (data.previewXml || data.soapResponse) {
        $('xmlSection').hidden = false;
        $('xmlBox').textContent = data.soapResponse || data.previewXml;
      }
      if (data.dryRun) alert('DryRun تهاتر: SOAP ساخته شد؛ POST واقعی زده نشد.');
      else if (data.skipped) alert(data.message);
      else if (data.success) alert(data.message || 'ارسال تهاتر موفق');
      else alert(data.message || (data.docNotSentError ? `عدم ارسال: ${data.docNotSentError}` : 'تهاتر ناموفق'));
    } catch (e) {
      alert(e.message);
    } finally {
      btn.disabled = false;
    }
  });
}

function showTahatorWaiting(title) {
  const box = $('tahatorResultBox');
  if (!box) return;
  box.hidden = false;
  box.textContent = `${title}\n\nصبر کنید…`;
  box.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function showTahatorResult(text) {
  const box = $('tahatorResultBox');
  if (!box) return;
  box.hidden = false;
  box.textContent = text;
  box.scrollIntoView({ behavior: 'smooth', block: 'start' });
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
    `NeedsSend (هر کدام): ${d.needsSend}`,
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
    `  ${r.incomeAccountGroup} ${r.ficheNo}: Success=${r.success} Skipped=${r.skipped}${r.skipReason ? ' (' + r.skipReason + ')' : ''} DocTyp=${r.docTyp} Branch=${r.branch}/${r.fund}${r.soapMessage ? ' — ' + r.soapMessage : ''}`
  );
  return [
    '=== نتیجه ارسال جفت تهاتر ===',
    `FicheNo ورودی: ${d.ficheNo}`,
    d.pair ? `جفت: ۱۵۷=${d.pair.amountFicheNo} → ۱۵۸=${d.pair.incomeFicheNo}` : '',
    `Success: ${d.success}`,
    `Skipped: ${d.skipped}`,
    d.skipReason ? `SkipReason: ${d.skipReason}` : '',
    `DryRun: ${d.dryRun}`,
    resultLines.length ? ['--- هر فیش ---', ...resultLines].join('\n') : '',
    d.triggerDate ? `تاریخ تریگر: ${d.triggerDate}` : '',
    `پیام: ${d.message || ''}`,
    '',
    '--- مراحل ---',
    ...(d.steps || [])
  ].filter(Boolean).join('\n');
}

setupEventHandlers();
init();
