/**
 * UHUI — Hero Media Slider
 * Full-viewport banner with backdrop/trailer autoplay, metadata overlay,
 * dot indicators, and auto-rotation.
 */

let slideTimer = null;
let currentIndex = 0;
let heroItems = [];
let heroEl = null;
let videoEl = null;
let isMuted = true;

const SLIDE_INTERVAL = 8000;

export function buildHero(container, items, userId) {
  heroEl = container;
  heroItems = items;
  currentIndex = 0;

  if (!items || items.length === 0) {
    container.style.display = 'none';
    return;
  }

  renderSlide(container, items[0]);
  buildDots(container, items.length);

  if (items.length > 1) {
    slideTimer = setInterval(() => {
      currentIndex = (currentIndex + 1) % heroItems.length;
      renderSlide(container, heroItems[currentIndex]);
      updateDots(container, currentIndex);
    }, SLIDE_INTERVAL);
  }
}

function renderSlide(container, item) {
  clearMedia(container);

  if (item.trailerUrl && isEmbeddableTrailer(item.trailerUrl)) {
    renderTrailer(container, item);
  } else if (item.backdropUrl) {
    const img = document.createElement('img');
    img.className = 'uhui-hero__backdrop';
    img.src = item.backdropUrl;
    img.alt = item.title || '';
    img.loading = 'eager';
    img.decoding = 'async';
    container.prepend(img);
  }

  const existingGrad = container.querySelector('.uhui-hero__gradient');
  if (!existingGrad) {
    const grad = document.createElement('div');
    grad.className = 'uhui-hero__gradient';
    container.appendChild(grad);
  }

  renderMeta(container, item);
}

function renderTrailer(container, item) {
  const video = document.createElement('video');
  video.className = 'uhui-hero__backdrop uhui-hero__backdrop--video';
  video.src = item.trailerUrl;
  video.autoplay = true;
  video.loop = true;
  video.muted = isMuted;
  video.playsInline = true;
  video.poster = item.backdropUrl || '';
  videoEl = video;
  container.prepend(video);

  video.play().catch(() => {
    video.remove();
    videoEl = null;
    if (item.backdropUrl) {
      const img = document.createElement('img');
      img.className = 'uhui-hero__backdrop';
      img.src = item.backdropUrl;
      img.alt = item.title || '';
      container.prepend(img);
    }
  });

  buildSoundToggle(container);
}

function isEmbeddableTrailer(url) {
  if (!url) return false;
  return url.endsWith('.mp4') || url.endsWith('.webm') || url.includes('/Videos/');
}

function buildSoundToggle(container) {
  let btn = container.querySelector('.uhui-hero__sound');
  if (btn) return;

  btn = document.createElement('button');
  btn.className = 'uhui-hero__sound';
  btn.setAttribute('aria-label', 'Toggle sound');
  btn.textContent = isMuted ? '🔇' : '🔊';
  btn.addEventListener('click', () => {
    isMuted = !isMuted;
    btn.textContent = isMuted ? '🔇' : '🔊';
    if (videoEl) videoEl.muted = isMuted;
  });
  container.appendChild(btn);
}

function renderMeta(container, item) {
  let meta = container.querySelector('.uhui-hero__meta');
  if (meta) meta.remove();

  meta = document.createElement('div');
  meta.className = 'uhui-hero__meta';

  if (item.logoUrl) {
    const logo = document.createElement('img');
    logo.className = 'uhui-hero__logo';
    logo.src = item.logoUrl;
    logo.alt = item.title || '';
    logo.loading = 'eager';
    meta.appendChild(logo);
  } else {
    const title = document.createElement('h1');
    title.className = 'uhui-hero__title';
    title.textContent = item.title || '';
    meta.appendChild(title);
  }

  const info = document.createElement('div');
  info.className = 'uhui-hero__info';

  if (item.communityRating) {
    const rating = document.createElement('span');
    rating.className = 'uhui-hero__rating';
    rating.textContent = `★ ${item.communityRating.toFixed(1)}`;
    info.appendChild(rating);
  }

  if (item.year) {
    const year = document.createElement('span');
    year.textContent = item.year;
    info.appendChild(year);
  }

  if (item.officialRating) {
    const badge = document.createElement('span');
    badge.className = 'uhui-hero__badge';
    badge.textContent = item.officialRating;
    info.appendChild(badge);
  }

  if (item.runtimeMinutes) {
    const runtime = document.createElement('span');
    const h = Math.floor(item.runtimeMinutes / 60);
    const m = item.runtimeMinutes % 60;
    runtime.textContent = h > 0 ? `${h}h ${m}m` : `${m}m`;
    info.appendChild(runtime);
  }

  if (item.genres) {
    const genres = document.createElement('span');
    genres.textContent = item.genres;
    genres.className = 'uhui-hero__genres';
    info.appendChild(genres);
  }

  meta.appendChild(info);

  if (item.overview) {
    const overview = document.createElement('p');
    overview.className = 'uhui-hero__overview';
    overview.textContent = item.overview;
    meta.appendChild(overview);
  }

  const actions = document.createElement('div');
  actions.className = 'uhui-hero__actions';

  const playBtn = document.createElement('button');
  playBtn.className = 'uhui-hero__btn uhui-hero__btn--play';
  playBtn.setAttribute('data-focusable', '');
  playBtn.textContent = '▶ Reproducir';
  playBtn.addEventListener('click', () => {
    window.location.href = `#!/item?id=${item.itemId}&serverId=`;
  });
  actions.appendChild(playBtn);

  const favBtn = document.createElement('button');
  favBtn.className = `uhui-hero__btn uhui-hero__btn--fav${item.isFavorite ? ' is-fav' : ''}`;
  favBtn.setAttribute('data-focusable', '');
  favBtn.textContent = item.isFavorite ? '♥ Favorito' : '♡ Favorito';
  actions.appendChild(favBtn);

  meta.appendChild(actions);
  container.appendChild(meta);
}

function buildDots(container, count) {
  if (count <= 1) return;

  const dotsWrap = document.createElement('div');
  dotsWrap.className = 'uhui-hero__dots';

  for (let i = 0; i < count; i++) {
    const dot = document.createElement('button');
    dot.className = `uhui-hero__dot${i === 0 ? ' uhui-hero__dot--active' : ''}`;
    dot.setAttribute('aria-label', `Slide ${i + 1}`);
    dot.addEventListener('click', () => {
      currentIndex = i;
      renderSlide(container, heroItems[i]);
      updateDots(container, i);
      resetTimer();
    });
    dotsWrap.appendChild(dot);
  }

  container.appendChild(dotsWrap);
}

function updateDots(container, activeIdx) {
  const dots = container.querySelectorAll('.uhui-hero__dot');
  dots.forEach((d, i) => {
    d.classList.toggle('uhui-hero__dot--active', i === activeIdx);
  });
}

function resetTimer() {
  if (slideTimer) clearInterval(slideTimer);
  if (heroItems.length > 1) {
    slideTimer = setInterval(() => {
      currentIndex = (currentIndex + 1) % heroItems.length;
      renderSlide(heroEl, heroItems[currentIndex]);
      updateDots(heroEl, currentIndex);
    }, SLIDE_INTERVAL);
  }
}

function clearMedia(container) {
  const oldImg = container.querySelector('.uhui-hero__backdrop:not(.uhui-hero__backdrop--video)');
  if (oldImg) oldImg.remove();
  if (videoEl) {
    videoEl.pause();
    videoEl.remove();
    videoEl = null;
  }
}

export function destroyHero() {
  if (slideTimer) {
    clearInterval(slideTimer);
    slideTimer = null;
  }
  if (videoEl) {
    videoEl.pause();
    videoEl.remove();
    videoEl = null;
  }
  heroItems = [];
  currentIndex = 0;
  heroEl = null;
}
