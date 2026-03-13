/**
 * UHUI — DOM Interceptor
 *
 * Detecta cuando Jellyfin renderiza la pantalla de inicio y la sustituye
 * por el layout Netflix personalizado.
 *
 * Estrategia multi-capa (más robusta que depender solo de un evento):
 *  1. MutationObserver en document.body (detecta adiciones de .homePage)
 *  2. Eventos del router de Jellyfin: viewshow, pageshow, hashchange, popstate
 *  3. Polling activo cada 300 ms durante los primeros 30 s (fallback para todos los casos)
 */

import { injectLayout, destroyLayout } from './layout.js';

let attached = false;
let injected = false;
let pollTimer = null;
let currentObserver = null;

// ── Detección de la página de inicio ──────────────────────────────────────────

function isHomePage() {
  if (document.querySelector('.homePage')) return true;

  // Jellyfin usa hashes del tipo #!/home.html, #!/home, o simplemente /
  const hash = location.hash.replace(/^#!?/, '');
  return (
    hash === '' ||
    hash === '/' ||
    hash === '/home' ||
    hash === '/home.html' ||
    hash.startsWith('/home?') ||
    hash.startsWith('/home.html?')
  );
}

// ── Gestión de la inyección ────────────────────────────────────────────────────

function handleHomeFound(homeEl) {
  if (injected || document.getElementById('uhui-home-root')) {
    injected = true;
    stopPolling();
    return;
  }
  injected = true;
  stopPolling();

  requestAnimationFrame(() => {
    injectStyles();
    injectLayout(homeEl);
  });
}

function tryInjectIfHome() {
  if (injected) return;
  if (!isHomePage()) return;

  const home = document.querySelector('.homePage');
  if (home) {
    handleHomeFound(home);
  }
}

// ── MutationObserver ──────────────────────────────────────────────────────────

function checkMutation(mutations, obs) {
  for (const m of mutations) {
    for (const node of m.addedNodes) {
      if (node.nodeType !== Node.ELEMENT_NODE) continue;

      const home = node.matches?.('.homePage')
        ? node
        : node.querySelector?.('.homePage');

      if (home) {
        obs.disconnect();
        currentObserver = null;
        attached = false;
        handleHomeFound(home);
        return;
      }
    }
  }
}

function attachObserver() {
  if (attached || currentObserver) return;
  attached = true;
  currentObserver = new MutationObserver(checkMutation);
  currentObserver.observe(document.body, { childList: true, subtree: true });
}

function detachObserver() {
  if (currentObserver) {
    currentObserver.disconnect();
    currentObserver = null;
  }
  attached = false;
}

// ── Polling fallback ──────────────────────────────────────────────────────────
// Revisa cada 300 ms si ya existe .homePage pero el observer se lo perdió.

function startPolling() {
  if (pollTimer) return;
  pollTimer = setInterval(tryInjectIfHome, 300);
  // Límite de 60 s para no consumir recursos indefinidamente
  setTimeout(stopPolling, 60_000);
}

function stopPolling() {
  if (pollTimer) {
    clearInterval(pollTimer);
    pollTimer = null;
  }
}

// ── Estilos CSS ───────────────────────────────────────────────────────────────

function injectStyles() {
  if (document.getElementById('uhui-styles')) return;

  const link = document.createElement('link');
  link.id = 'uhui-styles';
  link.rel = 'stylesheet';
  // import.meta.url = https://server/UltimateHomeUI/Web/interceptor.js
  link.href = new URL('./styles.css', import.meta.url).href;
  document.head.appendChild(link);
}

// ── Punto de entrada ──────────────────────────────────────────────────────────

export function init() {
  window.__uhui_active = true;

  // ── Eventos del router de Jellyfin ──
  // Jellyfin 10.x dispara 'viewshow'/'viewhide' desde su gestor de vistas.
  // 'pageshow' es el equivalente en versiones más antiguas.
  const onViewShow = () => {
    if (isHomePage()) {
      if (document.getElementById('uhui-home-root')) {
        injected = true;
        stopPolling();
        return;
      }
      injected = false;
      attachObserver();
      tryInjectIfHome();
      startPolling();
    }
  };

  const onViewHide = () => {
    if (injected) {
      injected = false;
      stopPolling();
      detachObserver();
      destroyLayout();
    }
  };

  document.addEventListener('viewshow', onViewShow);
  document.addEventListener('pageshow', onViewShow);

  document.addEventListener('viewhide', onViewHide);
  document.addEventListener('pagehide', onViewHide);

  // Cambios de hash (navegación SPA sin eventos de Jellyfin)
  window.addEventListener('hashchange', () => {
    if (isHomePage()) {
      if (document.getElementById('uhui-home-root')) {
        injected = true;
        stopPolling();
        return;
      }
      injected = false;
      attachObserver();
      tryInjectIfHome();
      startPolling();
    } else if (injected) {
      injected = false;
      stopPolling();
      detachObserver();
      destroyLayout();
    }
  });

  // ── Intento inicial ──
  // El script se carga después del DOM (módulo ES deferido).
  // Si la home ya está en el DOM (ej: recarga directa en /), la inyectamos de inmediato.
  attachObserver();
  tryInjectIfHome();

  // El polling garantiza que capturamos el momento exacto aunque los eventos fallen.
  startPolling();

  console.log('[UHUI] Interceptor activo — esperando la pantalla de inicio');
}
