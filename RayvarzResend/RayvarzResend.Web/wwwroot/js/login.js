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

async function checkExistingSession() {
  try {
    const res = await fetch('/api/auth/me', { credentials: 'include' });
    if (res.ok) {
      window.location.href = '/';
      return true;
    }
  } catch {
    // ignore
  }
  return false;
}

async function initLoginPage() {
  if (await checkExistingSession()) return;

  try {
    const res = await fetch('/api/auth/mode');
    if (res.ok) {
      const mode = await res.json();
      if (mode.preferSsoLogin) {
        window.location.href = mode.loginPath || '/auth/login';
        return;
      }
      if (!mode.localLoginAvailable) {
        const errEl = $('loginError');
        errEl.textContent = 'ورود محلی غیرفعال است';
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
    window.location.href = '/';
  } catch (ex) {
    errEl.textContent = ex.message || 'ورود ناموفق';
    errEl.hidden = false;
  } finally {
    btn.disabled = false;
  }
});

initLoginPage();
