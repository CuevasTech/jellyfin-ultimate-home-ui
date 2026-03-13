/**
 * UHUI — Layout Builder
 * Construye la estructura Netflix-style y orquesta la carga de datos.
 */

import { buildHero, destroyHero } from './hero.js';
import { buildRow } from './sections.js';
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
  // Ruta preferida: usa ApiClient para que Jellyfin gestione auth/device headers.
  if (typeof ApiClient !== 'undefined' && ApiClient.getJSON && ApiClient.getUrl) {
    const apiUrl = ApiClient.getUrl(`Plugins/UltimateHomeUI/Home/${userId}`);
    return ApiClient.getJSON(apiUrl);
  }

  // Fallback: fetch manual si ApiClient no está disponible.
  const base = getServerAddress();
  const url = `${base}/Plugins/UltimateHomeUI/Home/${userId}`;
  const response = await fetch(url, {
    headers: buildHeaders(),
    credentials: 'same-origin',
  });
  if (!response.ok) throw new Error(`[UHUI] API /Home/${userId} devolvió ${response.status}`);
  return response.json();
}

function pick(obj, ...keys) {
  for (const key of keys) {
    if (obj && obj[key] !== undefined && obj[key] !== null) {
      return obj[key];
    }
  }

  return null;
}

function normalizeMediaItem(item) {
  if (!item) return null;

  return {
    itemId: pick(item, 'itemId', 'ItemId', 'id', 'Id'),
    title: pick(item, 'title', 'Title', 'name', 'Name') || '',
    subtitle: pick(item, 'subtitle', 'Subtitle'),
    primaryImageUrl: pick(item, 'primaryImageUrl', 'PrimaryImageUrl'),
    backdropImageUrl: pick(item, 'backdropImageUrl', 'BackdropImageUrl'),
    mediaType: pick(item, 'mediaType', 'MediaType'),
    year: pick(item, 'year', 'Year'),
    communityRating: pick(item, 'communityRating', 'CommunityRating'),
    officialRating: pick(item, 'officialRating', 'OfficialRating'),
    playbackProgress: pick(item, 'playbackProgress', 'PlaybackProgress'),
    isPlayed: !!pick(item, 'isPlayed', 'IsPlayed'),
    isFavorite: !!pick(item, 'isFavorite', 'IsFavorite'),
    unplayedCount: pick(item, 'unplayedCount', 'UnplayedCount'),
  };
}

function normalizeHeroItem(item) {
  if (!item) return null;
  return {
    itemId: pick(item, 'itemId', 'ItemId', 'id', 'Id'),
    title: pick(item, 'title', 'Title', 'name', 'Name') || '',
    tagline: pick(item, 'tagline', 'Tagline'),
    overview: pick(item, 'overview', 'Overview'),
    year: pick(item, 'year', 'Year'),
    officialRating: pick(item, 'officialRating', 'OfficialRating'),
    communityRating: pick(item, 'communityRating', 'CommunityRating'),
    runtimeMinutes: pick(item, 'runtimeMinutes', 'RuntimeMinutes'),
    genres: pick(item, 'genres', 'Genres'),
    backdropUrl: pick(item, 'backdropUrl', 'BackdropUrl'),
    logoUrl: pick(item, 'logoUrl', 'LogoUrl'),
    trailerUrl: pick(item, 'trailerUrl', 'TrailerUrl'),
    mediaType: pick(item, 'mediaType', 'MediaType'),
    isFavorite: !!pick(item, 'isFavorite', 'IsFavorite'),
  };
}

// ── Inyección del layout ──────────────────────────────────────────────────────

export async function injectLayout(homeEl) {
  const existing = document.getElementById('uhui-home-root');
  if (existing) {
    existing.remove();
  }

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

  const heroEl = document.createElement('section');
  heroEl.className = 'uhui-hero';
  container.appendChild(heroEl);

  const rowsEl = document.createElement('div');
  rowsEl.className = 'uhui-rows-container homeSectionsContainer sections';
  container.appendChild(rowsEl);

  try {
    const data = await fetchHomeData(userId);
    const heroData = pick(data, 'heroItems', 'HeroItems') || [];
    const normalizedHero = heroData.map(normalizeHeroItem).filter(Boolean);

    const heroIntervalRaw = pick(data, 'heroSliderIntervalSeconds', 'HeroSliderIntervalSeconds');
    const heroInterval = Math.max(3, Math.min(120, parseInt(heroIntervalRaw, 10) || 8));
    const heroAutoTrailer = pick(data, 'heroSliderAutoPlayTrailer', 'HeroSliderAutoPlayTrailer');
    buildHero(heroEl, normalizedHero, userId, {
      intervalSeconds: heroInterval,
      autoPlayTrailer: heroAutoTrailer !== false,
    });

    const sections = pick(data, 'sections', 'Sections') || [];
    let visibleRows = 0;

    for (const section of sections) {
      const rawItems = pick(section, 'items', 'Items') || [];
      const items = rawItems.map(normalizeMediaItem).filter(Boolean);
      if (items.length > 0) {
        const rowEl = buildRow({
          sectionId: pick(section, 'sectionId', 'SectionId'),
          title: pick(section, 'title', 'Title'),
          cardType: pick(section, 'cardType', 'CardType'),
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
