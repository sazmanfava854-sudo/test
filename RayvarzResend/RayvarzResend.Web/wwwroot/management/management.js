const MODULES = [
  {
    key: 'unsent',
    title: 'ارسال به رایورز',
    description: 'ارسال تکی یا جمعی فیش به رایورز',
    icon: '📤',
    href: '/?tab=unsent',
    can: (user) => canAccessRayvarzModule(user)
  },
  {
    key: 'installment',
    title: 'چک خزانه',
    description: 'بررسی و ارسال چک‌های خزانه',
    icon: '🏦',
    href: '/?tab=installment',
    can: (user) => isAdminUser(user) || !!user?.canAccessInstallment
  },
  {
    key: 'ficheDate',
    title: 'تغییر تاریخ فیش',
    description: 'تغییر تاریخ فیش‌های ثبت‌شده',
    icon: '📅',
    href: '/?tab=ficheDate',
    can: (user) => isAdminUser(user) || !!user?.canAccessFicheDateChange
  },
  {
    key: 'bankInquiry',
    title: 'خدمات الکترونیک',
    description: 'تأیید استعلام بانکی و خدمات الکترونیک',
    icon: '💳',
    href: '/?tab=bankInquiry',
    can: (user) => isAdminUser(user) || !!user?.canAccessBankInquiryConfirm
  },
  {
    key: 'users',
    title: 'مدیریت کاربران',
    description: 'کاربران، گروه‌ها و دسترسی‌ها',
    icon: '👥',
    href: '/?tab=users',
    can: (user) => isAdminUser(user) || !!user?.canManageUsers
  }
];

let currentUser = null;
let authMode = null;

function isAdminUser(user) {
  return !!user?.isAdmin;
}

function hasAnyModulePermission(user) {
  if (isAdminUser(user)) return true;
  return !!(
    user?.canAccessUnsentFiches
    || user?.canAccessInstallment
    || user?.canAccessFicheDateChange
    || user?.canAccessBankInquiryConfirm
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
  const base = mode?.preferSsoLogin ? (mode.loginPath || '/auth/login') : '/login.html';
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
    link.innerHTML = `
      <span class="hub-card-icon" aria-hidden="true">${mod.icon}</span>
      <h2 class="hub-card-title">${mod.title}</h2>
      <p class="hub-card-desc">${mod.description}</p>
      <span class="hub-card-cta">ورود به فرم →</span>
    `;
    grid.appendChild(link);
  });
}

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
  currentUser = await res.json();
  document.body.classList.add('hub-authenticated');
  applyUserHeader(currentUser);
  renderModuleCards(currentUser);
  return true;
}

document.getElementById('btnLogout')?.addEventListener('click', () => {
  logout();
});

ensureAuthenticated();
