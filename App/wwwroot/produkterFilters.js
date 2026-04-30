window.klosterFilters = {
  layoutPills() {
    document.querySelectorAll('.seg-sliding').forEach((seg) => {
      const active = seg.getAttribute('data-active');
      if (!active) return;
      const btn = seg.querySelector(`[data-value="${CSS.escape(active)}"]`);
      const pill = seg.querySelector('.seg-pill');
      if (!btn || !pill) return;
      const segRect = seg.getBoundingClientRect();
      const btnRect = btn.getBoundingClientRect();
      pill.style.left = (btnRect.left - segRect.left) + 'px';
      pill.style.width = btnRect.width + 'px';
      pill.style.opacity = '1';
    });
  }
};

window.addEventListener('resize', () => window.klosterFilters.layoutPills());
