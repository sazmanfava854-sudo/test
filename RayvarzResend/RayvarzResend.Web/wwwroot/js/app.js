let currentFiche = null;
let config = null;
let currentUser = null;
let unsentItems = [];
const selectedUnsentFicheNos = new Set();
const unsentSearchState = {
  page: 1,
  pageSize: 50,
  totalCount: 0,
  totalPages: 0,
  filters: null
};

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
    single: $('tabSingle'),
    installment: $('tabInstallment'),
    users: $('tabUsers')
  };

  tabs.forEach((tab) => {
    tab.addEventListener('click', () => {
      const key = tab.dataset.tab;
      if (tab.hidden) return;
      if (tab.classList.contains('admin-only') && !isAdminUser()) return;
      activateMainTab(key, tabs, panels);
    });
  });
}

function activateMainTab(key, tabs = document.querySelectorAll('.main-tab'), panels = {
  unsent: $('tabUnsent'),
  single: $('tabSingle'),
  installment: $('tabInstallment'),
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

const installmentLookupLabels = {
  NoDocument: 'شماره سند',
  TrackingNo: 'کد پیگیری'
};

let installmentMode = 'single';
let installmentExcelRows = [];

function detectInstallmentKindFromIdentifier(identifier) {
  const digits = String(identifier || '').replace(/\D/g, '');
  return digits.length >= 10 ? 'TrackingNo' : 'NoDocument';
}

function setInstallmentMode(mode) {
  installmentMode = mode === 'excel' ? 'excel' : 'single';
  document.querySelectorAll('.installment-mode-tab').forEach((btn) => {
    const active = btn.dataset.installmentMode === installmentMode;
    btn.classList.toggle('active', active);
    btn.setAttribute('aria-selected', active ? 'true' : 'false');
  });
  const singlePanel = $('installmentSinglePanel');
  const excelPanel = $('installmentExcelPanel');
  if (singlePanel) singlePanel.hidden = installmentMode !== 'single';
  if (excelPanel) excelPanel.hidden = installmentMode !== 'excel';

  const headerSingle = $('installmentPreviewHeaderSingle');
  const headerExcel = $('installmentPreviewHeaderExcel');
  if (headerSingle) headerSingle.hidden = installmentMode === 'excel';
  if (headerExcel) headerExcel.hidden = installmentMode !== 'excel';

  $('installmentPreviewSection').hidden = true;
  if ($('btnInstallmentUpdate')) $('btnInstallmentUpdate').disabled = true;
}

function normalizeExcelHeader(value) {
  return String(value || '').trim().toLowerCase().replace(/\s+/g, '');
}

function mapExcelHeaderIndex(headers) {
  const map = {};
  headers.forEach((h, idx) => {
    const key = normalizeExcelHeader(h);
    if (key === 'identifier' || key === 'شناسه' || key === 'nodocument' || key === 'trackingno'
      || key === 'شمارهسند' || key === 'کدپیگیری') {
      map.identifier = idx;
    }
    if (key === 'paymentcost' || key === 'مبلغ') map.paymentCost = idx;
    if (key === 'paymentdate' || key === 'تاریخ' || key === 'تاریخپرداخت') map.paymentDate = idx;
    if (key === 'odooat' || key === 'عودت' || key === 'applyodooat' || key === 'applyendstate') map.odooat = idx;
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
  sheet[ref] = { t: 's', v: String(value) };
}

function downloadInstallmentExcelTemplate() {
  if (typeof XLSX === 'undefined') {
    alert('کتابخانه اکسل بارگذاری نشد');
    return;
  }
  const rows = [
    ['Identifier', 'PaymentCost', 'PaymentDate', 'Odooat'],
    ['75360', '661900000', '1405/05/05'],
    ['0502090614002610', '813516015', '1405/05/05', '1'],
    ['0502140614004800', '3157507100', '1405/05/05', '0']
  ];
  const sheet = XLSX.utils.aoa_to_sheet(rows);
  rows.forEach((row, r) => {
    row.forEach((val, c) => {
      if (val != null && val !== '') setSheetCellAsText(sheet, r, c, val);
    });
  });
  sheet['!cols'] = [{ wch: 22 }, { wch: 16 }, { wch: 14 }, { wch: 8 }];
  const book = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(book, sheet, 'Installment');
  XLSX.writeFile(book, 'installment-check-template.xlsx');
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
          reject(new Error('ستون‌های الزامی: Identifier، PaymentCost، PaymentDate'));
          return;
        }

        const parsed = [];
        for (let r = range.s.r + 1; r <= range.e.r; r++) {
          const identifier = parseExcelCellValue(sheet, r, col.identifier);
          const paymentCost = parseExcelCellValue(sheet, r, col.paymentCost);
          const paymentDate = parseExcelCellValue(sheet, r, col.paymentDate);

          if (hasScientificNotation(paymentCost)) {
            reject(new Error(
              `ردیف ${r + 1}: مبلغ «${paymentCost}» به‌صورت علمی است. ستون PaymentCost را Text کنید.`
            ));
            return;
          }

          const identifierDamaged = hasScientificNotation(identifier);
          if (identifierDamaged) {
            const hasCostDate = paymentCost.trim() && paymentDate.trim();
            if (!hasCostDate) {
              reject(new Error(
                `ردیف ${r + 1}: شناسه «${identifier}» خراب است — مبلغ و تاریخ را هم وارد کنید تا جستجو با آن‌ها انجام شود، یا کد پیگیری را دوباره به‌صورت Text تایپ کنید.`
              ));
              return;
            }
          }

          const item = {
            identifier: identifier.trim(),
            paymentCost: paymentCost.trim(),
            paymentDate: paymentDate.trim(),
            odooat: '',
            identifierDamaged
          };
          if (detectInstallmentKindFromIdentifier(item.identifier) === 'TrackingNo'
            && col.odooat != null) {
            item.odooat = parseExcelCellValue(sheet, r, col.odooat).trim();
          }
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
    applyEndState: !!$('installmentApplyEndState')?.checked
  };
  if (installmentMode === 'excel') {
    return {
      ...base,
      applyEndState: false,
      excelRows: installmentExcelRows.map((r) => ({
        identifier: r.identifier || '',
        paymentCost: r.paymentCost || '',
        paymentDate: r.paymentDate || '',
        odooat: r.odooat || ''
      }))
    };
  }
  const raw = ($('installmentValues')?.value || '').replace(/\D/g, '');
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

function formatOdooatPlan(row, kind) {
  if (kind === 'NoDocument') return 'همیشه';
  return row.willApplyEndState ? 'بله' : 'خیر';
}

function renderInstallmentPreview(data) {
  const section = $('installmentPreviewSection');
  const tbody = $('installmentPreviewTable')?.querySelector('tbody');
  const summary = $('installmentPreviewSummary');
  const updateBtn = $('btnInstallmentUpdate');
  const headerSingle = $('installmentPreviewHeaderSingle');
  const headerExcel = $('installmentPreviewHeaderExcel');
  if (!section || !tbody) return;

  const excelMode = !!data.excelMode || installmentMode === 'excel';
  if (headerSingle) headerSingle.hidden = excelMode;
  if (headerExcel) headerExcel.hidden = !excelMode;

  const items = data.items || [];
  section.hidden = false;
  if (summary) {
    if (excelMode) {
      summary.textContent = `ردیف یافت شد: ${data.foundCount || 0} | بدون نتیجه: ${data.notFoundCount || 0} | تطابق کامل: ${data.matchedCount || 0} | عدم تطابق: ${data.mismatchCount || 0}`;
    } else {
      summary.textContent = `ردیف یافت شد: ${data.foundCount || 0} | شناسه بدون نتیجه: ${data.notFoundCount || 0}`;
    }
  }
  tbody.innerHTML = '';
  items.forEach((row) => {
    const tr = document.createElement('tr');
    if (!row.found) tr.classList.add('row-not-found');
    else if (excelMode && row.found && row.dataMatches === false) tr.classList.add('row-mismatch');
    const kind = row.detectedLookupKind || '';
    const endCurrent = `${row.endStateDesc || '-'} / ${row.endStateCode || '-'}`;

    if (excelMode) {
      const status = !row.found
        ? (row.validationMessage || 'یافت نشد')
        : (row.dataMatches
          ? (row.matchedByCostDate ? 'تطابق با مبلغ/تاریخ (شناسه اکسل خراب)' : 'تطابق کامل')
          : (row.validationMessage || 'عدم تطابق'));
      tr.innerHTML = `
        <td class="col-installment-row">${toPersianDigits(row.rowIndex || '-')}</td>
        <td class="col-installment-detect">${installmentLookupLabels[kind] || kind || '—'}</td>
        <td class="col-installment-nodoc">${toPersianDigits(row.noDocument || '-')}</td>
        <td class="col-installment-tracking">${toPersianDigits(row.trackingNo || '-')}</td>
        <td class="col-installment-cost">${formatInstallmentCost(row.paymentCost)}</td>
        <td class="col-installment-date">${formatInstallmentDate(row.paymentDate)}</td>
        <td class="col-installment-workitem">${toPersianDigits(row.nidWorkItem || '-')}</td>
        <td class="col-installment-nosazi">${formatNosaziCode(row.nosaziCode)}</td>
        <td class="col-installment-odooat">${formatOdooatPlan(row, kind)}</td>
        <td class="col-installment-status">${status}</td>
        <td class="col-installment-comments">${row.proposedComments || '-'}</td>
      `;
    } else {
      tr.innerHTML = `
        <td class="col-installment-id">${toPersianDigits(row.lookupValue || '')}</td>
        <td class="col-installment-detect">${installmentLookupLabels[kind] || kind || '—'}</td>
        <td class="col-installment-nodoc">${toPersianDigits(row.noDocument || '-')}</td>
        <td class="col-installment-tracking">${toPersianDigits(row.trackingNo || '-')}</td>
        <td class="col-installment-status">${row.ci_InstallmentStatus ?? row.cI_InstallmentStatus ?? '-'}</td>
        <td class="col-installment-endstate">${endCurrent}</td>
        <td class="col-installment-comments">${row.proposedComments || '-'}</td>
      `;
    }
    tbody.appendChild(tr);
  });
  if (updateBtn) {
    const canUpdate = excelMode
      ? (data.matchedCount > 0)
      : (data.foundCount > 0);
    updateBtn.disabled = !canUpdate;
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
    `ApplyEndState (TrackingNo): ${data.applyEndState}`,
    data.dryRun
      ? `شبیه‌سازی — ${data.wouldUpdate || 0} ردیف UPDATE می‌شد | بدون نتیجه: ${data.notFound}${data.skippedMismatch ? ` | عدم تطابق: ${data.skippedMismatch}` : ''}`
      : `به‌روز: ${data.updated} | بدون نتیجه: ${data.notFound} | خطا: ${data.failed}${data.skippedMismatch ? ` | عدم تطابق: ${data.skippedMismatch}` : ''}`,
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
    alert(validationError);
    return false;
  }

  const pageSize = parseInt($('unsentPageSize')?.value || unsentSearchState.pageSize, 10) || 50;
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
    alert(e.message);
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

function isCenterUser() {
  return !isAdminUser() && getUserDistrict() === '102';
}

function applyAuthUi() {
  const admin = isAdminUser();
  document.querySelectorAll('.admin-only').forEach((el) => {
    el.hidden = !admin;
  });

  const heroUser = $('heroUser');
  if (heroUser && currentUser) {
    heroUser.hidden = false;
    $('userDisplayName').textContent = currentUser.displayName || currentUser.username;
    const districtLabel = getUserDistrict() ? districtLabelFromValue(getUserDistrict()) : '';
    $('userRoleBadge').textContent = admin
      ? 'ادمین'
      : isCenterUser()
        ? 'کاربر — شعبه مرکز'
        : (districtLabel ? `کاربر — ${districtLabel}` : 'کاربر');
    $('userRoleBadge').className = `user-badge ${admin ? 'badge-admin' : 'badge-user'}`;
  }

  activateMainTab(admin ? 'unsent' : 'single');
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
  if (!isAdminUser()) return;
  const res = await apiFetch('/api/admin/users');
  const data = await parseJsonResponse(res);
  const tbody = $('usersTable')?.querySelector('tbody');
  if (!tbody) return;
  tbody.innerHTML = '';
  (data.items || []).forEach((u) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${u.nationalId || u.username}</td>
      <td>${u.firstName || '—'}</td>
      <td>${u.lastName || '—'}</td>
      <td>${u.position || '—'}</td>
      <td>${districtLabelFromValue(u.district)}</td>
      <td>${u.isAdmin ? 'ادمین' : 'کاربر'}</td>
      <td>${u.isActive ? 'فعال' : 'غیرفعال'}</td>
    `;
    tbody.appendChild(tr);
  });
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

  document.querySelectorAll('.installment-mode-tab').forEach((btn) => {
    btn.addEventListener('click', () => setInstallmentMode(btn.dataset.installmentMode));
  });
  setInstallmentMode('single');

  bindClick('btnInstallmentDownloadTemplate', downloadInstallmentExcelTemplate);

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
        alert(e.message);
      }
    });
  }
  syncFundFromBranch();
  setupMainTabs();
  initDatePickers();
  window.addEventListener('load', initDatePickers);
  if (isAdminUser()) await loadUsersTable();
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
      if (data.dryRun) alert('DryRun تهاتر: SOAP ساخته شد؛ POST واقعی زده نشد.');
      else if (data.skipped) alert(data.message);
      else if (data.success) alert(data.message || 'ارسال تهاتر موفق');
      else alert(data.message || (data.docNotSentError ? `عدم ارسال: ${data.docNotSentError}` : 'تهاتر ناموفق'));
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
    if (!selected.length) return alert('حداقل یک فیش انتخاب کنید');

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

      if (data.dryRun) alert('DryRun: SOAP ساخته شد؛ POST واقعی زده نشد.');
      else alert(`ارسال دسته‌ای تمام شد — موفق: ${data.succeeded}، ناموفق: ${data.failed}، رد: ${data.skipped}`);

      selectedUnsentFicheNos.clear();
      if (unsentSearchState.filters) {
        await fetchUnsentResults(unsentSearchState.page);
      }
    } catch (e) {
      box.textContent = e.message;
      alert(e.message);
    } finally {
      updateUnsentSendButton();
    }
  });

  bindClick('btnInstallmentPreview', async () => {
    const payload = getInstallmentPayload();
    if (installmentMode === 'excel') {
      if (!payload.excelRows?.length) return alert('فایل اکسل را انتخاب کنید');
    } else if (!payload.valuesText) {
      return alert('حداقل یک شماره سند یا کد پیگیری وارد کنید');
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
      alert(e.message);
      $('installmentPreviewSection').hidden = true;
      if ($('btnInstallmentUpdate')) $('btnInstallmentUpdate').disabled = true;
    } finally {
      btn.disabled = false;
    }
  });

  bindClick('btnInstallmentUpdate', async () => {
    const payload = getInstallmentPayload();
    if (installmentMode === 'excel') {
      if (!payload.excelRows?.length) return alert('فایل اکسل را انتخاب کنید');
    } else if (!payload.valuesText) {
      return alert('حداقل یک شماره سند یا کد پیگیری وارد کنید');
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
        alert(`${data.wouldUpdate || 0} ردیف — تغییری روی سرور اعمال نشد.`);
      } else {
        alert(`UPDATE تمام شد — ${data.updated} ردیف به‌روز، ${data.notFound} بدون نتیجه`);
      }
      $('btnInstallmentPreview').click();
    } catch (e) {
      if (box) box.textContent = e.message;
      alert(e.message);
    } finally {
      btn.disabled = false;
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
  bindClick('btnRefreshUsers', loadUsersTable);
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
