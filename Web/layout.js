/**
 * UHUI — Layout Builder
 * Constructs the Netflix-style home structure and orchestrates data loading.
 * Uses Jellyfin's global ApiClient when available, falls back to localStorage tokens.
 */

import { buildHero, destroyHero } from './hero.js';
import { buildRow } from './sections.js';
import { buildTabs } from './tabs.js';
import { initTVNav, destroyTVNav } from './tv-nav.js';

let homeRoot = null;

function getUserId() {
  if (typeof ApiClient !== 'undefined' && ApiClient.getCurrentUserId) {
    try { return ApiClient.getCurrentUserId(); } catch { /* fall through */ }
  }

  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    if (servers.length > 0 && servers[0].UserId) {
      return servers[0].UserId;
    }
  } catch { /* ignore */ }

  return null;
}

function getServerAddress() {
  if (typeof ApiClient !== 'undefined' && ApiClient.serverAddress) {
    try { return ApiClient.serverAddress(); } catch { /* fall through */ }
  }
  return '';
}

function getAuthHeaders() {
  if (typeof ApiClient !== 'undefined' && ApiClient.accessToken) {
    try {
      return { 'X-MediaBrowser-Token': ApiClient.accessToken() };
    } catch { /* fall through */ }
  }

  const headers = {};
  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    if (servers.length > 0 && servers[0].AccessToken) {
      headers['X-MediaBrowser-Token'] = servers[0].AccessToken;
    }
  } catch { /* ignore */ }
  return headers;
}

async function fetchHomeData(userId) {
  const base = getServerAddress();
  const url = base + '/Plugins/UltimateHomeUI/Home/' + userId;
  const response = await fetch(url, { headers: getAuthHeaders() });
  if (!response.ok) throw new Error('Home API returned ' + response.status);
  return response.json();
}

export async function injectLayout(homeEl) {
  const userId = getUserId();
  if (!userId) {
    console.warn('[UHUI] No user ID found — skipping layout injection');
    return;
  }

  homeEl.style.display = 'none';

  const container = document.createElement('div');
  container.className = 'uhui-home';
  container.id = 'uhui-home-root';
  homeEl.parentNode.insertBefore(container, homeEl.nextSibling);
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
    for (const section of sections) {
      const items = section.Items || section.items || [];
      if (items.length > 0) {
        const rowEl = buildRow({
          sectionId: section.SectionId || section.sectionId,
          title: section.Title || section.title,
          cardType: section.CardType || section.cardType,
          items: items,
        });
        rowsEl.appendChild(rowEl);
      }
    }

    if (sections.length === 0) {
      const empty = document.createElement('div');
      empty.style.cssText = 'text-align:center;padding:4rem 1rem;color:#888;font-size:1.1rem;';
      empty.textContent = 'No hay secciones configuradas. Ve a la configuración del plugin para personalizarlo.';
      rowsEl.appendChild(empty);
    }
  } catch (err) {
    console.error('[UHUI] Failed to load home data:', err);
    homeEl.style.display = '';
    container.remove();
    homeRoot = null;
    return;
  }

  initTVNav();
}

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
