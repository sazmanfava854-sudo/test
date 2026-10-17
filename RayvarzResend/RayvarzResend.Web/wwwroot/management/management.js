const MODULES = [
  {
    key: 'unsent',
    title: 'ارسال به رایورز',
    description: '',
    icon: '📤',
    href: '/?tab=unsent',
    can: (user) => canAccessRayvarzModule(user)
  },
  {
    key: 'installment',
    title: 'چک خزانه',
    description: '',
    icon: '🏦',
    href: '/?tab=installment',
    can: (user) => isAdminUser(user) || !!user?.canAccessInstallment
  },
  {
    key: 'ficheDate',
    title: 'تغییر تاریخ فیش',
    description: '',
    icon: '📅',
    href: '/?tab=ficheDate',
    can: (user) => isAdminUser(user) || !!user?.canAccessFicheDateChange
  },
  {
    key: 'bankInquiry',
    title: 'خدمات الکترونیک',
    description: '',
    icon: '💳',
    href: '/?tab=bankInquiry',
    can: (user) => isAdminUser(user) || !!user?.canAccessBankInquiryConfirm
  },
  {
    key: 'shahkar',
    title: 'شاهکار',
    description: '',
    icon: '✓',
    href: '/?tab=shahkar',
    can: (user) => isAdminUser(user) || !!user?.canAccessShahkar
  },
  {
    key: 'users',
    title: 'مدیریت کاربران',
    description: '',
    icon: '👥',
    href: '/?tab=users',
    can: (user) => isAdminUser(user) || !!user?.canManageUsers
  }
];

let currentUser = null;
let authMode = null;

function readIsAdminFlag(user) {
  if (!user) return false;
  const raw = user.isAdmin ?? user.IsAdmin;
  return raw === true || raw === 1 || raw === '1' || raw === 'true';
}

function normalizeAuthSession(user) {
  if (!user) return user;
  if (readIsAdminFlag(user)) {
    user.isAdmin = true;
    user.canAccessUnsentFiches = true;
    user.canAccessInstallment = true;
    user.canAccessFicheDateChange = true;
    user.canAccessBankInquiryConfirm = true;
    user.canAccessShahkar = true;
    user.canManageUsers = true;
  }
  return user;
}

function isAdminUser(user) {
  return readIsAdminFlag(user ?? currentUser);
}

function hasAnyModulePermission(user) {
  if (isAdminUser(user)) return true;
  return !!(
    user?.canAccessUnsentFiches
    || user?.canAccessInstallment
    || user?.canAccessFicheDateChange
    || user?.canAccessBankInquiryConfirm
    || user?.canAccessShahkar
    || user?.canManageUsers
  );
}

function canAccessUnsent(user) {
  return isAdminUser(user) || !!user?.canAccessUnsentFiches;
}

function canAccessRayvarzModule(user) {
  if (isAdminUser(user)) return true;
  if (canAccessUnsent(user)) return true;
  return !hasAnyModulePermission(user);
}

async function loadAuthMode() {
  if (authMode) return authMode;
  try {
    const res = await fetch('/api/auth/mode', { credentials: 'include' });
    if (res.ok) authMode = await res.json();
  } catch {
    // ignore
  }
  return authMode;
}

async function redirectToLogin() {
  const mode = await loadAuthMode();
  const base = mode?.allowHybridLocalLogin || !mode?.preferSsoLogin
    ? '/login.html'
    : (mode.loginPath || '/auth/login');
  const returnUrl = encodeURIComponent('/management/');
  window.location.href = base.includes('?')
    ? `${base}&returnUrl=${returnUrl}`
    : `${base}?returnUrl=${returnUrl}`;
}

function getUserDistrict(user) {
  return user?.districtCode ?? user?.district ?? '';
}

function isCenterUser(user) {
  return !isAdminUser(user) && String(getUserDistrict(user)) === '102';
}

function applyUserHeader(user) {
  const heroUser = document.getElementById('heroUser');
  if (!heroUser || !user) return;
  heroUser.hidden = false;
  document.getElementById('userDisplayName').textContent = user.displayName || user.username || '—';
  const badge = document.getElementById('userRoleBadge');
  badge.textContent = isAdminUser(user)
    ? 'ادمین'
    : isCenterUser(user)
      ? 'کاربر — شعبه مرکز'
      : 'کاربر';
  badge.className = `user-badge ${isAdminUser(user) ? 'badge-admin' : 'badge-user'}`;
}

function renderModuleCards(user) {
  const grid = document.getElementById('hubGrid');
  const empty = document.getElementById('hubEmpty');
  const visible = MODULES.filter((m) => m.can(user));
  grid.innerHTML = '';

  if (visible.length === 0) {
    grid.hidden = true;
    empty.hidden = false;
    return;
  }

  empty.hidden = true;
  grid.hidden = false;

  visible.forEach((mod) => {
    const link = document.createElement('a');
    link.className = 'hub-card';
    link.href = mod.href;
    link.setAttribute('role', 'listitem');
    const desc = (mod.description || '').trim();
    link.innerHTML = `
      <span class="hub-card-icon" aria-hidden="true">${mod.icon}</span>
      <h2 class="hub-card-title">${mod.title}</h2>
      ${desc ? `<p class="hub-card-desc">${desc}</p>` : ''}
      <span class="hub-card-cta">ورود به فرم →</span>
    `;
    grid.appendChild(link);
  });
}

function shortHash(hash) {
  const h = String(hash || '').trim();
  return h ? h.slice(0, 12) : '—';
}

function formatUtc(value) {
  if (!value) return '—';
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? String(value) : d.toLocaleString('fa-IR');
}

function renderRuleGate(data) {
  const panel = document.getElementById('ruleGatePanel');
  const status = document.getElementById('ruleGateStatus');
  const details = document.getElementById('ruleGateDetails');
  const approve = document.getElementById('btnRuleGateApprove');

  if (!data?.enabled || !data?.configured || !data?.sendGateActive) {
    panel.hidden = true;
    return;
  }

  const blocked = !!data.sendBlocked;
  panel.hidden = false;
  panel.classList.toggle('is-blocked', blocked);
  panel.classList.toggle('is-open', !blocked);
  status.textContent = blocked
    ? (data.sendBlockedAdminMessage || 'نیاز به به روز رسانی هست')
    : 'ارسال به رایورز باز است.';
  approve.hidden = !blocked;

  const state = data.state || {};
  const open = data.openCandidate;
  const rows = [
    ['NidMember', data.nidMember],
    ['هش تأییدشده برای ارسال', shortHash(state.approvedSendXmlHash)],
    ['هش پایدار (Stable)', shortHash(state.lastStableXmlHash)],
    ['آخرین تغییر Gate', `${formatUtc(state.sendGateChangedAtUtc)} — ${state.sendGateChangedBy || '—'}`]
  ];
  if (open) {
    rows.push(['Candidate در انتظار', `#${open.candidateId} ${open.status}`]);
    rows.push(['هش Candidate', shortHash(open.canonicalXmlHash)]);
    rows.push(['تغییردهنده', `${open.modifyer || '—'} ${open.modifyDesc ? `(${open.modifyDesc})` : ''}`]);
    rows.push(['پایان دوره ثبات ۷۲ساعته', formatUtc(open.stableEligibleAtUtc)]);
  }
  details.innerHTML = '';
  rows.forEach(([label, value]) => {
    const dt = document.createElement('dt');
    dt.textContent = label;
    const dd = document.createElement('dd');
    dd.textContent = String(value ?? '—');
    details.append(dt, dd);
  });
}

async function loadRuleGate() {
  try {
    const res = await fetch('/api/rule/sync/status', { credentials: 'include' });
    if (!res.ok) return;
    renderRuleGate(await res.json());
  } catch {
    // ignore
  }
}

async function approveRuleGate() {
  const note = window.prompt('تغییرات قانون در کد اعمال و deploy شده است؟ توضیح (اختیاری):', '');
  if (note === null) return;
  const btn = document.getElementById('btnRuleGateApprove');
  btn.disabled = true;
  try {
    const res = await fetch('/api/rule/send-gate/approve', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ note })
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) window.alert(data.error || `خطا (HTTP ${res.status})`);
  } finally {
    btn.disabled = false;
    await loadRuleGate();
  }
}

document.getElementById('btnRuleGateApprove')?.addEventListener('click', approveRuleGate);
document.getElementById('btnRuleGateRefresh')?.addEventListener('click', loadRuleGate);

async function logout() {
  try {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' });
  } catch {
    // ignore
  }
  // نه /auth/login — نشست SSO فعال کاربر را بلافاصله دوباره وارد می‌کند
  window.location.href = '/login.html';
}

async function ensureAuthenticated() {
  const res = await fetch('/api/auth/me', { credentials: 'include' });
  if (!res.ok) {
    await redirectToLogin();
    return false;
  }
  currentUser = normalizeAuthSession(await res.json());
  document.body.classList.add('hub-authenticated');
  applyUserHeader(currentUser);
  renderModuleCards(currentUser);
  if (isAdminUser(currentUser)) loadRuleGate();
  return true;
}

document.getElementById('btnLogout')?.addEventListener('click', () => {
  logout();
});

ensureAuthenticated();
