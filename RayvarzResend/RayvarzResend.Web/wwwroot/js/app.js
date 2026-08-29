let currentFiche = null;
let config = null;
let currentUser = null;
let unsentItems = [];
const selectedUnsentFicheNos = new Set();
const unsentSearchState = {
  page: 1,
  pageSize: 25,
  totalCount: 0,
  totalPages: 0,
  filters: null
};

const $ = (id) => document.getElementById(id);

let appToastTimer = null;

function hideAppMessage() {
  const host = $('appToastHost');
  if (!host) return;
  host.innerHTML = '';
  if (appToastTimer) {
    clearTimeout(appToastTimer);
    appToastTimer = null;
  }
}

function showAppMessage(message, type = 'info', { timeout = 9000 } = {}) {
  const host = $('appToastHost');
  if (!host || !message) return;

  hideAppMessage();

  const toast = document.createElement('div');
  toast.className = `app-toast app-toast-${type}`;
  toast.setAttribute('role', type === 'error' ? 'alert' : 'status');

  const text = document.createElement('span');
  text.className = 'app-toast-text';
  text.textContent = message;

  const closeBtn = document.createElement('button');
  closeBtn.type = 'button';
  closeBtn.className = 'app-toast-close';
  closeBtn.setAttribute('aria-label', 'بستن');
  closeBtn.textContent = '×';
  closeBtn.addEventListener('click', hideAppMessage);

  toast.appendChild(text);
  toast.appendChild(closeBtn);
  host.appendChild(toast);

  if (timeout > 0) {
    appToastTimer = setTimeout(hideAppMessage, timeout);
  }
}

function showAppSuccess(message, options) {
  showAppMessage(message, 'success', options);
}

function showAppError(message, options) {
  showAppMessage(message, 'error', { timeout: 12000, ...options });
}

function showAppWarning(message, options) {
  showAppMessage(message, 'warning', options);
}

function showAppInfo(message, options) {
  showAppMessage(message, 'info', options);
}

const categoryLabels = {
  Income: 'درآمد',
  DutyNosazi: 'نوسازی',
  DutySenfi: 'صنفی',
  Unknown: 'نامشخص'
};

function branchFromRegion(regionStr) {
  const r = parseInt(regionStr, 10);
  if (Number.isNaN(r)) return null;
  if (r === 102) return 102;
  if (r === 218 || r === 80) return 218;
  if (r >= 1 && r <= 12) return 200 + r;
  if (r >= 201 && r <= 212) return r;
  return null;
}

/** همان منطق تب تکی: branch ۲۰۱–۲۱۲ / ۲۱۸ → منطقه ۱–۱۲ / ۲۱۸ برای فیلتر SQL */
function branchIdToDistrict(branchId) {
  const id = parseInt(branchId, 10);
  if (!id) return '';
  if (id === 102) return '102';
  if (id === 218) return '218';
  if (id >= 201 && id <= 212) return String(id - 200);
  if (id >= 1 && id <= 12) return String(id);
  return '';
}

/** پر کردن کمبو منطقه/شعبه از config.branches — مشترک بین ارسال تکی و دسته‌ای */
function fillBranchSelect(selectEl, { includeAll = false, allLabel = 'همه مناطق', restrictToDistrict = null } = {}) {
  if (!selectEl || !config?.branches) return;
  selectEl.innerHTML = '';
  if (includeAll) {
    const all = document.createElement('option');
    all.value = '';
    all.textContent = allLabel;
    selectEl.appendChild(all);
  }
  const restrict = restrictToDistrict ? String(restrictToDistrict) : '';
  config.branches.forEach((b) => {
    if (restrict) {
      const dist = branchIdToDistrict(String(b.id));
      if (dist !== restrict) return;
    }
    const opt = document.createElement('option');
    opt.value = b.id;
    opt.textContent = b.name;
    selectEl.appendChild(opt);
  });
}

function getUserDistrict() {
  const d = currentUser?.district;
  return d == null || d === '' ? '' : String(d);
}

function applyRegionalUserRestrictions() {
  if (isAdminUser()) {
    fillBranchSelect($('branch'));
    $('branch')?.removeAttribute('disabled');
    $('fund')?.removeAttribute('disabled');
    syncFundFromBranch();
    return;
  }

  const district = getUserDistrict();
  fillBranchSelect($('branch'), { restrictToDistrict: district || '__none__' });
  const branchId = district ? branchFromRegion(district) : null;
  if (branchId) {
    $('branch').value = branchId;
    $('branch').disabled = true;
    if (branchId !== 102) syncFundFromBranch();
    $('fund').disabled = true;
  }
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
    installment: $('tabInstallment'),
    ficheDate: $('tabFicheDate'),
    bankInquiry: $('tabBankInquiry'),
    users: $('tabUsers')
  };

  tabs.forEach((tab) => {
    tab.addEventListener('click', () => {
      const key = tab.dataset.tab;
      if (tab.hidden) return;
      activateMainTab(key, tabs, panels);
    });
  });
}

function activateMainTab(key, tabs = document.querySelectorAll('.main-tab'), panels = {
  unsent: $('tabUnsent'),
  installment: $('tabInstallment'),
  ficheDate: $('tabFicheDate'),
  bankInquiry: $('tabBankInquiry'),
  users: $('tabUsers')
}) {
  tabs.forEach((t) => {
    const active = t.dataset.tab === key && !t.hidden;
    t.classList.toggle('active', active);
    t.setAttribute('aria-selected', active ? 'true' : 'false');
  });
  Object.entries(panels).forEach(([name, panel]) => {
    if (!panel) return;
    const show = name === key;
    panel.hidden = !show;
    panel.classList.toggle('active', show);
  });
}

function getSingleFicheKind() {
  return $('singleFicheKind')?.value || 'Income';
}

let rayvarzSendMode = 'single';

function setRayvarzSendMode(mode) {
  if (mode === 'bulk' && !canAccessUnsent()) mode = 'single';
  rayvarzSendMode = mode === 'bulk' ? 'bulk' : 'single';
  document.querySelectorAll('[data-rayvarz-mode]').forEach((btn) => {
    const active = btn.dataset.rayvarzMode === rayvarzSendMode;
    btn.classList.toggle('active', active);
    btn.setAttribute('aria-selected', active ? 'true' : 'false');
  });
  const singlePanel = $('rayvarzSinglePanel');
  const bulkPanel = $('rayvarzBulkPanel');
  if (singlePanel) singlePanel.hidden = rayvarzSendMode !== 'single';
  if (bulkPanel) bulkPanel.hidden = rayvarzSendMode !== 'bulk';
  if (rayvarzSendMode === 'bulk') {
    $('ficheSection')?.setAttribute('hidden', '');
    $('resultSection')?.setAttribute('hidden', '');
    $('xmlSection')?.setAttribute('hidden', '');
  }
}

const installmentLookupLabels = {
  NoDocument: 'شماره سند',
  TrackingNo: 'کد پیگیری'
};

let installmentMode = 'single';
let installmentExcelRows = [];

const ficheDateStatusLabels = {
  0: 'صدورموقت',
  1: 'صدوردایم',
  2: 'چاپ',
  3: 'تایید دستی/لحظه‌ای بانک',
  4: 'ابطال',
  5: 'تایید بانک'
};

const ficheDateStatusOrder = [0, 1, 2, 3, 4, 5];

let ficheDateItems = [];
const selectedFicheDateNos = new Set();
const ficheDateSearchState = {
  page: 1,
  pageSize: 25,
  totalCount: 0,
  totalPages: 0
};

function getInstallmentApplyEndState() {
  if (installmentMode === 'excel') {
    return !!$('installmentApplyEndStateExcel')?.checked;
  }
  return !!$('installmentApplyEndState')?.checked;
}

function syncInstallmentOdooatCheckboxes(source) {
  const single = $('installmentApplyEndState');
  const excel = $('installmentApplyEndStateExcel');
  if (!single || !excel) return;
  const checked = source === 'excel' ? excel.checked : single.checked;
  single.checked = checked;
  excel.checked = checked;
}

function setInstallmentMode(mode) {
  const prevMode = installmentMode;
  installmentMode = mode === 'excel' ? 'excel' : 'single';
  document.querySelectorAll('.installment-mode-tab[data-installment-mode]').forEach((btn) => {
    const active = btn.dataset.installmentMode === installmentMode;
    btn.classList.toggle('active', active);
    btn.setAttribute('aria-selected', active ? 'true' : 'false');
  });
  const singlePanel = $('installmentSinglePanel');
  const excelPanel = $('installmentExcelPanel');
  if (singlePanel) singlePanel.hidden = installmentMode !== 'single';
  if (excelPanel) excelPanel.hidden = installmentMode !== 'excel';

  if (prevMode !== installmentMode) {
    syncInstallmentOdooatCheckboxes(prevMode === 'excel' ? 'excel' : 'single');
  }

  $('installmentPreviewSection').hidden = true;
  if ($('btnInstallmentUpdate')) $('btnInstallmentUpdate').disabled = true;
}

function normalizeExcelHeader(value) {
  return String(value || '')
    .trim()
    .toLowerCase()
    .replace(/\s+/g, '')
    .replace(/ي/g, 'ی')
    .replace(/ك/g, 'ک');
}

function mapExcelHeaderIndex(headers) {
  const map = {};
  headers.forEach((h, idx) => {
    const key = normalizeExcelHeader(h);
    if (key === 'identifier' || key === 'شناسه' || key === 'nodocument' || key === 'trackingno'
      || key === 'شمارهسند' || key === 'کدپیگیری' || key === 'شمارهسند/کدپیگیری') {
      map.identifier = idx;
    }
    if (key === 'paymentcost' || key === 'مبلغ') map.paymentCost = idx;
    if (key === 'paymentdate' || key === 'تاریخ' || key === 'تاریخپرداخت') map.paymentDate = idx;
  });
  return map;
}

function hasScientificNotation(text) {
  return /e[+-]?\d+/i.test(String(text ?? '').trim());
}

function parseExcelCellValue(sheet, rowIdx, colIdx) {
  const ref = XLSX.utils.encode_cell({ r: rowIdx, c: colIdx });
  const cell = sheet[ref];
  if (!cell) return '';
  if (cell.w != null && String(cell.w).trim() !== '') return String(cell.w).trim();
  if (cell.t === 's') return String(cell.v ?? '').trim();
  if (cell.t === 'n' && Number.isFinite(cell.v)) {
    const n = cell.v;
    if (Number.isInteger(n) && Math.abs(n) <= Number.MAX_SAFE_INTEGER) return String(Math.trunc(n));
    return String(n);
  }
  return String(cell.v ?? '').trim();
}

function setSheetCellAsText(sheet, rowIdx, colIdx, value) {
  const ref = XLSX.utils.encode_cell({ r: rowIdx, c: colIdx });
  sheet[ref] = { t: 's', v: String(value), w: String(value) };
}

function parseInstallmentExcelFile(file) {
  return new Promise((resolve, reject) => {
    if (typeof XLSX === 'undefined') {
      reject(new Error('کتابخانه خواندن اکسل بارگذاری نشد'));
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target.result);
        const workbook = XLSX.read(data, { type: 'array', cellText: true, cellDates: false });
        const sheetName = workbook.SheetNames[0];
        if (!sheetName) {
          reject(new Error('برگه‌ای در فایل اکسل یافت نشد'));
          return;
        }
        const sheet = workbook.Sheets[sheetName];
        const range = sheet?.['!ref']
          ? XLSX.utils.decode_range(sheet['!ref'])
          : { s: { r: 0, c: 0 }, e: { r: 0, c: 0 } };

        const headerRow = [];
        for (let c = range.s.c; c <= range.e.c; c++) {
          headerRow.push(parseExcelCellValue(sheet, range.s.r, c));
        }
        if (!headerRow.length) {
          reject(new Error('فایل اکسل خالی است'));
          return;
        }

        const col = mapExcelHeaderIndex(headerRow);
        const required = ['identifier', 'paymentCost', 'paymentDate'];
        const missing = required.filter((k) => col[k] == null);
        if (missing.length) {
          reject(new Error('ستون‌های الزامی: شناسه، مبلغ، تاریخ پرداخت'));
          return;
        }

        const parsed = [];
        for (let r = range.s.r + 1; r <= range.e.r; r++) {
          const identifier = parseExcelCellValue(sheet, r, col.identifier);
          const paymentCost = parseExcelCellValue(sheet, r, col.paymentCost);
          const paymentDate = parseExcelCellValue(sheet, r, col.paymentDate);

          if (hasScientificNotation(identifier)) {
            reject(new Error(
              `ردیف ${r + 1}: شناسه «${identifier}» به‌صورت علمی خوانده شد (مثل 5.02E+14). تغییر نوع ستون به Text عدد را برنمی‌گرداند — کد پیگیری را دوباره تایپ کنید یا از «دانلود قالب اکسل» استفاده کنید.`
            ));
            return;
          }
          if (hasScientificNotation(paymentCost)) {
            reject(new Error(
              `ردیف ${r + 1}: مبلغ «${paymentCost}» به‌صورت علمی است. ستون PaymentCost را Text کنید.`
            ));
            return;
          }

          const item = {
            identifier: identifier.trim(),
            paymentCost: paymentCost.trim(),
            paymentDate: paymentDate.trim()
          };
          if (!item.identifier && !item.paymentCost && !item.paymentDate) continue;
          parsed.push(item);
        }

        if (!parsed.length) {
          reject(new Error('هیچ ردیف داده‌ای در فایل اکسل یافت نشد'));
          return;
        }
        resolve(parsed);
      } catch (err) {
        reject(err);
      }
    };
    reader.onerror = () => reject(new Error('خطا در خواندن فایل'));
    reader.readAsArrayBuffer(file);
  });
}

function getInstallmentPayload() {
  const base = {
    applyEndState: getInstallmentApplyEndState()
  };
  if (installmentMode === 'excel') {
    return {
      ...base,
      excelRows: installmentExcelRows.map((r) => ({
        identifier: r.identifier || '',
        paymentCost: r.paymentCost || '',
        paymentDate: r.paymentDate || ''
      }))
    };
  }
  const raw = ($('installmentValues')?.value || '').trim();
  return {
    ...base,
    valuesText: raw
  };
}

function syncInstallmentDryRunUi() {
  const updateBtn = $('btnInstallmentUpdate');
  if (updateBtn && !updateBtn.disabled) updateBtn.textContent = 'اعمال';
}

function toPersianDigits(value) {
  return String(value ?? '').replace(/\d/g, (d) => '۰۱۲۳۴۵۶۷۸۹'[Number(d)]);
}

function formatInstallmentCost(value) {
  if (value == null || value === '') return '-';
  const n = Number(String(value).replace(/,/g, ''));
  if (!Number.isNaN(n)) return n.toLocaleString('fa-IR');
  return toPersianDigits(value);
}

function formatInstallmentDate(value) {
  if (!value) return '-';
  const raw = String(value).trim();
  const digits = raw.replace(/\D/g, '');
  if (digits.length >= 8) {
    return toPersianDigits(`${digits.slice(0, 4)}/${digits.slice(4, 6)}/${digits.slice(6, 8)}`);
  }
  return toPersianDigits(raw);
}

function formatNosaziCode(value) {
  if (!value) return '-';
  return toPersianDigits(String(value).trim());
}

function formatOdooatPlan(row) {
  return row.willApplyEndState ? 'بله' : 'خیر';
}

function renderInstallmentPreview(data) {
  const section = $('installmentPreviewSection');
  const tbody = $('installmentPreviewTable')?.querySelector('tbody');
  const summary = $('installmentPreviewSummary');
  const updateBtn = $('btnInstallmentUpdate');
  if (!section || !tbody) return;

  const items = data.items || [];
  section.hidden = false;
  if (summary) {
    summary.textContent = `ردیف یافت شد: ${data.foundCount || 0} | بدون نتیجه: ${data.notFoundCount || 0} | تطابق کامل: ${data.matchedCount || 0} | عدم تطابق: ${data.mismatchCount || 0}`;
  }
  tbody.innerHTML = '';
  items.forEach((row, idx) => {
    const tr = document.createElement('tr');
    if (!row.found) tr.classList.add('row-not-found');
    else if (row.found && row.dataMatches === false) tr.classList.add('row-mismatch');
    const kind = row.detectedLookupKind || '';
    const rowNum = row.rowIndex || idx + 1;
    const status = !row.found
      ? (row.validationMessage || 'یافت نشد')
      : (row.dataMatches ? 'تطابق کامل' : (row.validationMessage || 'عدم تطابق'));

    tr.innerHTML = `
      <td class="col-installment-row">${toPersianDigits(rowNum)}</td>
      <td class="col-installment-detect">${installmentLookupLabels[kind] || kind || '—'}</td>
      <td class="col-installment-nodoc">${toPersianDigits(row.noDocument || '-')}</td>
      <td class="col-installment-tracking">${toPersianDigits(row.trackingNo || '-')}</td>
      <td class="col-installment-cost">${formatInstallmentCost(row.paymentCost)}</td>
      <td class="col-installment-date">${formatInstallmentDate(row.paymentDate)}</td>
      <td class="col-installment-workitem">${toPersianDigits(row.nidWorkItem || '-')}</td>
      <td class="col-installment-nosazi">${formatNosaziCode(row.nosaziCode)}</td>
      <td class="col-installment-odooat">${formatOdooatPlan(row)}</td>
      <td class="col-installment-status">${status}</td>
      <td class="col-installment-comments">${row.proposedComments || '-'}</td>
    `;
    tbody.appendChild(tr);
  });
  if (updateBtn) {
    updateBtn.disabled = !(data.matchedCount > 0);
    updateBtn.textContent = 'اعمال';
  }
}

function formatInstallmentUpdateResult(data) {
  const lines = (data.results || []).map((r) => {
    const count = data.dryRun ? (r.wouldUpdate || 0) : (r.rowsAffected || 0);
    const kind = installmentLookupLabels[r.detectedLookupKind] || r.detectedLookupKind || '';
    return `${r.lookupValue} [${kind}]: ${r.success ? 'OK' : 'FAIL'} (${count} ردیف) — ${r.message || ''}`;
  });
  const modeLine = data.excelMode ? 'حالت: اکسل' : 'حالت: تکی';
  return [
    '=== نتیجه UPDATE Installment_List ===',
    modeLine,
    `DryRun: ${data.dryRun}`,
    `ApplyEndState (عودت): ${data.applyEndState}`,
    data.dryRun
      ? `شبیه‌سازی — ${data.wouldUpdate || 0} ردیف UPDATE می‌شد | بدون نتیجه: ${data.notFound}${data.skippedMismatch ? ` | عدم تطابق: ${data.skippedMismatch}` : ''}`
      : `به‌روز: ${data.updated} | بدون نتیجه: ${data.notFound} | خطا: ${data.failed}${data.skippedMismatch ? ` | عدم تطابق: ${data.skippedMismatch}` : ''}`,
    '',
    ...lines
  ].join('\n');
}

function initFicheDateStatusControls() {
  const filterHost = $('ficheDateStatusFilters');
  const statusSelect = $('ficheDateNewStatus');
  if (filterHost) {
    filterHost.innerHTML = '';
    ficheDateStatusOrder.forEach((value) => {
      const labelText = ficheDateStatusLabels[value];
      const labelEl = document.createElement('label');
      labelEl.className = 'fiche-date-ms-option';
      labelEl.setAttribute('role', 'option');
      const input = document.createElement('input');
      input.type = 'checkbox';
      input.value = String(value);
      input.className = 'fiche-date-status-filter';
      if (value === 1) input.checked = true;
      const text = document.createElement('span');
      text.className = 'fiche-date-ms-option-text';
      text.textContent = `${value} — ${labelText}`;
      labelEl.appendChild(input);
      labelEl.appendChild(text);
      filterHost.appendChild(labelEl);
    });
    setupFicheDateStatusDropdown();
    updateFicheDateStatusTriggerLabel();
  }
  if (statusSelect) {
    statusSelect.innerHTML = '';
    ficheDateStatusOrder.forEach((value) => {
      const label = ficheDateStatusLabels[value];
      const opt = document.createElement('option');
      opt.value = String(value);
      opt.textContent = `${value} — ${label}`;
      if (value === 1) opt.selected = true;
      statusSelect.appendChild(opt);
    });
  }
  syncFicheDateApplyFields();
}

function updateFicheDateStatusTriggerLabel() {
  const labelEl = $('ficheDateStatusTriggerLabel');
  if (!labelEl) return;
  const selected = getSelectedFicheDateStatuses();
  if (selected.length === 0) {
    labelEl.textContent = 'انتخاب وضعیت…';
    return;
  }
  if (selected.length === 1) {
    const value = selected[0];
    labelEl.textContent = `${value} — ${ficheDateStatusLabels[value] || value}`;
    return;
  }
  labelEl.textContent = `${selected.length.toLocaleString('fa-IR')} وضعیت انتخاب‌شده`;
}

function closeFicheDateStatusMenu() {
  const menu = $('ficheDateStatusFilters');
  const trigger = $('ficheDateStatusTrigger');
  if (menu) menu.hidden = true;
  if (trigger) trigger.setAttribute('aria-expanded', 'false');
}

function toggleFicheDateStatusMenu() {
  const menu = $('ficheDateStatusFilters');
  const trigger = $('ficheDateStatusTrigger');
  if (!menu || !trigger) return;
  const open = menu.hidden;
  menu.hidden = !open;
  trigger.setAttribute('aria-expanded', open ? 'true' : 'false');
}

function setupFicheDateStatusDropdown() {
  const trigger = $('ficheDateStatusTrigger');
  const menu = $('ficheDateStatusFilters');
  if (!trigger || !menu || trigger.dataset.bound === '1') return;
  trigger.dataset.bound = '1';

  trigger.addEventListener('click', (e) => {
    e.stopPropagation();
    toggleFicheDateStatusMenu();
  });

  menu.querySelectorAll('.fiche-date-status-filter').forEach((input) => {
    input.addEventListener('change', updateFicheDateStatusTriggerLabel);
  });

  document.addEventListener('click', (e) => {
    const wrap = $('ficheDateStatusMulti');
    if (!wrap || wrap.contains(e.target)) return;
    closeFicheDateStatusMenu();
  });

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeFicheDateStatusMenu();
  });
}

function syncFicheDateApplyFields() {
  const pairs = [
    ['ficheDateApplyPermanent', 'ficheDateNewPermanent'],
    ['ficheDateApplyTemporary', 'ficheDateNewTemporary'],
    ['ficheDateApplyBreak', 'ficheDateNewBreak']
  ];
  pairs.forEach(([chkId, inputId]) => {
    const chk = $(chkId);
    const input = $(inputId);
    if (!chk || !input) return;
    input.disabled = !chk.checked;
  });
  const statusChk = $('ficheDateApplyStatus');
  const statusSel = $('ficheDateNewStatus');
  if (statusChk && statusSel) statusSel.disabled = !statusChk.checked;
}

function updateFicheDateIdentifierHint() {
  const input = $('ficheDateIdentifier');
  const hint = $('ficheDateIdentifierHint');
  if (!input || !hint) return;
  const value = (input.value || '').trim();
  if (!value) {
    hint.hidden = true;
    hint.textContent = '';
    return;
  }
  const type = detectIdentifierType(value);
  if (!type) {
    hint.hidden = true;
    return;
  }
  hint.hidden = false;
  hint.textContent = `تشخیص: ${identifierTypeLabels[type] || type}`;
}

async function searchFicheDateAccountGroups(query, limit = 20) {
  const params = new URLSearchParams();
  const trimmed = (query || '').trim();
  if (trimmed) params.set('q', trimmed);
  params.set('limit', String(limit));
  const res = await apiFetch(`/api/fiche-date/account-groups?${params.toString()}`);
  const data = await parseJsonResponse(res);
  if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
  return data.titles || [];
}

let accountGroupSearchTimer = null;
let accountGroupSearchSeq = 0;

function closeFicheDateAccountGroupMenu() {
  const input = $('ficheDateAccountGroup');
  const menu = $('ficheDateAccountGroupMenu');
  if (!input || !menu) return;
  menu.hidden = true;
  input.setAttribute('aria-expanded', 'false');
}

function renderFicheDateAccountGroupMenu(titles) {
  const input = $('ficheDateAccountGroup');
  const menu = $('ficheDateAccountGroupMenu');
  if (!input || !menu) return;

  menu.innerHTML = '';
  if (!titles.length) {
    const empty = document.createElement('div');
    empty.className = 'account-group-combobox-empty';
    empty.textContent = 'موردی یافت نشد';
    menu.appendChild(empty);
    menu.hidden = false;
    input.setAttribute('aria-expanded', 'true');
    return;
  }

  titles.forEach((title) => {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'account-group-combobox-option';
    btn.textContent = title;
    btn.addEventListener('click', () => {
      input.value = title;
      closeFicheDateAccountGroupMenu();
    });
    menu.appendChild(btn);
  });
  menu.hidden = false;
  input.setAttribute('aria-expanded', 'true');
}

async function fetchFicheDateAccountGroupSuggestions(query) {
  const seq = ++accountGroupSearchSeq;
  try {
    const titles = await searchFicheDateAccountGroups(query);
    if (seq !== accountGroupSearchSeq) return;
    renderFicheDateAccountGroupMenu(titles);
  } catch {
    if (seq !== accountGroupSearchSeq) return;
    closeFicheDateAccountGroupMenu();
  }
}

function setupFicheDateAccountGroupLazyLoad() {
  if (!canAccessFicheDateChange()) return;
  const input = $('ficheDateAccountGroup');
  const menu = $('ficheDateAccountGroupMenu');
  if (!input || !menu || input.dataset.lazyBound === '1') return;
  input.dataset.lazyBound = '1';

  const scheduleSearch = () => {
    clearTimeout(accountGroupSearchTimer);
    accountGroupSearchTimer = setTimeout(() => {
      fetchFicheDateAccountGroupSuggestions(input.value);
    }, 300);
  };

  input.addEventListener('input', scheduleSearch);
  input.addEventListener('focus', () => {
    if (menu.hidden) scheduleSearch();
  });

  document.addEventListener('click', (e) => {
    if (e.target === input || menu.contains(e.target)) return;
    closeFicheDateAccountGroupMenu();
  });
}

function getSelectedFicheDateStatuses() {
  return Array.from(document.querySelectorAll('.fiche-date-status-filter:checked'))
    .map((el) => parseInt(el.value, 10))
    .filter((n) => !Number.isNaN(n));
}

function getFicheDateSearchPayload(page = ficheDateSearchState.page) {
  const pageSize = parseInt($('ficheDatePageSize')?.value || ficheDateSearchState.pageSize, 10) || 25;
  ficheDateSearchState.pageSize = pageSize;
  return {
    identifierValue: ($('ficheDateIdentifier')?.value || '').trim(),
    permanentFromDate: ($('ficheDatePermanentFrom')?.value || '').trim(),
    permanentToDate: ($('ficheDatePermanentTo')?.value || '').trim(),
    temporaryFromDate: ($('ficheDateTemporaryFrom')?.value || '').trim(),
    temporaryToDate: ($('ficheDateTemporaryTo')?.value || '').trim(),
    accountGroupTitle: ($('ficheDateAccountGroup')?.value || '').trim(),
    eumFicheStatuses: getSelectedFicheDateStatuses(),
    page,
    pageSize
  };
}

function hasFicheDateSearchFilter(payload = getFicheDateSearchPayload()) {
  return !!(payload.identifierValue
    || payload.permanentFromDate || payload.permanentToDate
    || payload.temporaryFromDate || payload.temporaryToDate
    || payload.accountGroupTitle
    || (payload.eumFicheStatuses && payload.eumFicheStatuses.length > 0));
}

function updateFicheDatePaginationUi() {
  const bar = $('ficheDatePagination');
  const label = $('ficheDatePageLabel');
  const prevBtn = $('btnFicheDatePrevPage');
  const nextBtn = $('btnFicheDateNextPage');
  if (!bar || !label || !prevBtn || !nextBtn) return;

  const { page, totalPages, totalCount } = ficheDateSearchState;
  const hasResults = totalCount > 0;
  bar.hidden = !hasResults;

  if (!hasResults) {
    label.textContent = '';
    prevBtn.disabled = true;
    nextBtn.disabled = true;
    return;
  }

  label.textContent = `صفحه ${page.toLocaleString('fa-IR')} از ${totalPages.toLocaleString('fa-IR')} — ${totalCount.toLocaleString('fa-IR')} مورد`;
  prevBtn.disabled = page <= 1;
  nextBtn.disabled = page >= totalPages;
}

function syncFicheDateUpdateButton() {
  const btn = $('btnFicheDateUpdate');
  if (!btn) return;
  btn.disabled = selectedFicheDateNos.size === 0;
}

function renderFicheDateTable(items, meta = {}) {
  ficheDateItems = items || [];
  if (meta.page != null) ficheDateSearchState.page = meta.page;
  if (meta.pageSize != null) ficheDateSearchState.pageSize = meta.pageSize;
  if (meta.totalCount != null) ficheDateSearchState.totalCount = meta.totalCount;
  if (meta.totalPages != null) ficheDateSearchState.totalPages = meta.totalPages;

  const section = $('ficheDateResultsSection');
  const tbody = $('ficheDateTable')?.querySelector('tbody');
  const countLabel = $('ficheDateCountLabel');
  const selectAll = $('ficheDateSelectAll');
  if (!section || !tbody) return;

  tbody.innerHTML = '';
  if (ficheDateItems.length === 0) {
    section.hidden = false;
    if (countLabel) {
      countLabel.textContent = ficheDateSearchState.totalCount > 0
        ? `۰ مورد در این صفحه — ${ficheDateSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`
        : 'نتیجه‌ای یافت نشد';
    }
    if (selectAll) selectAll.checked = false;
    updateFicheDatePaginationUi();
    syncFicheDateUpdateButton();
    return;
  }

  ficheDateItems.forEach((item) => {
    const checked = selectedFicheDateNos.has(item.ficheNo) ? ' checked' : '';
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td class="col-check"><input type="checkbox" class="fiche-date-row-check" data-fiche-no="${item.ficheNo}"${checked} /></td>
      <td>${toPersianDigits(item.ficheNo || '-')}</td>
      <td>${toPersianDigits(item.billId || '-')}</td>
      <td>${toPersianDigits(item.paymentId || '-')}</td>
      <td>${item.accountGroupTitle || '-'}</td>
      <td>${toPersianDigits(item.exportTemporaryDate || '-')}</td>
      <td>${toPersianDigits(item.exportPermanentDate || '-')}</td>
      <td>${toPersianDigits(item.paymentBreakDate || '-')}</td>
      <td>${toPersianDigits(item.paymentDate || '-')}</td>
      <td>${item.eumFicheStatusLabel || item.eumFicheStatus}</td>
    `;
    tbody.appendChild(tr);
  });

  tbody.querySelectorAll('.fiche-date-row-check').forEach((cb) => {
    cb.addEventListener('change', () => {
      const ficheNo = cb.dataset.ficheNo;
      if (cb.checked) selectedFicheDateNos.add(ficheNo);
      else selectedFicheDateNos.delete(ficheNo);
      syncFicheDateUpdateButton();
      if (selectAll) {
        const all = tbody.querySelectorAll('.fiche-date-row-check');
        selectAll.checked = all.length > 0 && Array.from(all).every((x) => x.checked);
      }
      updateFicheDateCountLabel();
    });
  });

  section.hidden = false;
  updateFicheDateCountLabel();
  if (selectAll) {
    const selectedOnPage = ficheDateItems.filter((item) => selectedFicheDateNos.has(item.ficheNo)).length;
    selectAll.checked = ficheDateItems.length > 0 && selectedOnPage === ficheDateItems.length;
  }
  updateFicheDatePaginationUi();
  syncFicheDateUpdateButton();
}

function updateFicheDateCountLabel() {
  const countLabel = $('ficheDateCountLabel');
  if (!countLabel) return;
  const selectedTotal = selectedFicheDateNos.size;
  const pageInfo = `${ficheDateItems.length.toLocaleString('fa-IR')} مورد در این صفحه — ${ficheDateSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`;
  countLabel.textContent = selectedTotal > 0
    ? `${pageInfo} | ${selectedTotal.toLocaleString('fa-IR')} انتخاب‌شده`
    : pageInfo;
}

async function fetchFicheDateResults(page = 1, { clearSelection = false } = {}) {
  const payload = getFicheDateSearchPayload(page);
  if (!hasFicheDateSearchFilter(payload)) {
    showAppWarning('حداقل یک فیلتر وارد کنید');
    return false;
  }

  if (clearSelection) selectedFicheDateNos.clear();
  ficheDateSearchState.page = page;

  const btn = $('btnFicheDateSearch');
  const prevBtn = $('btnFicheDatePrevPage');
  const nextBtn = $('btnFicheDateNextPage');
  const box = $('ficheDateResultBox');
  if (btn) btn.disabled = true;
  if (prevBtn) prevBtn.disabled = true;
  if (nextBtn) nextBtn.disabled = true;
  const prevLabel = btn?.textContent;
  if (btn) btn.textContent = 'در حال جستجو…';
  if (box) box.hidden = true;

  try {
    const res = await apiFetch('/api/fiche-date/search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);

    const totalCount = data.totalCount ?? data.count ?? 0;
    const totalPages = data.totalPages ?? (data.pageSize > 0 ? Math.ceil(totalCount / data.pageSize) : 0);
    renderFicheDateTable(data.items || [], {
      page: data.page ?? page,
      pageSize: data.pageSize ?? payload.pageSize,
      totalCount,
      totalPages
    });
    return true;
  } catch (e) {
    showAppError(e.message);
    renderFicheDateTable([], { page: 1, pageSize: payload.pageSize, totalCount: 0, totalPages: 0 });
    return false;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = prevLabel || 'جستجو';
    }
    updateFicheDatePaginationUi();
  }
}

function getFicheDateUpdatePayload() {
  const applyStatus = !!$('ficheDateApplyStatus')?.checked;
  return {
    ficheNos: Array.from(selectedFicheDateNos),
    applyExportPermanentDate: !!$('ficheDateApplyPermanent')?.checked,
    newExportPermanentDate: ($('ficheDateNewPermanent')?.value || '').trim(),
    applyExportTemporaryDate: !!$('ficheDateApplyTemporary')?.checked,
    newExportTemporaryDate: ($('ficheDateNewTemporary')?.value || '').trim(),
    applyPaymentBreakDate: !!$('ficheDateApplyBreak')?.checked,
    newPaymentBreakDate: ($('ficheDateNewBreak')?.value || '').trim(),
    applyEumFicheStatus: applyStatus,
    newEumFicheStatus: applyStatus ? parseInt($('ficheDateNewStatus')?.value || '1', 10) : null
  };
}

function formatFicheDateUpdateResult(data) {
  const lines = (data.results || []).map((r) =>
    `${r.ficheNo}: ${r.success ? 'OK' : 'FAIL'} — ${r.message || ''}`
  );
  return [
    '=== نتیجه UPDATE Income_Fiche ===',
    `DryRun: ${data.dryRun}`,
    data.dryRun
      ? `شبیه‌سازی — ${data.wouldUpdate || 0} فیش UPDATE می‌شد | بدون نتیجه: ${data.notFound}`
      : `به‌روز: ${data.updated} | بدون نتیجه: ${data.notFound} | خطا: ${data.failed}`,
    '',
    ...lines
  ].join('\n');
}

let bankInquiryItems = [];
const selectedBankInquiryNos = new Set();
const bankInquirySearchState = {
  page: 1,
  pageSize: 25,
  totalCount: 0,
  totalPages: 0
};

function getBankInquirySearchPayload(page = bankInquirySearchState.page) {
  const pageSize = parseInt($('bankInquiryPageSize')?.value || bankInquirySearchState.pageSize, 10) || 25;
  bankInquirySearchState.pageSize = pageSize;
  return {
    paymentDate: ($('bankInquirySearchPaymentDate')?.value || '').trim(),
    ficheNo: ($('bankInquiryFicheNo')?.value || '').trim(),
    billId: ($('bankInquiryBillId')?.value || '').trim(),
    paymentId: ($('bankInquiryPaymentId')?.value || '').trim(),
    page,
    pageSize
  };
}

function validateBankInquirySearchPayload(payload) {
  const hasDate = !!payload.paymentDate;
  const hasFicheNo = !!payload.ficheNo;
  const hasBillPayment = !!payload.billId && !!payload.paymentId;
  if (!hasDate && !hasFicheNo && !hasBillPayment) {
    return 'حداقل یکی از فیلترها را وارد کنید: تاریخ پرداخت، شماره فیش، یا شناسه قبض و شناسه پرداخت';
  }
  if ((payload.billId && !payload.paymentId) || (!payload.billId && payload.paymentId)) {
    return 'هر دو فیلد شناسه قبض و شناسه پرداخت الزامی است';
  }
  return null;
}

function updateBankInquiryPaginationUi() {
  const bar = $('bankInquiryPagination');
  const label = $('bankInquiryPageLabel');
  const prevBtn = $('btnBankInquiryPrevPage');
  const nextBtn = $('btnBankInquiryNextPage');
  if (!bar || !label || !prevBtn || !nextBtn) return;

  const { page, totalPages, totalCount } = bankInquirySearchState;
  const show = totalCount > 0;
  bar.hidden = !show;
  if (!show) return;

  label.textContent = `صفحه ${page.toLocaleString('fa-IR')} از ${Math.max(totalPages, 1).toLocaleString('fa-IR')}`;
  prevBtn.disabled = page <= 1;
  nextBtn.disabled = page >= totalPages;
}

function syncBankInquiryConfirmButton() {
  const btn = $('btnBankInquiryConfirm');
  if (!btn) return;
  btn.disabled = selectedBankInquiryNos.size === 0;
  const applySection = $('bankInquiryApplySection');
  if (applySection) applySection.hidden = selectedBankInquiryNos.size === 0;
}

function updateBankInquiryCountLabel() {
  const countLabel = $('bankInquiryCountLabel');
  if (!countLabel) return;
  const selectedTotal = selectedBankInquiryNos.size;
  const pageInfo = `${bankInquiryItems.length.toLocaleString('fa-IR')} مورد در این صفحه — ${bankInquirySearchState.totalCount.toLocaleString('fa-IR')} مورد کل`;
  countLabel.textContent = selectedTotal > 0
    ? `${pageInfo} — ${selectedTotal.toLocaleString('fa-IR')} انتخاب‌شده`
    : pageInfo;
}

function renderBankInquiryTable(items, meta = {}) {
  bankInquiryItems = items || [];
  if (meta.page != null) bankInquirySearchState.page = meta.page;
  if (meta.pageSize != null) bankInquirySearchState.pageSize = meta.pageSize;
  if (meta.totalCount != null) bankInquirySearchState.totalCount = meta.totalCount;
  if (meta.totalPages != null) bankInquirySearchState.totalPages = meta.totalPages;

  const section = $('bankInquiryResultsSection');
  const tbody = $('bankInquiryTable')?.querySelector('tbody');
  const selectAll = $('bankInquirySelectAll');
  if (!section || !tbody) return;

  section.hidden = false;

  if (bankInquiryItems.length === 0) {
    tbody.innerHTML = '<tr><td colspan="9" class="empty-row">موردی یافت نشد</td></tr>';
    updateBankInquiryCountLabel();
    updateBankInquiryPaginationUi();
    syncBankInquiryConfirmButton();
    return;
  }

  tbody.innerHTML = bankInquiryItems.map((item) => {
    const checked = selectedBankInquiryNos.has(item.ficheNo) ? ' checked' : '';
    return `<tr>
      <td class="col-check"><input type="checkbox" class="bank-inquiry-row-check" data-fiche-no="${item.ficheNo}"${checked} /></td>
      <td>${toPersianDigits(item.ficheNo || '-')}</td>
      <td>${toPersianDigits(item.billId || '-')}</td>
      <td>${toPersianDigits(item.paymentId || '-')}</td>
      <td>${formatShamsiDisplay(item.paymentDate)}</td>
      <td>${formatShamsiDisplay(item.bankPaymentDate)}</td>
      <td>${item.eumFicheStatusLabel || item.eumFicheStatus}</td>
      <td>${formatShamsiDisplay(item.userConfirmDate)}</td>
      <td>${item.usernameUserConfirm || '-'}</td>
    </tr>`;
  }).join('');

  tbody.querySelectorAll('.bank-inquiry-row-check').forEach((cb) => {
    cb.addEventListener('change', () => {
      const ficheNo = cb.dataset.ficheNo;
      if (cb.checked) selectedBankInquiryNos.add(ficheNo);
      else selectedBankInquiryNos.delete(ficheNo);
      syncBankInquiryConfirmButton();
      if (selectAll) {
        const all = tbody.querySelectorAll('.bank-inquiry-row-check');
        selectAll.checked = all.length > 0 && Array.from(all).every((x) => x.checked);
      }
      updateBankInquiryCountLabel();
      prefillBankInquiryNewPaymentDate();
    });
  });

  updateBankInquiryCountLabel();
  if (selectAll) {
    const selectedOnPage = bankInquiryItems.filter((item) => selectedBankInquiryNos.has(item.ficheNo)).length;
    selectAll.checked = bankInquiryItems.length > 0 && selectedOnPage === bankInquiryItems.length;
  }
  updateBankInquiryPaginationUi();
  syncBankInquiryConfirmButton();
}

function prefillBankInquiryNewPaymentDate() {
  const input = $('bankInquiryNewPaymentDate');
  if (!input || input.value.trim()) return;
  const selected = bankInquiryItems.filter((item) => selectedBankInquiryNos.has(item.ficheNo));
  if (selected.length === 1 && selected[0].paymentDate) {
    input.value = formatShamsiDisplay(selected[0].paymentDate);
  }
}

async function fetchBankInquiryResults(page = 1, { clearSelection = false } = {}) {
  const payload = getBankInquirySearchPayload(page);
  const validationError = validateBankInquirySearchPayload(payload);
  if (validationError) return showAppWarning(validationError);

  if (clearSelection) selectedBankInquiryNos.clear();
  bankInquirySearchState.page = page;

  const btn = $('btnBankInquirySearch');
  const prevBtn = $('btnBankInquiryPrevPage');
  const nextBtn = $('btnBankInquiryNextPage');
  const box = $('bankInquiryResultBox');
  if (btn) btn.disabled = true;
  if (prevBtn) prevBtn.disabled = true;
  if (nextBtn) nextBtn.disabled = true;

  try {
    const res = await apiFetch('/api/bank-inquiry/search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
    renderBankInquiryTable(data.items || [], {
      page: data.page,
      pageSize: data.pageSize,
      totalCount: data.totalCount,
      totalPages: data.totalPages
    });
    if (box) box.hidden = true;
  } catch (e) {
    renderBankInquiryTable([], { page: 1, pageSize: payload.pageSize, totalCount: 0, totalPages: 0 });
    if (box) {
      box.hidden = false;
      box.textContent = e.message;
    }
    showAppError(e.message);
  } finally {
    if (btn) btn.disabled = false;
    updateBankInquiryPaginationUi();
  }
}

function getBankInquiryConfirmPayload() {
  return {
    ficheNos: Array.from(selectedBankInquiryNos),
    newPaymentDate: ($('bankInquiryNewPaymentDate')?.value || '').trim()
  };
}

function validateBankInquiryConfirmPayload(payload) {
  if (!payload.ficheNos.length) return 'حداقل یک فیش از نتایج انتخاب کنید';
  if (!payload.newPaymentDate) return 'تاریخ پرداخت جدید را وارد کنید';
  return null;
}

function formatBankInquiryConfirmResult(data) {
  const lines = (data.results || []).map((r) =>
    `${r.ficheNo}: ${r.success ? 'OK' : 'FAIL'} — ${r.message || ''}`
  );
  return [
    '=== نتیجه UPDATE Income_Fiche (تایید استعلام بانک) ===',
    `DryRun: ${data.dryRun}`,
    data.dryRun
      ? `شبیه‌سازی — ${data.wouldUpdate || 0} فیش UPDATE می‌شد | بدون نتیجه: ${data.notFound}`
      : `به‌روز: ${data.updated || 0} | بدون نتیجه: ${data.notFound} | خطا: ${data.failed}`,
    `PaymentDate جدید: ${data.paymentDate || '-'}`,
    `UserConfirmDate: ${data.userConfirmDate || '-'}`,
    `UsernameUserConfirm: ${data.usernameUserConfirm || '-'}`,
    `EumFicheStatus → ${data.newEumFicheStatus}`,
    `EumIncomePaymentType → ${data.newEumIncomePaymentType}`,
    '',
    ...lines
  ].join('\n');
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
  return Array.from(selectedUnsentFicheNos);
}

function getUnsentSearchFilters() {
  const fromDate = ($('unsentFromDate')?.value || '').trim();
  const toDate = ($('unsentToDate')?.value || '').trim();
  return {
    ficheKind: $('unsentFicheKind').value,
    ficheNo: ($('unsentFicheNo')?.value || '').trim(),
    fromDate: fromDate || null,
    toDate: toDate || null,
    billId: ($('unsentBillId')?.value || '').trim(),
    paymentId: ($('unsentPaymentId')?.value || '').trim(),
    district: branchIdToDistrict($('unsentDistrict')?.value)
  };
}

function validateUnsentSearchFilters(filters) {
  if ((filters.fromDate && !filters.toDate) || (!filters.fromDate && filters.toDate)) {
    return 'هر دو تاریخ از و تا را وارد کنید';
  }
  if (!filters.fromDate || !filters.toDate) {
    return 'بازه تاریخ (از و تا) برای جستجوی فیش‌های ارسال‌نشده الزامی است';
  }
  return null;
}

function updateUnsentPaginationUi() {
  const bar = $('unsentPagination');
  const label = $('unsentPageLabel');
  const prevBtn = $('btnUnsentPrevPage');
  const nextBtn = $('btnUnsentNextPage');
  if (!bar || !label || !prevBtn || !nextBtn) return;

  const { page, totalPages, totalCount } = unsentSearchState;
  const hasResults = totalCount > 0;
  bar.hidden = !hasResults;

  if (!hasResults) {
    label.textContent = '';
    prevBtn.disabled = true;
    nextBtn.disabled = true;
    return;
  }

  label.textContent = `صفحه ${page.toLocaleString('fa-IR')} از ${totalPages.toLocaleString('fa-IR')} — ${totalCount.toLocaleString('fa-IR')} مورد`;
  prevBtn.disabled = page <= 1;
  nextBtn.disabled = page >= totalPages;
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

function renderUnsentTable(items, meta = {}) {
  unsentItems = items || [];
  if (meta.page != null) unsentSearchState.page = meta.page;
  if (meta.pageSize != null) unsentSearchState.pageSize = meta.pageSize;
  if (meta.totalCount != null) unsentSearchState.totalCount = meta.totalCount;
  if (meta.totalPages != null) unsentSearchState.totalPages = meta.totalPages;

  const section = $('unsentResultsSection');
  const tbody = $('unsentTable')?.querySelector('tbody');
  const countLabel = $('unsentCountLabel');
  const selectAll = $('unsentSelectAll');
  if (!section || !tbody) return;

  if (!unsentItems.length) {
    section.hidden = false;
    tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;color:var(--text-muted)">موردی یافت نشد</td></tr>';
    if (countLabel) {
      countLabel.textContent = unsentSearchState.totalCount > 0
        ? `۰ مورد در این صفحه — ${unsentSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`
        : '۰ مورد';
    }
    if (selectAll) selectAll.checked = false;
    updateUnsentPaginationUi();
    updateUnsentSendButton();
    return;
  }

  section.hidden = false;
  tbody.innerHTML = unsentItems.map((item) => {
    const checked = selectedUnsentFicheNos.has(item.ficheNo) ? ' checked' : '';
    return `
    <tr>
      <td class="col-check"><input type="checkbox" class="unsent-row-check" data-fiche-no="${item.ficheNo}"${checked} /></td>
      <td>${item.subKindLabel || (item.isTahator ? 'تهاتر' : '-')}</td>
      <td>${item.bnkAcntNo || '-'}</td>
      <td>${item.billId || '-'}</td>
      <td>${item.paymentId || '-'}</td>
      <td>${formatShamsiDisplay(item.bankPaymentDate)}</td>
      <td>${formatShamsiDisplay(item.paymentDate)}</td>
      <td>${item.ficheNo}</td>
      <td>${Number(item.payable || 0).toLocaleString()}</td>
    </tr>
  `;
  }).join('');

  const selectedOnPage = unsentItems.filter((item) => selectedUnsentFicheNos.has(item.ficheNo)).length;
  if (countLabel) {
    const selectedTotal = selectedUnsentFicheNos.size;
    const pageInfo = `${unsentItems.length.toLocaleString('fa-IR')} مورد در این صفحه — ${unsentSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`;
    countLabel.textContent = selectedTotal > 0
      ? `${pageInfo} | ${selectedTotal.toLocaleString('fa-IR')} انتخاب‌شده`
      : pageInfo;
  }
  if (selectAll) {
    selectAll.checked = unsentItems.length > 0 && selectedOnPage === unsentItems.length;
  }

  document.querySelectorAll('.unsent-row-check').forEach((cb) => {
    cb.addEventListener('change', () => {
      const ficheNo = cb.dataset.ficheNo;
      if (!ficheNo) return;
      if (cb.checked) selectedUnsentFicheNos.add(ficheNo);
      else selectedUnsentFicheNos.delete(ficheNo);

      const all = document.querySelectorAll('.unsent-row-check');
      const checked = document.querySelectorAll('.unsent-row-check:checked');
      if (selectAll) selectAll.checked = all.length > 0 && checked.length === all.length;
      updateUnsentSendButton();
      const countLabelEl = $('unsentCountLabel');
      if (countLabelEl && unsentSearchState.totalCount > 0) {
        const selectedTotal = selectedUnsentFicheNos.size;
        const pageInfo = `${unsentItems.length.toLocaleString('fa-IR')} مورد در این صفحه — ${unsentSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`;
        countLabelEl.textContent = selectedTotal > 0
          ? `${pageInfo} | ${selectedTotal.toLocaleString('fa-IR')} انتخاب‌شده`
          : pageInfo;
      }
    });
  });
  updateUnsentPaginationUi();
  updateUnsentSendButton();
}

async function fetchUnsentResults(page = 1, { clearSelection = false } = {}) {
  const filters = getUnsentSearchFilters();
  const validationError = validateUnsentSearchFilters(filters);
  if (validationError) {
    showAppWarning(validationError);
    return false;
  }

  const pageSize = parseInt($('unsentPageSize')?.value || unsentSearchState.pageSize, 10) || 25;
  unsentSearchState.pageSize = pageSize;
  unsentSearchState.filters = filters;
  if (clearSelection) selectedUnsentFicheNos.clear();

  const btn = $('btnUnsentSearch');
  const prevBtn = $('btnUnsentPrevPage');
  const nextBtn = $('btnUnsentNextPage');
  if (btn) btn.disabled = true;
  if (prevBtn) prevBtn.disabled = true;
  if (nextBtn) nextBtn.disabled = true;
  const prevLabel = btn?.textContent;
  if (btn) btn.textContent = 'در حال جستجو…';
  $('unsentResultBox').hidden = true;

  try {
    const res = await apiFetch('/api/unsent/search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...filters,
        page,
        pageSize
      })
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);

    const totalCount = data.totalCount ?? data.count ?? 0;
    const totalPages = data.totalPages ?? (data.pageSize > 0 ? Math.ceil(totalCount / data.pageSize) : 0);
    renderUnsentTable(data.items || [], {
      page: data.page ?? page,
      pageSize: data.pageSize ?? pageSize,
      totalCount,
      totalPages
    });
    return true;
  } catch (e) {
    showAppError(e.message);
    renderUnsentTable([], { page: 1, pageSize, totalCount: 0, totalPages: 0 });
    return false;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = prevLabel || 'جستجو';
    }
    updateUnsentPaginationUi();
  }
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

function redirectToLogin() {
  window.location.href = '/login.html';
}

async function apiFetch(url, options = {}) {
  const res = await fetch(url, { credentials: 'include', ...options });
  if (res.status === 401) {
    redirectToLogin();
    throw new Error('نشست منقضی شده — دوباره وارد شوید');
  }
  return res;
}

function isAdminUser() {
  return !!currentUser?.isAdmin;
}

function canAccessUnsent() {
  return isAdminUser() || !!currentUser?.canAccessUnsentFiches;
}

function canAccessInstallment() {
  return isAdminUser() || !!currentUser?.canAccessInstallment;
}

function canAccessFicheDateChange() {
  return isAdminUser() || !!currentUser?.canAccessFicheDateChange;
}

function canAccessBankInquiryConfirm() {
  return isAdminUser() || !!currentUser?.canAccessBankInquiryConfirm;
}

function canManageUsers() {
  return isAdminUser() || !!currentUser?.canManageUsers;
}

function isCenterUser() {
  return !isAdminUser() && getUserDistrict() === '102';
}

function applyPermissionUi() {
  document.querySelectorAll('.perm-unsent').forEach((el) => {
    el.hidden = !canAccessUnsent();
  });
  document.querySelectorAll('.perm-installment').forEach((el) => {
    el.hidden = !canAccessInstallment();
  });
  document.querySelectorAll('.perm-fiche-date').forEach((el) => {
    el.hidden = !canAccessFicheDateChange();
  });
  document.querySelectorAll('.perm-bank-inquiry').forEach((el) => {
    el.hidden = !canAccessBankInquiryConfirm();
  });
  document.querySelectorAll('.perm-users').forEach((el) => {
    el.hidden = !canManageUsers();
  });
  if (!canAccessUnsent() && rayvarzSendMode === 'bulk') {
    setRayvarzSendMode('single');
  }
}

function defaultMainTabKey() {
  return 'unsent';
}

function defaultRayvarzSendMode() {
  return canAccessUnsent() ? 'bulk' : 'single';
}

function applyAuthUi() {
  applyPermissionUi();

  const heroUser = $('heroUser');
  if (heroUser && currentUser) {
    heroUser.hidden = false;
    $('userDisplayName').textContent = currentUser.displayName || currentUser.username;
    const districtLabel = getUserDistrict() ? districtLabelFromValue(getUserDistrict()) : '';
    $('userRoleBadge').textContent = isAdminUser()
      ? 'ادمین'
      : isCenterUser()
        ? 'کاربر — شعبه مرکز'
        : (districtLabel ? `کاربر — ${districtLabel}` : 'کاربر');
    $('userRoleBadge').className = `user-badge ${isAdminUser() ? 'badge-admin' : 'badge-user'}`;
  }

  activateMainTab(defaultMainTabKey());
  setRayvarzSendMode(defaultRayvarzSendMode());
}

async function ensureAuthenticated() {
  const res = await fetch('/api/auth/me', { credentials: 'include' });
  if (!res.ok) {
    redirectToLogin();
    return false;
  }
  currentUser = await res.json();
  document.body.classList.add('app-authenticated');
  applyAuthUi();
  return true;
}

function districtLabelFromValue(value) {
  if (!value) return '—';
  if (String(value) === '102') return 'شعبه مرکز (۱۰۲)';
  const branch = config?.branches?.find((b) => branchIdToDistrict(String(b.id)) === String(value));
  return branch ? branch.name : value;
}

async function loadUsersTable() {
  if (!canManageUsers()) return;
  const res = await apiFetch('/api/admin/users');
  const data = await parseJsonResponse(res);
  cachedUsers = data.items || [];
  const tbody = $('usersTable')?.querySelector('tbody');
  if (!tbody) return;
  tbody.innerHTML = '';
  cachedUsers.forEach((u) => {
    const tr = document.createElement('tr');
    const groupNames = (u.groupIds || [])
      .map((id) => cachedGroups.find((g) => String(g.id) === String(id))?.name)
      .filter(Boolean)
      .join('، ') || '—';
    tr.innerHTML = `
      <td>${u.nationalId || u.username}</td>
      <td>${u.firstName || '—'}</td>
      <td>${u.lastName || '—'}</td>
      <td>${u.position || '—'}</td>
      <td>${districtLabelFromValue(u.district)}</td>
      <td>${u.isAdmin ? 'ادمین' : 'کاربر'}</td>
      <td>${groupNames}</td>
      <td>${u.isActive ? 'فعال' : 'غیرفعال'}</td>
      <td><button type="button" class="btn secondary btn-sm btn-edit-user" data-user-id="${u.id}">ویرایش</button></td>
    `;
    tbody.appendChild(tr);
  });
  tbody.querySelectorAll('.btn-edit-user').forEach((btn) => {
    btn.addEventListener('click', () => openUserEdit(btn.dataset.userId));
  });
}

let cachedGroups = [];
let cachedUsers = [];
let editingUserId = null;
let editingGroupId = null;

async function loadGroupsTable() {
  if (!canManageUsers()) return;
  const res = await apiFetch('/api/admin/groups');
  const data = await parseJsonResponse(res);
  cachedGroups = data.items || [];
  const tbody = $('groupsTable')?.querySelector('tbody');
  if (!tbody) return;
  tbody.innerHTML = '';
  cachedGroups.forEach((g) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${g.name}</td>
      <td>${g.canAccessUnsentFiches ? 'بله' : 'خیر'}</td>
      <td>${g.canAccessInstallment ? 'بله' : 'خیر'}</td>
      <td>${g.canAccessFicheDateChange ? 'بله' : 'خیر'}</td>
      <td>${g.canAccessBankInquiryConfirm ? 'بله' : 'خیر'}</td>
      <td>${g.canManageUsers ? 'بله' : 'خیر'}</td>
      <td><button type="button" class="btn secondary btn-sm btn-edit-group" data-group-id="${g.id}">ویرایش</button></td>
    `;
    tbody.appendChild(tr);
  });
  tbody.querySelectorAll('.btn-edit-group').forEach((btn) => {
    btn.addEventListener('click', () => openGroupEdit(btn.dataset.groupId));
  });
  renderUserGroupCheckboxes();
}

function openGroupEdit(groupId) {
  const group = cachedGroups.find((g) => String(g.id) === String(groupId));
  if (!group) return;
  editingGroupId = group.id;
  if ($('newGroupName')) $('newGroupName').value = group.name;
  if ($('newGroupUnsent')) $('newGroupUnsent').checked = group.canAccessUnsentFiches;
  if ($('newGroupInstallment')) $('newGroupInstallment').checked = group.canAccessInstallment;
  if ($('newGroupFicheDate')) $('newGroupFicheDate').checked = group.canAccessFicheDateChange;
  if ($('newGroupBankInquiry')) $('newGroupBankInquiry').checked = group.canAccessBankInquiryConfirm;
  if ($('newGroupUsers')) $('newGroupUsers').checked = group.canManageUsers;
  const btn = $('btnCreateGroup');
  if (btn) btn.textContent = 'بروزرسانی گروه';
}

function resetGroupForm() {
  editingGroupId = null;
  if ($('newGroupName')) $('newGroupName').value = '';
  if ($('newGroupUnsent')) $('newGroupUnsent').checked = false;
  if ($('newGroupInstallment')) $('newGroupInstallment').checked = false;
  if ($('newGroupFicheDate')) $('newGroupFicheDate').checked = false;
  if ($('newGroupBankInquiry')) $('newGroupBankInquiry').checked = false;
  if ($('newGroupUsers')) $('newGroupUsers').checked = false;
  const btn = $('btnCreateGroup');
  if (btn) btn.textContent = 'ثبت گروه';
}

async function saveGroupFromForm() {
  const payload = {
    name: ($('newGroupName')?.value || '').trim(),
    canAccessUnsentFiches: !!$('newGroupUnsent')?.checked,
    canAccessInstallment: !!$('newGroupInstallment')?.checked,
    canAccessFicheDateChange: !!$('newGroupFicheDate')?.checked,
    canAccessBankInquiryConfirm: !!$('newGroupBankInquiry')?.checked,
    canManageUsers: !!$('newGroupUsers')?.checked
  };
  if (!payload.name) return alert('نام گروه الزامی است');
  const url = editingGroupId
    ? `/api/admin/groups/${editingGroupId}`
    : '/api/admin/groups';
  const method = editingGroupId ? 'PUT' : 'POST';
  const res = await apiFetch(url, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  const data = await parseJsonResponse(res);
  if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
  resetGroupForm();
  await loadGroupsTable();
  await loadUsersTable();
}

function renderUserGroupCheckboxes() {
  const host = $('editUserGroups');
  if (!host) return;
  host.innerHTML = '';
  cachedGroups.forEach((g) => {
    const label = document.createElement('label');
    label.className = 'check-field';
    const input = document.createElement('input');
    input.type = 'checkbox';
    input.value = g.id;
    input.dataset.groupId = g.id;
    label.appendChild(input);
    label.appendChild(document.createTextNode(g.name));
    host.appendChild(label);
  });
}

function openUserEdit(userId) {
  const user = cachedUsers.find((u) => String(u.id) === String(userId));
  if (!user) return;
  editingUserId = user.id;
  const panel = $('userEditPanel');
  if (panel) panel.hidden = false;
  if ($('userEditTitle')) {
    $('userEditTitle').textContent = `${user.firstName} ${user.lastName} — ${user.nationalId || user.username}`;
  }
  if ($('editUserIsAdmin')) $('editUserIsAdmin').checked = !!user.isAdmin;
  if ($('editUserIsActive')) $('editUserIsActive').checked = !!user.isActive;
  if ($('editUserNewPassword')) $('editUserNewPassword').value = '';
  renderUserGroupCheckboxes();
  const selected = new Set((user.groupIds || []).map(String));
  $('editUserGroups')?.querySelectorAll('input[type=checkbox]').forEach((cb) => {
    cb.checked = selected.has(cb.dataset.groupId);
  });
}

function closeUserEdit() {
  editingUserId = null;
  const panel = $('userEditPanel');
  if (panel) panel.hidden = true;
}

async function saveUserEdit() {
  if (!editingUserId) return;
  const groupIds = [];
  $('editUserGroups')?.querySelectorAll('input[type=checkbox]:checked').forEach((cb) => {
    groupIds.push(cb.dataset.groupId);
  });
  const payload = {
    isAdmin: !!$('editUserIsAdmin')?.checked,
    isActive: !!$('editUserIsActive')?.checked,
    groupIds
  };
  const res = await apiFetch(`/api/admin/users/${editingUserId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  const data = await parseJsonResponse(res);
  if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
  closeUserEdit();
  await loadUsersTable();
}

async function resetUserPassword() {
  if (!editingUserId) return;
  const password = $('editUserNewPassword')?.value || '';
  if (!password || password.length < 6) return alert('رمز عبور جدید حداقل ۶ کاراکتر باشد');
  const res = await apiFetch(`/api/admin/users/${editingUserId}/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ password })
  });
  const data = await parseJsonResponse(res);
  if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
  if ($('editUserNewPassword')) $('editUserNewPassword').value = '';
  alert('رمز عبور با موفقیت تغییر کرد');
}

async function createUserFromForm() {
  const nationalId = ($('newUserNationalId')?.value || '').trim();
  const payload = {
    username: nationalId,
    password: $('newUserPassword')?.value || '',
    firstName: ($('newUserFirstName')?.value || '').trim(),
    lastName: ($('newUserLastName')?.value || '').trim(),
    nationalId,
    position: ($('newUserPosition')?.value || '').trim(),
    district: branchIdToDistrict($('newUserDistrict')?.value || ''),
    isAdmin: !!$('newUserIsAdmin')?.checked
  };
  if (!payload.firstName || !payload.lastName || !payload.nationalId || !payload.password) {
    return alert('نام، نام خانوادگی، کد ملی و رمز عبور الزامی است');
  }
  if (payload.nationalId.length !== 10 || !/^\d+$/.test(payload.nationalId)) {
    return alert('کد ملی باید ۱۰ رقم باشد');
  }
  if (!payload.isAdmin && !payload.district) {
    return alert('برای کاربر منطقه‌ای، انتخاب منطقه یا شعبه مرکز الزامی است');
  }
  const box = $('usersResultBox');
  try {
    const res = await apiFetch('/api/admin/users', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
    if (box) {
      box.hidden = false;
      box.textContent = `کاربر ${data.user?.nationalId || data.user?.username || nationalId} با موفقیت ثبت شد.`;
    }
    $('newUserPassword').value = '';
    $('newUserNationalId').value = '';
    await loadUsersTable();
  } catch (e) {
    if (box) {
      box.hidden = false;
      box.textContent = e.message;
    }
    alert(e.message);
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
  const authed = await ensureAuthenticated();
  if (!authed) return;

  try {
    const res = await apiFetch('/api/config');
    config = await parseJsonResponse(res);
    syncInstallmentDryRunUi();
  } catch (e) {
    alert(e.message);
    return;
  }

  const branchSel = $('branch');
  const fundSel = $('fund');
  fillBranchSelect(branchSel);
  fillBranchSelect($('unsentDistrict'), { includeAll: true });
  fillBranchSelect($('newUserDistrict'), { includeAll: true, allLabel: 'انتخاب منطقه' });
  applyRegionalUserRestrictions();
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
  const installmentInput = $('installmentValues');
  if (installmentInput) {
    installmentInput.addEventListener('input', () => {
      installmentInput.value = installmentInput.value.replace(/\D/g, '');
    });
  }

  document.querySelectorAll('.installment-mode-tab[data-installment-mode]').forEach((btn) => {
    btn.addEventListener('click', () => setInstallmentMode(btn.dataset.installmentMode));
  });
  document.querySelectorAll('[data-rayvarz-mode]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (btn.hidden) return;
      setRayvarzSendMode(btn.dataset.rayvarzMode);
    });
  });
  $('installmentApplyEndState')?.addEventListener('change', () => syncInstallmentOdooatCheckboxes('single'));
  $('installmentApplyEndStateExcel')?.addEventListener('change', () => syncInstallmentOdooatCheckboxes('excel'));
  setInstallmentMode('single');

  const excelInput = $('installmentExcelFile');
  if (excelInput) {
    excelInput.addEventListener('change', async () => {
      const status = $('installmentExcelStatus');
      const file = excelInput.files?.[0];
      installmentExcelRows = [];
      $('installmentPreviewSection').hidden = true;
      if ($('btnInstallmentUpdate')) $('btnInstallmentUpdate').disabled = true;
      if (!file) {
        if (status) status.textContent = 'فایلی انتخاب نشده';
        return;
      }
      if (status) status.textContent = 'در حال خواندن فایل…';
      try {
        installmentExcelRows = await parseInstallmentExcelFile(file);
        if (status) {
          status.textContent = `${file.name} — ${installmentExcelRows.length.toLocaleString('fa-IR')} ردیف خوانده شد`;
        }
      } catch (e) {
        if (status) status.textContent = e.message;
        excelInput.value = '';
        showAppError(e.message);
      }
    });
  }
  syncFundFromBranch();
  setupMainTabs();
  initDatePickers();
  initFicheDateStatusControls();
  $('ficheDateIdentifier')?.addEventListener('input', updateFicheDateIdentifierHint);
  ['ficheDateApplyPermanent', 'ficheDateApplyTemporary', 'ficheDateApplyBreak', 'ficheDateApplyStatus']
    .forEach((id) => $(id)?.addEventListener('change', syncFicheDateApplyFields));
  if (canAccessFicheDateChange()) {
    setupFicheDateAccountGroupLazyLoad();
  }
  window.addEventListener('load', initDatePickers);
  if (canManageUsers()) {
    await loadGroupsTable();
    await loadUsersTable();
  }
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
  if (!value) return showAppWarning('شناسه فیش را وارد کنید');

  $('btnLoad').disabled = true;
  try {
    const res = await apiFetch('/api/fiche/load', {
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
    showAppError(e.message);
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
    return showAppWarning(currentFiche.blockReason || currentFiche.statusMessage || 'این فیش قابل پیش‌نمایش نیست');
  }
  $('btnPreview').disabled = true;
  try {
    const res = await apiFetch('/api/fiche/preview', {
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
    showAppError(e.message);
  } finally {
    $('btnPreview').disabled = false;
  }
  });

  bindClick('btnSend', async () => {
  if (!currentFiche) return;
  if (!isTahatorIncomeFiche(currentFiche) && !currentFiche.canSend) {
    return showAppWarning(currentFiche.blockReason || currentFiche.statusMessage || 'این فیش قابل ارسال نیست');
  }

  if (isTahatorIncomeFiche(currentFiche)) {
    const dry = config?.tahator?.dryRun ?? config?.dryRun;
    const warn = dry
      ? `DryRun فعال — تهاتر ${currentFiche.ficheNo} فقط SOAP می‌سازد. ادامه؟`
      : `ارسال تهاتر ${currentFiche.ficheNo} به رایورز؟`;
    if (!confirm(warn)) return;
    $('btnSend').disabled = true;
    try {
      const res = await apiFetch('/api/tahator/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ficheNo: currentFiche.ficheNo, branch: 0, fund: 0 })
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      $('resultSection').hidden = false;
      showTahatorSendResult(data);
      if (data.previewXml || data.soapResponse) {
        $('xmlSection').hidden = false;
        $('xmlBox').textContent = data.soapResponse || data.previewXml;
      }
      if (data.dryRun) showAppInfo('DryRun تهاتر: SOAP ساخته شد؛ POST واقعی زده نشد.');
      else if (data.skipped) showAppWarning(data.message);
      else if (data.success) showAppSuccess(data.message || 'ارسال تهاتر موفق');
      else showAppError(data.message || (data.docNotSentError ? `عدم ارسال: ${data.docNotSentError}` : 'تهاتر ناموفق'));
    } catch (e) {
      showAppError(e.message);
    } finally {
      $('btnSend').disabled = false;
      updateSendButton(currentFiche);
    }
    return;
  }

  if (!confirm(`ارسال فیش ${currentFiche.ficheNo} به رایورز؟`)) return;

  $('btnSend').disabled = true;
  try {
    const res = await apiFetch('/api/fiche/send', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(getPayload(true))
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || data.detail || data.title || `خطا (HTTP ${res.status})`);

    $('resultSection').hidden = false;
    showSendResult(data);

    if (data.dryRun) {
      showAppInfo('توجه: DryRun فعال است — چیزی به رایورز ارسال نشد، فقط XML ساخته شد.');
    } else if (data.success && data.verifiedInRayvarz === false) {
      showAppWarning('هشدار: ارسال تأیید نشد — فیش در incmdocsys نیست. پاسخ SOAP و DocNotSent را ببینید.');
    } else if (!data.success) {
      showAppError(data.message || data.docNotSentError || 'ارسال ناموفق — Message و پاسخ SOAP را بررسی کنید.');
    } else if (data.success && data.verifiedInRayvarz) {
      showAppSuccess('فیش در رایورز ثبت شد (VerifiedInRayvarz=true).');
    }

    if (data.previewXml || data.soapResponse) {
      $('xmlSection').hidden = false;
      $('xmlBox').textContent = data.soapResponse || data.previewXml;
    }
    $('resultSection').scrollIntoView({ behavior: 'smooth', block: 'start' });
  } catch (e) {
    showAppError(e.message);
  } finally {
    $('btnSend').disabled = false;
    updateSendButton(currentFiche);
  }
  });

  const selectAll = $('unsentSelectAll');
  if (selectAll) {
    selectAll.addEventListener('change', () => {
      unsentItems.forEach((item) => {
        if (selectAll.checked) selectedUnsentFicheNos.add(item.ficheNo);
        else selectedUnsentFicheNos.delete(item.ficheNo);
      });
      document.querySelectorAll('.unsent-row-check').forEach((cb) => {
        cb.checked = selectAll.checked;
      });
      updateUnsentSendButton();
      const countLabelEl = $('unsentCountLabel');
      if (countLabelEl && unsentSearchState.totalCount > 0) {
        const selectedTotal = selectedUnsentFicheNos.size;
        const pageInfo = `${unsentItems.length.toLocaleString('fa-IR')} مورد در این صفحه — ${unsentSearchState.totalCount.toLocaleString('fa-IR')} مورد کل`;
        countLabelEl.textContent = selectedTotal > 0
          ? `${pageInfo} | ${selectedTotal.toLocaleString('fa-IR')} انتخاب‌شده`
          : pageInfo;
      }
    });
  }

  bindClick('btnUnsentSearch', async () => {
    await fetchUnsentResults(1, { clearSelection: true });
  });

  bindClick('btnUnsentPrevPage', async () => {
    if (unsentSearchState.page <= 1) return;
    await fetchUnsentResults(unsentSearchState.page - 1);
  });

  bindClick('btnUnsentNextPage', async () => {
    if (unsentSearchState.page >= unsentSearchState.totalPages) return;
    await fetchUnsentResults(unsentSearchState.page + 1);
  });

  const pageSizeSel = $('unsentPageSize');
  if (pageSizeSel) {
    pageSizeSel.addEventListener('change', async () => {
      if (!unsentSearchState.filters) return;
      await fetchUnsentResults(1);
    });
  }

  bindClick('btnUnsentPlan', async () => {
    const selected = getSelectedUnsentFicheNos();
    if (!selected.length) return showAppWarning('حداقل یک فیش انتخاب کنید');

    const btn = $('btnUnsentPlan');
    btn.disabled = true;
    const box = $('unsentResultBox');
    box.hidden = false;
    box.textContent = 'در حال بررسی مسیر ارسال هر فیش…';

    try {
      const res = await apiFetch('/api/unsent/plan-batch', {
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
      showAppError(e.message);
    } finally {
      updateUnsentSendButton();
    }
  });

  bindClick('btnUnsentSend', async () => {
    const selected = getSelectedUnsentFicheNos();
    if (!selected.length) return showAppWarning('حداقل یک فیش انتخاب کنید');

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
      const res = await apiFetch('/api/unsent/send-batch', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ficheKind: kind,
          ficheNos: selected
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

      if (data.dryRun) showAppInfo('DryRun: SOAP ساخته شد؛ POST واقعی زده نشد.');
      else if (data.failed > 0) {
        showAppWarning(`ارسال دسته‌ای تمام شد — موفق: ${data.succeeded}، ناموفق: ${data.failed}، رد: ${data.skipped}`);
      } else {
        showAppSuccess(`ارسال دسته‌ای تمام شد — موفق: ${data.succeeded}، ناموفق: ${data.failed}، رد: ${data.skipped}`);
      }

      selectedUnsentFicheNos.clear();
      if (unsentSearchState.filters) {
        await fetchUnsentResults(unsentSearchState.page);
      }
    } catch (e) {
      box.textContent = e.message;
      showAppError(e.message);
    } finally {
      updateUnsentSendButton();
    }
  });

  bindClick('btnInstallmentPreview', async () => {
    const payload = getInstallmentPayload();
    if (installmentMode === 'excel') {
      if (!payload.excelRows?.length) return showAppWarning('فایل اکسل را انتخاب کنید');
    } else if (!payload.valuesText) {
      return showAppWarning('حداقل یک شماره سند یا کد پیگیری وارد کنید');
    }

    const btn = $('btnInstallmentPreview');
    const box = $('installmentResultBox');
    btn.disabled = true;
    if (box) box.hidden = true;
    try {
      const res = await apiFetch('/api/installment/preview', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      renderInstallmentPreview(data);
    } catch (e) {
      showAppError(e.message);
      $('installmentPreviewSection').hidden = true;
      if ($('btnInstallmentUpdate')) $('btnInstallmentUpdate').disabled = true;
    } finally {
      btn.disabled = false;
    }
  });

  bindClick('btnInstallmentUpdate', async () => {
    const payload = getInstallmentPayload();
    if (installmentMode === 'excel') {
      if (!payload.excelRows?.length) return showAppWarning('فایل اکسل را انتخاب کنید');
    } else if (!payload.valuesText) {
      return showAppWarning('حداقل یک شماره سند یا کد پیگیری وارد کنید');
    }

    const dry = config?.installment?.dryRun ?? config?.dryRun ?? true;
    const dryNote = dry ? 'در حالت DryRun تغییری روی سرور اعمال نمی‌شود.' : '';
    if (dryNote && !confirm(`${dryNote}\n\nادامه؟`)) return;
    if (!dryNote && !confirm('ادامه؟')) return;

    const btn = $('btnInstallmentUpdate');
    const box = $('installmentResultBox');
    btn.disabled = true;
    if (box) {
      box.hidden = false;
      box.textContent = 'در حال اعمال…';
    }
    try {
      const res = await apiFetch('/api/installment/update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      if (box) box.textContent = formatInstallmentUpdateResult(data);
      if (data.dryRun) {
        showAppInfo(`${data.wouldUpdate || 0} ردیف — تغییری روی سرور اعمال نشد.`);
      } else {
        showAppSuccess(`UPDATE تمام شد — ${data.updated} ردیف به‌روز، ${data.notFound} بدون نتیجه`);
      }
      $('btnInstallmentPreview').click();
    } catch (e) {
      if (box) box.textContent = e.message;
      showAppError(e.message);
    } finally {
      btn.disabled = false;
    }
  });

  bindClick('btnFicheDateSearch', async () => {
    await fetchFicheDateResults(1, { clearSelection: true });
  });

  bindClick('btnFicheDatePrevPage', async () => {
    if (ficheDateSearchState.page <= 1) return;
    await fetchFicheDateResults(ficheDateSearchState.page - 1);
  });

  bindClick('btnFicheDateNextPage', async () => {
    if (ficheDateSearchState.page >= ficheDateSearchState.totalPages) return;
    await fetchFicheDateResults(ficheDateSearchState.page + 1);
  });

  const ficheDatePageSize = $('ficheDatePageSize');
  if (ficheDatePageSize) {
    ficheDatePageSize.addEventListener('change', async () => {
      if (ficheDateSearchState.totalCount > 0) {
        await fetchFicheDateResults(1);
      }
    });
  }

  const ficheDateSelectAll = $('ficheDateSelectAll');
  if (ficheDateSelectAll) {
    ficheDateSelectAll.addEventListener('change', () => {
      const tbody = $('ficheDateTable')?.querySelector('tbody');
      if (!tbody) return;
      tbody.querySelectorAll('.fiche-date-row-check').forEach((cb) => {
        cb.checked = ficheDateSelectAll.checked;
        const ficheNo = cb.dataset.ficheNo;
        if (ficheDateSelectAll.checked) selectedFicheDateNos.add(ficheNo);
        else selectedFicheDateNos.delete(ficheNo);
      });
      updateFicheDateCountLabel();
      syncFicheDateUpdateButton();
    });
  }

  bindClick('btnFicheDateUpdate', async () => {
    const payload = getFicheDateUpdatePayload();
    if (!payload.ficheNos.length) return showAppWarning('حداقل یک فیش انتخاب کنید');
    const hasChange = (payload.applyExportPermanentDate && payload.newExportPermanentDate)
      || (payload.applyExportTemporaryDate && payload.newExportTemporaryDate)
      || (payload.applyPaymentBreakDate && payload.newPaymentBreakDate)
      || (payload.applyEumFicheStatus && payload.newEumFicheStatus != null);
    if (!hasChange) return showAppWarning('حداقل یک فیلد برای تغییر مشخص کنید');

    const dry = config?.ficheDateChange?.dryRun ?? config?.dryRun ?? true;
    const warn = dry
      ? `DryRun فعال — ${payload.ficheNos.length} فیش فقط شبیه‌سازی می‌شود. ادامه؟`
      : `تغییر تاریخ/وضعیت ${payload.ficheNos.length} فیش در Income_Fiche؟`;
    if (!confirm(warn)) return;

    const btn = $('btnFicheDateUpdate');
    const box = $('ficheDateResultBox');
    btn.disabled = true;
    if (box) {
      box.hidden = false;
      box.textContent = 'در حال اعمال…';
    }
    try {
      const res = await apiFetch('/api/fiche-date/update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      if (box) box.textContent = formatFicheDateUpdateResult(data);
      if (data.dryRun) {
        showAppInfo(`${data.wouldUpdate || 0} فیش — تغییری روی سرور اعمال نشد (DryRun).`);
      } else {
        showAppSuccess(`UPDATE تمام شد — ${data.updated} فیش به‌روز، ${data.notFound} بدون نتیجه`);
        $('btnFicheDateSearch').click();
      }
    } catch (e) {
      if (box) box.textContent = e.message;
      showAppError(e.message);
    } finally {
      btn.disabled = false;
      syncFicheDateUpdateButton();
    }
  });

  bindClick('btnBankInquirySearch', async () => {
    await fetchBankInquiryResults(1, { clearSelection: true });
  });

  bindClick('btnBankInquiryPrevPage', async () => {
    if (bankInquirySearchState.page <= 1) return;
    await fetchBankInquiryResults(bankInquirySearchState.page - 1);
  });

  bindClick('btnBankInquiryNextPage', async () => {
    if (bankInquirySearchState.page >= bankInquirySearchState.totalPages) return;
    await fetchBankInquiryResults(bankInquirySearchState.page + 1);
  });

  $('bankInquiryPageSize')?.addEventListener('change', async () => {
    await fetchBankInquiryResults(1);
  });

  $('bankInquirySelectAll')?.addEventListener('change', (e) => {
    const tbody = $('bankInquiryTable')?.querySelector('tbody');
    const selectAll = e.target;
    if (!tbody || !selectAll) return;
    tbody.querySelectorAll('.bank-inquiry-row-check').forEach((cb) => {
      cb.checked = selectAll.checked;
      const ficheNo = cb.dataset.ficheNo;
      if (selectAll.checked) selectedBankInquiryNos.add(ficheNo);
      else selectedBankInquiryNos.delete(ficheNo);
    });
    updateBankInquiryCountLabel();
    syncBankInquiryConfirmButton();
    prefillBankInquiryNewPaymentDate();
  });

  bindClick('btnBankInquiryConfirm', async () => {
    const payload = getBankInquiryConfirmPayload();
    const validationError = validateBankInquiryConfirmPayload(payload);
    if (validationError) return showAppWarning(validationError);

    const dry = config?.bankInquiryConfirm?.dryRun ?? config?.dryRun ?? true;
    const warn = dry
      ? `DryRun فعال — تایید استعلام بانک برای ${payload.ficheNos.length} فیش فقط شبیه‌سازی می‌شود. ادامه؟`
      : `تایید استعلام بانک برای ${payload.ficheNos.length} فیش با تاریخ پرداخت «${payload.newPaymentDate}» ثبت شود؟`;
    if (!confirm(warn)) return;

    const btn = $('btnBankInquiryConfirm');
    const box = $('bankInquiryResultBox');
    btn.disabled = true;
    if (box) {
      box.hidden = false;
      box.textContent = 'در حال ثبت…';
    }
    try {
      const res = await apiFetch('/api/bank-inquiry/confirm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await parseJsonResponse(res);
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      if (box) box.textContent = formatBankInquiryConfirmResult(data);
      if (data.dryRun) {
        showAppInfo('تایید استعلام بانک — تغییری روی سرور اعمال نشد (DryRun).');
      } else {
        showAppSuccess(`تایید ثبت شد — ${data.updated} فیش به‌روز، ${data.notFound} بدون نتیجه`);
        await fetchBankInquiryResults(bankInquirySearchState.page);
      }
    } catch (e) {
      if (box) box.textContent = e.message;
      showAppError(e.message);
    } finally {
      syncBankInquiryConfirmButton();
    }
  });
}

function setupAuthAndAdminHandlers() {
  bindClick('btnLogout', async () => {
    try {
      await apiFetch('/api/auth/logout', { method: 'POST' });
    } catch {
      // ignore
    }
    redirectToLogin();
  });

  bindClick('btnCreateUser', createUserFromForm);
  bindClick('btnRefreshUsers', async () => {
    await loadGroupsTable();
    await loadUsersTable();
  });
  bindClick('btnCreateGroup', async () => {
    try {
      await saveGroupFromForm();
    } catch (e) {
      alert(e.message);
    }
  });
  bindClick('btnRefreshGroups', loadGroupsTable);
  bindClick('btnSaveUserEdit', async () => {
    try {
      await saveUserEdit();
    } catch (e) {
      alert(e.message);
    }
  });
  bindClick('btnResetUserPassword', async () => {
    try {
      await resetUserPassword();
    } catch (e) {
      alert(e.message);
    }
  });
  bindClick('btnCancelUserEdit', closeUserEdit);
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
setupAuthAndAdminHandlers();
init();
