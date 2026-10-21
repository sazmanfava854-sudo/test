function setupPasswordVisibilityToggles(root = document) {
  root.querySelectorAll('.password-field-wrap').forEach((wrap) => {
    const input = wrap.querySelector('input');
    const btn = wrap.querySelector('.password-toggle-btn');
    if (!input || !btn || btn.dataset.bound === '1') return;
    btn.dataset.bound = '1';
    const showLabel = btn.getAttribute('data-label-show') || 'نمایش رمز';
    const hideLabel = btn.getAttribute('data-label-hide') || 'پنهان کردن رمز';
    btn.addEventListener('click', () => {
      const revealing = input.type === 'password';
      input.type = revealing ? 'text' : 'password';
      btn.setAttribute('aria-pressed', revealing ? 'true' : 'false');
      btn.setAttribute('aria-label', revealing ? hideLabel : showLabel);
      btn.textContent = revealing ? '🙈' : '👁';
    });
  });
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => setupPasswordVisibilityToggles());
} else {
  setupPasswordVisibilityToggles();
}
