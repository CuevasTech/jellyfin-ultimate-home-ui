/**
 * UHUI — Section / Row Builder
 * Creates horizontal carousels with 4 card types: Square, Thumb, Portrait, Landscape.
 * Includes progress bars, favorite badges, scroll buttons, and swipe support.
 */

import { addSwipe } from './touch.js';

const CARD_TYPE_MAP = {
  0: 'square',
  1: 'thumb',
  2: 'portrait',
  3: 'landscape',
  'Square': 'square',
  'Thumb': 'thumb',
  'Portrait': 'portrait',
  'Landscape': 'landscape',
};

export function buildRow(section) {
  const row = document.createElement('div');
  row.className = 'uhui-row';
  row.dataset.sectionId = section.sectionId;

  const header = document.createElement('div');
  header.className = 'uhui-row__header';
  const title = document.createElement('h2');
  title.className = 'uhui-row__title';
  title.textContent = section.title || '';
  header.appendChild(title);
  row.appendChild(header);

  const wrapper = document.createElement('div');
  wrapper.className = 'uhui-row__wrapper';

  const track = document.createElement('div');
  track.className = 'uhui-row__track';

  const cardType = CARD_TYPE_MAP[section.cardType] || 'portrait';

  for (const item of (section.items || [])) {
    const card = buildCard(item, cardType);
    track.appendChild(card);
  }

  wrapper.appendChild(track);

  const prevBtn = document.createElement('button');
  prevBtn.className = 'uhui-row__btn uhui-row__btn--prev';
  prevBtn.setAttribute('aria-label', 'Anterior');
  prevBtn.textContent = '‹';
  prevBtn.addEventListener('click', () => {
    track.scrollBy({ left: -track.clientWidth * 0.8, behavior: 'smooth' });
  });

  const nextBtn = document.createElement('button');
  nextBtn.className = 'uhui-row__btn uhui-row__btn--next';
  nextBtn.setAttribute('aria-label', 'Siguiente');
  nextBtn.textContent = '›';
  nextBtn.addEventListener('click', () => {
    track.scrollBy({ left: track.clientWidth * 0.8, behavior: 'smooth' });
  });

  wrapper.appendChild(prevBtn);
  wrapper.appendChild(nextBtn);
  row.appendChild(wrapper);

  addSwipe(track, {
    onLeft: () => track.scrollBy({ left: track.clientWidth * 0.6, behavior: 'smooth' }),
    onRight: () => track.scrollBy({ left: -track.clientWidth * 0.6, behavior: 'smooth' }),
  });

  return row;
}

function toClientImageUrl(path) {
  if (!path) return '';

  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const clean = path.replace(/^\/+/, '');
  if (typeof ApiClient !== 'undefined' && typeof ApiClient.getUrl === 'function') {
    return ApiClient.getUrl(clean);
  }

  return '/' + clean;
}

function buildImageFallback(itemId, type, maxWidth = 420) {
  if (!itemId) return '';
  if (typeof ApiClient !== 'undefined' && typeof ApiClient.getImageUrl === 'function') {
    try {
      return ApiClient.getImageUrl(itemId, { type, maxWidth, quality: 90 });
    } catch {
      // ignore and fallback to static path
    }
  }

  return toClientImageUrl(`/Items/${itemId}/Images/${type}?maxWidth=${maxWidth}&quality=90`);
}

function buildCard(item, cardType) {
  const card = document.createElement('div');
  card.className = `uhui-card uhui-card--${cardType}`;
  card.setAttribute('data-focusable', '');
  card.setAttribute('tabindex', '0');
  const itemId = item.itemId;
  card.dataset.itemId = itemId || '';

  const useBackdrop = cardType === 'thumb' || cardType === 'landscape';
  const imgUrl = useBackdrop && item.backdropImageUrl
    ? item.backdropImageUrl
    : (item.primaryImageUrl || (useBackdrop ? '' : buildImageFallback(itemId, 'Primary')));
  const resolvedUrl = imgUrl || (useBackdrop
    ? buildImageFallback(itemId, 'Backdrop', 800)
    : buildImageFallback(itemId, 'Primary', 420));

  if (resolvedUrl) {
    const img = document.createElement('img');
    img.src = toClientImageUrl(resolvedUrl);
    img.alt = item.title || '';
    img.loading = 'lazy';
    img.decoding = 'async';
    img.onerror = () => {
      img.src = buildImageFallback(itemId, useBackdrop ? 'Primary' : 'Backdrop', useBackdrop ? 800 : 420);
    };
    card.appendChild(img);
  } else {
    const placeholder = document.createElement('div');
    placeholder.className = 'uhui-card__placeholder';
    placeholder.textContent = item.title || '';
    card.appendChild(placeholder);
  }

  const overlay = document.createElement('div');
  overlay.className = 'uhui-card__overlay';

  const titleEl = document.createElement('div');
  titleEl.className = 'uhui-card__title';
  titleEl.textContent = item.title || '';
  overlay.appendChild(titleEl);

  if (item.subtitle) {
    const sub = document.createElement('div');
    sub.className = 'uhui-card__subtitle';
    sub.textContent = item.subtitle;
    overlay.appendChild(sub);
  }

  card.appendChild(overlay);

  if (item.playbackProgress && item.playbackProgress > 0) {
    const progress = document.createElement('div');
    progress.className = 'uhui-card__progress';
    const fill = document.createElement('div');
    fill.className = 'uhui-card__progress-fill';
    fill.style.width = `${Math.min(item.playbackProgress * 100, 100)}%`;
    progress.appendChild(fill);
    card.appendChild(progress);
  }

  if (item.isFavorite) {
    const fav = document.createElement('span');
    fav.className = 'uhui-card__fav uhui-card__fav--visible';
    fav.textContent = '♥';
    card.appendChild(fav);
  }

  if (item.isPlayed) {
    const played = document.createElement('div');
    played.className = 'uhui-card__played';
    played.textContent = '✓';
    card.appendChild(played);
  }

  if (item.unplayedCount && item.unplayedCount > 0) {
    const badge = document.createElement('div');
    badge.className = 'uhui-card__unplayed';
    badge.textContent = item.unplayedCount.toString();
    card.appendChild(badge);
  }

  card.addEventListener('click', () => {
    if (itemId) {
      window.location.href = `#!/details?id=${itemId}`;
    }
  });

  card.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' || e.keyCode === 13) {
      if (itemId) {
        window.location.href = `#!/details?id=${itemId}`;
      }
    }
  });

  return card;
}
