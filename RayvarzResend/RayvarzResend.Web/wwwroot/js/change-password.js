const PASSWORD_POLICY_HINT =
  'حداقل ۸ کاراکتر، حرف انگلیسی کوچک و بزرگ، و علامت (مثل @)';

function validatePasswordPolicy(password) {
  const value = String(password ?? '');
  if (!value.trim()) return 'رمز عبور الزامی است';
  if (value.length < 8) return 'رمز عبور حداقل ۸ کاراکتر باشد';
  if (!/[a-z]/.test(value)) return 'رمز عبور باید حداقل یک حرف انگلیسی کوچک داشته باشد';
  if (!/[A-Z]/.test(value)) return 'رمز عبور باید حداقل یک حرف انگلیسی بزرگ داشته باشد';
  if (!/[!@#$%^&*()_+\-=[\]{}|;:'",.<>?/\\`~]/.test(value)) {
    return 'رمز عبور باید حداقل یک علامت (مثل @) داشته باشد';
  }
  return null;
}

function setupChangePasswordDialog() {
  const dialog = document.getElementById('changePasswordDialog');
  const btnOpen = document.getElementById('btnChangePassword');
  const btnClose = document.getElementById('btnChangePasswordClose');
  const btnSubmit = document.getElementById('btnChangePasswordSubmit');
  const errorEl = document.getElementById('changePasswordError');
  if (!dialog || !btnOpen || !btnSubmit) return;

  const resetForm = () => {
    ['changePasswordCurrent', 'changePasswordNew', 'changePasswordConfirm'].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.value = '';
    });
    if (errorEl) {
      errorEl.hidden = true;
      errorEl.textContent = '';
    }
  };

  const closeDialog = () => {
    if (dialog.open) dialog.close();
    resetForm();
  };

  btnOpen.addEventListener('click', () => {
    resetForm();
    if (typeof dialog.showModal === 'function') dialog.showModal();
  });
  btnClose?.addEventListener('click', closeDialog);
  dialog.addEventListener('cancel', (e) => {
    e.preventDefault();
    closeDialog();
  });

  btnSubmit.addEventListener('click', async () => {
    const current = document.getElementById('changePasswordCurrent')?.value || '';
    const next = document.getElementById('changePasswordNew')?.value || '';
    const confirm = document.getElementById('changePasswordConfirm')?.value || '';
    if (!current) {
      if (errorEl) {
        errorEl.hidden = false;
        errorEl.textContent = 'رمز عبور فعلی را وارد کنید';
      }
      return;
    }
    const policyError = validatePasswordPolicy(next);
    if (policyError) {
      if (errorEl) {
        errorEl.hidden = false;
        errorEl.textContent = policyError;
      }
      return;
    }
    if (next !== confirm) {
      if (errorEl) {
        errorEl.hidden = false;
        errorEl.textContent = 'رمز جدید و تکرار آن یکسان نیست';
      }
      return;
    }
    btnSubmit.disabled = true;
    if (errorEl) errorEl.hidden = true;
    try {
      const res = await fetch('/api/auth/change-password', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ currentPassword: current, newPassword: next })
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || `خطا (HTTP ${res.status})`);
      closeDialog();
      if (typeof window.showAppSuccess === 'function') {
        window.showAppSuccess(data.message || 'رمز عبور با موفقیت تغییر کرد');
      } else {
        window.alert(data.message || 'رمز عبور با موفقیت تغییر کرد');
      }
    } catch (err) {
      if (errorEl) {
        errorEl.hidden = false;
        errorEl.textContent = err.message || 'خطا در تغییر رمز';
      }
    } finally {
      btnSubmit.disabled = false;
    }
  });
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', setupChangePasswordDialog);
} else {
  setupChangePasswordDialog();
}
