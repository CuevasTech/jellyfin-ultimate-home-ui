/**
 * UHUI — Touch / Swipe Handler
 * Pointer Events API with Touch Events fallback for older Android WebView.
 */

/**
 * Attach horizontal swipe detection to an element.
 * @param {HTMLElement} el
 * @param {{ onLeft?: () => void, onRight?: () => void, threshold?: number }} opts
 */
export function addSwipe(el, { onLeft, onRight, threshold = 50 } = {}) {
  let startX = 0;
  let startY = 0;
  let tracking = false;

  function onStart(x, y) {
    startX = x;
    startY = y;
    tracking = true;
  }

  function onEnd(x, y) {
    if (!tracking) return;
    tracking = false;
    const dx = x - startX;
    const dy = y - startY;
    if (Math.abs(dx) < Math.abs(dy) * 1.5) return;
    if (dx < -threshold) onLeft && onLeft();
    else if (dx > threshold) onRight && onRight();
  }

  el.addEventListener('pointerdown', (e) => onStart(e.clientX, e.clientY), { passive: true });
  el.addEventListener('pointerup', (e) => onEnd(e.clientX, e.clientY));

  el.addEventListener('touchstart', (e) => {
    const t = e.touches[0];
    onStart(t.clientX, t.clientY);
  }, { passive: true });
  el.addEventListener('touchend', (e) => {
    const t = e.changedTouches[0];
    onEnd(t.clientX, t.clientY);
  });
}
