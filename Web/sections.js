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
  const row = document.createElement('section');
  row.className = 'verticalSection';
  row.dataset.sectionId = section.sectionId;

  const header = document.createElement('div');
  header.className = 'sectionTitleContainer flex align-items-center';
  const title = document.createElement('h2');
  title.className = 'sectionTitle';
  title.textContent = section.title || '';
  header.appendChild(title);
  row.appendChild(header);

  const wrapper = document.createElement('div');
  wrapper.className = 'emby-scroller emby-scroller-with-paging';

  const track = document.createElement('div');
  track.className = 'itemsContainer scrollSlider';

  const cardType = CARD_TYPE_MAP[section.cardType] || 'portrait';

  for (const item of (section.items || [])) {
    const card = buildCard(item, cardType);
    track.appendChild(card);
  }

  wrapper.appendChild(track);
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
  const cardClasses = ['card'];
  if (cardType === 'landscape' || cardType === 'thumb') {
    cardClasses.push('backdropCard');
  } else if (cardType === 'square') {
    cardClasses.push('squareCard');
  } else {
    cardClasses.push('portraitCard');
  }
  card.className = cardClasses.join(' ');
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

  const cardScalable = document.createElement('div');
  cardScalable.className = 'cardScalable';
  const cardBox = document.createElement('div');
  cardBox.className = 'cardBox cardPadder';

  const imageContainer = document.createElement('div');
  imageContainer.className = 'cardImageContainer coveredImage defaultCardBackground';
  imageContainer.setAttribute('aria-hidden', 'true');
  const img = document.createElement('img');
  img.className = 'cardImage';
  img.alt = item.title || '';
  img.loading = 'lazy';
  img.src = resolvedUrl ? toClientImageUrl(resolvedUrl) : '';
  img.onerror = function () {
    const fallback = useBackdrop ? buildImageFallback(itemId, 'Backdrop', 800) : buildImageFallback(itemId, 'Primary', 420);
    if (fallback) this.src = toClientImageUrl(fallback);
  };
  imageContainer.appendChild(img);
  cardBox.appendChild(imageContainer);

  const text = document.createElement('div');
  text.className = 'cardText cardTextCentered';
  text.textContent = item.title || '';

  cardBox.appendChild(text);
  cardScalable.appendChild(cardBox);
  card.appendChild(cardScalable);

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
