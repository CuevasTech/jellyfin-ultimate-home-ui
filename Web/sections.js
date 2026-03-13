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
  row.className = 'verticalSection uhui-native-section';
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
  track.className = 'itemsContainer scrollSlider focuscontainer-x';

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
  const cardClasses = ['card', 'uhui-native-card'];
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

  const cardBox = document.createElement('div');
  cardBox.className = 'cardBox cardPadder';

  const image = document.createElement('div');
  image.className = 'cardImageContainer coveredImage defaultCardBackground';
  image.style.backgroundImage = resolvedUrl ? `url("${toClientImageUrl(resolvedUrl)}")` : '';
  image.setAttribute('aria-hidden', 'true');
  cardBox.appendChild(image);

  const text = document.createElement('div');
  text.className = 'cardText cardTextCentered';
  text.textContent = item.title || '';

  if (item.subtitle) {
    const sub = document.createElement('div');
    sub.className = 'cardText cardText-secondary';
    sub.textContent = item.subtitle;
    card.appendChild(sub);
  }

  card.appendChild(cardBox);
  card.appendChild(text);

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
    fav.textContent = 'favorite';
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
