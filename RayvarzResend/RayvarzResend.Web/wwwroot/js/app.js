let currentFiche = null;
let config = null;

const $ = (id) => document.getElementById(id);

async function init() {
  config = await fetch('/api/config').then(r => r.json());
  const badge = $('configBadge');
  badge.textContent = config.dryRun
    ? `حالت DryRun فعال — ارسال واقعی نمی‌شود | ${config.serviceUrl}`
    : `ارسال واقعی | ${config.serviceUrl}`;

  const branchSel = $('branch');
  config.branches.forEach(b => {
    const opt = document.createElement('option');
    opt.value = b.id;
    opt.textContent = `${b.id} — ${b.name}`;
    branchSel.appendChild(opt);
  });

  const today = new Date();
  $('docDate').value = `140${today.getFullYear() - 2020}/${String(today.getMonth()+1).padStart(2,'0')}/${String(today.getDate()).padStart(2,'0')}`;
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
    const data = await res.json();
    if (!res.ok) throw new Error(data.error || data.title || 'خطا');

    currentFiche = data;
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

function renderFiche(f) {
  $('ficheSection').hidden = false;
  const cat = { Income: 'درآمد', DutyNosazi: 'نوسازی', DutySenfi: 'صنفی', Unknown: 'نامشخص' };
  const statusClass = f.existsInRayvarz ? 'status-err' : (f.statusMessage === 'آماده ارسال' ? 'status-ok' : 'status-warn');

  $('ficheSummary').innerHTML = `
    <div><b>فیش:</b> ${f.ficheNo}</div>
    <div><b>نوع:</b> ${cat[f.category] || f.category}</div>
    <div><b>Payable:</b> ${Number(f.payable).toLocaleString()}</div>
    <div><b>BnkAcntNo:</b> ${f.bnkAcntNo || '-'}</div>
    <div><b>BillID:</b> ${f.billId}</div>
    <div><b>PaymentID:</b> ${f.paymentId}</div>
    <div><b>DocTyp:</b> ${f.docTyp} — ${f.docDsc}</div>
    <div><b>وضعیت:</b> <span class="${statusClass}">${f.statusMessage}</span></div>
    <div><b>در رایورز:</b> ${f.existsInRayvarz ? 'بله' : 'خیر'}</div>
  `;

  const tbody = $('rowsTable').querySelector('tbody');
  tbody.innerHTML = '';
  (f.rows || []).forEach((r, i) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `<td>${i+1}</td><td>${r.incmNo}</td><td>${r.incmRowDsc}</td><td>${Number(r.val).toLocaleString()}</td>`;
    tbody.appendChild(tr);
  });
}

$('btnPreview').onclick = async () => {
  if (!currentFiche) return;
  const res = await fetch('/api/fiche/preview', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      fiche: currentFiche,
      branch: parseInt($('branch').value),
      docDate: $('docDate').value,
      resetStatus: false
    })
  });
  const data = await res.json();
  $('xmlSection').hidden = false;
  $('xmlBox').textContent = data.xml;
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
      body: JSON.stringify({
        fiche: currentFiche,
        branch: parseInt($('branch').value),
        docDate: $('docDate').value,
        resetStatus: true
      })
    });
    const data = await res.json();
    if (!res.ok) throw new Error(data.error || data.title || 'خطا');

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
  } catch (e) {
    alert(e.message);
  } finally {
    $('btnSend').disabled = false;
  }
};

init();
