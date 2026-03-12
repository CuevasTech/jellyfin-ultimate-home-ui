/**
 * UHUI — Layout Builder
 * Constructs the Netflix-style home structure and orchestrates data loading.
 */

import { buildHero, destroyHero } from './hero.js';
import { buildRow } from './sections.js';
import { buildTabs } from './tabs.js';
import { initTVNav, destroyTVNav } from './tv-nav.js';

let homeRoot = null;

function getUserId() {
  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    if (servers.length > 0 && servers[0].UserId) {
      return servers[0].UserId;
    }
  } catch { /* ignore */ }

  const meta = document.querySelector('meta[name="user-id"]');
  if (meta) return meta.content;

  return null;
}

function getApiHeaders() {
  const headers = { 'Content-Type': 'application/json' };
  try {
    const creds = JSON.parse(localStorage.getItem('jellyfin_credentials') || '{}');
    const servers = creds.Servers || [];
    if (servers.length > 0 && servers[0].AccessToken) {
      headers['Authorization'] = `MediaBrowser Token="${servers[0].AccessToken}"`;
    }
  } catch { /* ignore */ }
  return headers;
}

async function fetchHomeData(userId) {
  const response = await fetch(`/Plugins/UltimateHomeUI/Home/${userId}`, {
    headers: getApiHeaders()
  });
  if (!response.ok) throw new Error(`Home API returned ${response.status}`);
  return response.json();
}

export async function injectLayout(homeEl) {
  const userId = getUserId();
  if (!userId) return;

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

    buildTabs(tabsEl, data.tabs || []);
    buildHero(heroEl, data.heroItems || [], userId);

    for (const section of (data.sections || [])) {
      if (section.items && section.items.length > 0) {
        const rowEl = buildRow(section);
        rowsEl.appendChild(rowEl);
      }
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
