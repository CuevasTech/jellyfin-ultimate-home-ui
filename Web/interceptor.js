/**
 * UHUI — DOM Interceptor
 * Uses MutationObserver to detect when Jellyfin renders the Home page,
 * then replaces it with the custom Netflix-style layout.
 *
 * Jellyfin is a SPA — navigations don't trigger full page reloads.
 * We rely on MutationObserver + the custom 'viewshow' / 'viewhide' events
 * that Jellyfin's internal router fires (they bubble up to document).
 */

import { injectLayout, destroyLayout } from './layout.js';

let attached = false;
let injected = false;

function isHomePage() {
  if (document.querySelector('.homePage')) return true;
  const hash = location.hash.replace(/^#!?/, '');
  return hash === '/home.html' || hash === '/home' || hash === '/' || hash === '';
}

function checkHome(mutations, obs) {
  for (const m of mutations) {
    for (const node of m.addedNodes) {
      if (node.nodeType !== Node.ELEMENT_NODE) continue;
      const home = (node.matches && node.matches('.homePage'))
        ? node
        : (node.querySelector && node.querySelector('.homePage'));
      if (home) {
        obs.disconnect();
        attached = false;
        handleHomeFound(home);
        return;
      }
    }
  }
}

function handleHomeFound(homeEl) {
  if (injected) return;
  injected = true;
  requestAnimationFrame(() => {
    injectStyles();
    injectLayout(homeEl);
  });
}

function attachObserver() {
  if (attached) return;
  attached = true;
  const obs = new MutationObserver(checkHome);
  obs.observe(document.body, { childList: true, subtree: true });
}

function injectStyles() {
  if (document.getElementById('uhui-styles')) return;

  const link = document.createElement('link');
  link.id = 'uhui-styles';
  link.rel = 'stylesheet';
  link.href = import.meta.url.replace('interceptor.js', 'styles.css');
  document.head.appendChild(link);
}

export function init() {
  window.__uhui_active = true;

  document.addEventListener('viewshow', () => {
    if (isHomePage()) {
      injected = false;
      attachObserver();

      const home = document.querySelector('.homePage');
      if (home) handleHomeFound(home);
    }
  });

  document.addEventListener('viewhide', () => {
    if (injected) {
      injected = false;
      destroyLayout();
    }
  });

  window.addEventListener('hashchange', () => {
    if (isHomePage()) {
      injected = false;
      attachObserver();
    }
  });

  if (isHomePage()) {
    const home = document.querySelector('.homePage');
    if (home) {
      handleHomeFound(home);
    } else {
      attachObserver();
    }
  } else {
    attachObserver();
  }

  console.log('[UHUI] Interceptor active — watching for Home page');
}
