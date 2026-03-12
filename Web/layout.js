/**
 * UHUI — Layout Builder
 * Construye la estructura Netflix-style y orquesta la carga de datos.
 */

import { buildHero, destroyHero } from './hero.js';
import { buildRow } from './sections.js';
import { buildTabs } from './tabs.js';
import { initTVNav, destroyTVNav } from './tv-nav.js';

let homeRoot = null;

// ── Autenticación ─────────────────────────────────────────────────────────────

/**
 * Intenta obtener el userId del usuario actual usando múltiples métodos.
 * Jellyfin expone el ApiClient como global en la ventana.
 */
function getUserId() {
  // Método 1: ApiClient.getCurrentUserId() — API estándar de jellyfin-apiclient-javascript
  if (typeof ApiClient !== 'undefined') {
    try {
      const id = ApiClient.getCurrentUserId?.();
      if (id) return id;
    } catch { /* ignorar */ }

    // Método 2: propiedad interna _currentUser (algunas versiones)
    try {
      const id = ApiClient._currentUser?.Id;
      if (id) return id;
    } catch { /* ignorar */ }
  }

  // Método 3: localStorage jellyfin_credentials (fallback universal)
  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    for (const server of servers) {
      if (server.UserId) return server.UserId;
    }
  } catch { /* ignorar */ }

  return null;
}

/**
 * Espera hasta maxWaitMs a que el userId esté disponible (autenticación async).
 */
async function waitForUserId(maxWaitMs = 8000) {
  const start = Date.now();
  while (Date.now() - start < maxWaitMs) {
    const id = getUserId();
    if (id) return id;
    await new Promise(r => setTimeout(r, 300));
  }
  return null;
}

function getAccessToken() {
  if (typeof ApiClient !== 'undefined') {
    try {
      const token = ApiClient.accessToken?.();
      if (token && token !== 'null' && token !== 'undefined') return token;
    } catch { /* ignorar */ }
  }

  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    for (const server of servers) {
      if (server.AccessToken) return server.AccessToken;
    }
  } catch { /* ignorar */ }

  return null;
}

function getServerAddress() {
  if (typeof ApiClient !== 'undefined') {
    try {
      const addr = ApiClient.serverAddress?.();
      if (addr) return addr.replace(/\/$/, '');
    } catch { /* ignorar */ }
  }
  // Si no hay dirección, usamos rutas relativas (mismo origen)
  return '';
}

function buildHeaders() {
  const headers = {};
  const token = getAccessToken();
  if (token) {
    headers['X-MediaBrowser-Token'] = token;
  }
  return headers;
}

// ── Fetch de datos ────────────────────────────────────────────────────────────

async function fetchHomeData(userId) {
  const base = getServerAddress();
  const url = `${base}/Plugins/UltimateHomeUI/Home/${userId}`;

  const response = await fetch(url, {
    headers: buildHeaders(),
    credentials: 'same-origin',
  });

  if (!response.ok) {
    throw new Error(`[UHUI] API /Home/${userId} devolvió ${response.status}`);
  }

  return response.json();
}

// ── Inyección del layout ──────────────────────────────────────────────────────

export async function injectLayout(homeEl) {
  // Esperar a tener userId (autenticación puede no estar completa al instante)
  const userId = await waitForUserId();

  if (!userId) {
    console.warn('[UHUI] No se encontró userId tras esperar — se muestra el home original');
    return;
  }

  // Ocultar home original y añadir nuestro contenedor
  homeEl.style.display = 'none';

  const container = document.createElement('div');
  container.className = 'uhui-home';
  container.id = 'uhui-home-root';

  // Insertar junto al homeEl, no dentro (para no interferir con Jellyfin)
  if (homeEl.parentNode) {
    homeEl.parentNode.insertBefore(container, homeEl.nextSibling);
  } else {
    document.body.appendChild(container);
  }

  homeRoot = container;

  const tabsEl = document.createElement('nav');
  tabsEl.className = 'uhui-tabs';
  container.appendChild(tabsEl);

  const heroEl = document.createElement('section');
  heroEl.className = 'uhui-hero';
  container.appendChild(heroEl);

  const rowsEl = document.createElement('div');
  rowsEl.className = 'uhui-rows-container';
  container.appendChild(rowsEl);

  try {
    const data = await fetchHomeData(userId);

    buildTabs(tabsEl, data.Tabs || data.tabs || []);
    buildHero(heroEl, data.HeroItems || data.heroItems || [], userId);

    const sections = data.Sections || data.sections || [];
    let visibleRows = 0;

    for (const section of sections) {
      const items = section.Items || section.items || [];
      if (items.length > 0) {
        const rowEl = buildRow({
          sectionId: section.SectionId || section.sectionId,
          title: section.Title || section.title,
          cardType: section.CardType ?? section.cardType,
          items,
        });
        rowsEl.appendChild(rowEl);
        visibleRows++;
      }
    }

    if (visibleRows === 0) {
      const empty = document.createElement('div');
      empty.style.cssText = 'text-align:center;padding:4rem 1rem;color:#888;font-size:1.1rem;';
      empty.textContent = 'No hay contenido disponible. Comprueba que las bibliotecas de Jellyfin tienen contenido.';
      rowsEl.appendChild(empty);
    }
  } catch (err) {
    console.error('[UHUI] Error cargando datos del home:', err);
    // Fallback: mostrar el home original de Jellyfin
    homeEl.style.display = '';
    container.remove();
    homeRoot = null;
    return;
  }

  initTVNav();
}

// ── Destrucción del layout ────────────────────────────────────────────────────

export function destroyLayout() {
  destroyHero();
  destroyTVNav();

  if (homeRoot) {
    homeRoot.remove();
    homeRoot = null;
  }

  const original = document.querySelector('.homePage');
  if (original) {
    original.style.display = '';
  }
}
