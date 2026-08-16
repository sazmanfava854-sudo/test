/**
 * Bug 14 — mirror of app.js sumRowVals / formatValMappingDetail (keep in sync).
 */
function sumRowVals(rows) {
  return (rows || []).reduce((a, r) => a + Number(r.val), 0);
}

function formatValMappingDetail(f) {
  const rows = f.rows || [];
  const sum = sumRowVals(rows);
  const payable = Number(f.payable);
  const parts = rows.map((r) => Number(r.val).toLocaleString()).join(' + ');
  const matched = Math.abs(sum - payable) < 0.5;
  const matchNote = matched
    ? '✓ جمع = Payable'
    : `⚠ اختلاف ${Math.abs(sum - payable).toLocaleString()} ریال`;
  const incomeNote = f.category === 'Income'
    ? ' | مقادیر سرور (اسکیل + Oddment)'
    : '';
  return `${parts} = ${sum.toLocaleString()} | Payable: ${payable.toLocaleString()} | ${matchNote}${incomeNote}`;
}

function valMappingSource(f) {
  if (f.category === 'DutyNosazi' || f.category === 'DutySenfi') {
    return 'مقادیر ارسالی — نوسازی: Payable بین ردیف‌ها توزیع شده';
  }
  return 'مقادیر ارسالی از API — Income_Calculation اسکیل‌شده به Payable (+ Oddment)';
}

let failed = 0;

function assertTrue(cond, msg) {
  if (!cond) {
    console.error('FAIL:', msg);
    failed++;
  } else {
    console.log('OK:', msg);
  }
}

const incomeMatched = {
  category: 'Income',
  payable: 5_379_066_000,
  rows: [
    { val: 87_501_332 },
    { val: 3_506_537_488 },
    { val: 1_616_686_008 },
    { val: 35_000_533 },
    { val: 133_340_639 },
  ],
};

const detail = formatValMappingDetail(incomeMatched);
assertTrue(detail.includes('✓ جمع = Payable'), 'matched income shows checkmark');
assertTrue(detail.includes('مقادیر سرور'), 'income note present');
assertTrue(valMappingSource(incomeMatched).includes('اسکیل'), 'income source mentions scale');

const incomeMismatch = { category: 'Income', payable: 1_000, rows: [{ val: 600 }, { val: 300 }] };
assertTrue(formatValMappingDetail(incomeMismatch).includes('⚠ اختلاف'), 'mismatch shows warning');

const duty = { category: 'DutyNosazi', payable: 1_000_000, rows: [{ val: 400_000 }, { val: 600_000 }] };
assertTrue(formatValMappingDetail(duty).includes('✓ جمع = Payable'), 'duty matched sum');
assertTrue(valMappingSource(duty).includes('نوسازی'), 'duty source text');

assertTrue(sumRowVals([]) === 0, 'empty rows sum zero');

if (failed > 0) {
  console.error(`\n${failed} test(s) failed`);
  process.exit(1);
}
console.log(`\nAll Bug14 UI summary tests passed`);
