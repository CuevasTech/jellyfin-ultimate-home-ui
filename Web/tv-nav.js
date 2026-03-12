/**
 * UHUI — TV / D-pad / Keyboard Navigation
 * 2D spatial focus manager using bounding rects.
 * Supports arrow keys, Enter/OK, Back, and Gamepad API.
 */

const TV_KEY = {
  UP:    38,
  DOWN:  40,
  LEFT:  37,
  RIGHT: 39,
  SELECT: 13,
  BACK:  8,
  ESCAPE: 27,
};

let keyHandler = null;
let gamepadPollId = null;

/**
 * Find the next focusable element in a given direction using bounding rects.
 */
function findNext(focusable, current, dir) {
  if (!current) return focusable[0] || null;
  const cRect = current.getBoundingClientRect();
  const cx = cRect.left + cRect.width / 2;
  const cy = cRect.top + cRect.height / 2;

  const candidates = focusable.filter(function(el) {
    if (el === current) return false;
    const r = el.getBoundingClientRect();
    const ex = r.left + r.width / 2;
    const ey = r.top + r.height / 2;
    switch (dir) {
      case 'right': return ex > cx;
      case 'left':  return ex < cx;
      case 'down':  return ey > cy;
      case 'up':    return ey < cy;
      default:      return false;
    }
  });

  if (!candidates.length) return null;

  function score(el) {
    const r = el.getBoundingClientRect();
    const ex = r.left + r.width / 2;
    const ey = r.top + r.height / 2;
    const primary   = (dir === 'left' || dir === 'right') ? Math.abs(ex - cx) : Math.abs(ey - cy);
    const secondary = (dir === 'left' || dir === 'right') ? Math.abs(ey - cy) : Math.abs(ex - cx);
    return primary + secondary * 2;
  }

  candidates.sort(function(a, b) { return score(a) - score(b); });
  return candidates[0];
}

function handleKeydown(e) {
  const dir = {
    [TV_KEY.UP]:    'up',
    [TV_KEY.DOWN]:  'down',
    [TV_KEY.LEFT]:  'left',
    [TV_KEY.RIGHT]: 'right',
  }[e.keyCode];

  if (dir) {
    e.preventDefault();
    const focusable = Array.from(document.querySelectorAll('[data-focusable]'));
    const current = document.activeElement;
    const next = findNext(focusable, current, dir);
    if (next) {
      next.focus({ preventScroll: true });
      next.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: 'smooth' });
    }
    return;
  }

  if (e.keyCode === TV_KEY.SELECT) {
    const active = document.activeElement;
    if (active && active.hasAttribute('data-focusable')) {
      active.click();
    }
    return;
  }

  if (e.keyCode === TV_KEY.BACK || e.keyCode === TV_KEY.ESCAPE) {
    window.history.back();
  }
}

/* --- Gamepad API polling --- */

const GAMEPAD_DEADZONE = 0.5;
let prevAxes = [0, 0, 0, 0];
let prevButtons = [];

function pollGamepad() {
  const gamepads = navigator.getGamepads ? navigator.getGamepads() : [];
  const gp = gamepads[0];
  if (!gp) return;

  const ax0 = gp.axes[0] || 0;
  const ax1 = gp.axes[1] || 0;

  let dir = null;
  if (ax0 < -GAMEPAD_DEADZONE && prevAxes[0] >= -GAMEPAD_DEADZONE) dir = 'left';
  if (ax0 > GAMEPAD_DEADZONE && prevAxes[0] <= GAMEPAD_DEADZONE) dir = 'right';
  if (ax1 < -GAMEPAD_DEADZONE && prevAxes[1] >= -GAMEPAD_DEADZONE) dir = 'up';
  if (ax1 > GAMEPAD_DEADZONE && prevAxes[1] <= GAMEPAD_DEADZONE) dir = 'down';

  if (!dir && gp.buttons) {
    if (gp.buttons[12] && gp.buttons[12].pressed && !(prevButtons[12] && prevButtons[12].pressed)) dir = 'up';
    if (gp.buttons[13] && gp.buttons[13].pressed && !(prevButtons[13] && prevButtons[13].pressed)) dir = 'down';
    if (gp.buttons[14] && gp.buttons[14].pressed && !(prevButtons[14] && prevButtons[14].pressed)) dir = 'left';
    if (gp.buttons[15] && gp.buttons[15].pressed && !(prevButtons[15] && prevButtons[15].pressed)) dir = 'right';
  }

  if (dir) {
    const focusable = Array.from(document.querySelectorAll('[data-focusable]'));
    const current = document.activeElement;
    const next = findNext(focusable, current, dir);
    if (next) {
      next.focus({ preventScroll: true });
      next.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: 'smooth' });
    }
  }

  if (gp.buttons && gp.buttons[0] && gp.buttons[0].pressed && !(prevButtons[0] && prevButtons[0].pressed)) {
    var active = document.activeElement;
    if (active && active.hasAttribute('data-focusable')) {
      active.click();
    }
  }

  if (gp.buttons && gp.buttons[1] && gp.buttons[1].pressed && !(prevButtons[1] && prevButtons[1].pressed)) {
    window.history.back();
  }

  prevAxes = [ax0, ax1, gp.axes[2] || 0, gp.axes[3] || 0];
  prevButtons = gp.buttons ? Array.from(gp.buttons).map(function(b) { return { pressed: b.pressed }; }) : [];
}

export function initTVNav() {
  if (keyHandler) return;
  keyHandler = handleKeydown;
  document.addEventListener('keydown', keyHandler);

  if (!gamepadPollId) {
    gamepadPollId = setInterval(pollGamepad, 100);
  }
}

export function destroyTVNav() {
  if (keyHandler) {
    document.removeEventListener('keydown', keyHandler);
    keyHandler = null;
  }
  if (gamepadPollId) {
    clearInterval(gamepadPollId);
    gamepadPollId = null;
  }
}
