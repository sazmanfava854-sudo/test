const $ = (id) => document.getElementById(id);

async function parseJsonResponse(res) {
  const text = await res.text();
  if (!text) return {};
  try {
    return JSON.parse(text);
  } catch {
    throw new Error(text || `خطا (HTTP ${res.status})`);
  }
}

function resolvePostLoginTarget(mode) {
  const params = new URLSearchParams(window.location.search);
  const returnUrl = (params.get('returnUrl') || '').trim();
  if (returnUrl.startsWith('/') && !returnUrl.startsWith('//')) return returnUrl;
  const fallback = (mode?.postLoginDefaultPath || '/management/').trim();
  return fallback.startsWith('/') ? fallback : '/management/';
}

function canShowLocalLoginForm(mode) {
  if (!mode) return true;
  return !!(
    mode.localLoginAvailable
    || mode.allowHybridLocalLogin
    || mode.allowAdminLocalLoginOnPublicHost
  );
}

function shouldForceSsoRedirect(mode) {
  if (mode?.allowHybridLocalLogin) return false;
  if (!mode?.preferSsoLogin) return false;
  return !canShowLocalLoginForm(mode);
}

async function checkExistingSession(mode) {
  try {
    const res = await fetch('/api/auth/me', { credentials: 'include' });
    if (res.ok) {
      window.location.href = resolvePostLoginTarget(mode);
      return true;
    }
  } catch {
    // ignore
  }
  return false;
}

function showSsoBlock(mode) {
  const block = $('loginSsoBlock');
  if (!block) return;
  const show = !!mode?.preferSsoLogin;
  block.hidden = !show;
  const link = $('btnSsoLogin');
  if (link && mode?.loginPath) link.setAttribute('href', mode.loginPath);
}

async function initLoginPage() {
  let mode = null;
  try {
    const res = await fetch('/api/auth/mode');
    if (res.ok) mode = await res.json();
  } catch {
    // ignore
  }

  if (await checkExistingSession(mode)) return;

  try {
    if (mode) {
      showSsoBlock(mode);
      if (shouldForceSsoRedirect(mode)) {
        window.location.href = mode.loginPath || '/auth/login';
        return;
      }
      if (!canShowLocalLoginForm(mode)) {
        const errEl = $('loginError');
        errEl.textContent = 'ورود محلی غیرفعال است — از ورود سازمانی استفاده کنید';
        errEl.hidden = false;
        $('loginForm').hidden = true;
      }
    }
  } catch {
    // ignore
  }

  const params = new URLSearchParams(window.location.search);
  const error = params.get('error');
  if (error) {
    const errEl = $('loginError');
    errEl.textContent = decodeURIComponent(error);
    errEl.hidden = false;
  }
}

$('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const errEl = $('loginError');
  const btn = $('btnLogin');
  errEl.hidden = true;
  errEl.textContent = '';
  btn.disabled = true;

  try {
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({
        username: $('loginUsername').value.trim(),
        password: $('loginPassword').value
      })
    });
    const data = await parseJsonResponse(res);
    if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
    let mode = null;
    try {
      const modeRes = await fetch('/api/auth/mode');
      if (modeRes.ok) mode = await modeRes.json();
    } catch {
      // ignore
    }
    window.location.href = resolvePostLoginTarget(mode);
  } catch (ex) {
    errEl.textContent = ex.message || 'ورود ناموفق';
    errEl.hidden = false;
  } finally {
    btn.disabled = false;
  }
});

initLoginPage();
