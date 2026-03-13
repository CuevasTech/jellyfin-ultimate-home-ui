/**
 * UHUI — Hero Media Slider
 * Estructura y clases idénticas a jellyfin-plugin-media-bar (slideshowpure)
 * para que el aspecto visual sea el mismo: blur, máscaras, logo, plot, botones.
 */

let slideTimer = null;
let currentIndex = 0;
let heroItems = [];
let heroRoot = null;
let slideIntervalMs = 8000;
let allowTrailerAutoplay = true;

function toClientImageUrl(path) {
  if (!path) return '';
  if (/^https?:\/\//i.test(path)) return path;
  const clean = path.replace(/^\/+/, '');
  if (typeof ApiClient !== 'undefined' && typeof ApiClient.getUrl === 'function') {
    return ApiClient.getUrl(clean);
  }
  return '/' + clean;
}

function buildImageUrl(itemId, type, maxWidth = 1920) {
  if (!itemId) return '';
  if (typeof ApiClient !== 'undefined' && typeof ApiClient.getImageUrl === 'function') {
    try {
      return ApiClient.getImageUrl(itemId, { type, maxWidth, quality: 90 });
    } catch {
      // ignore
    }
  }
  return toClientImageUrl(`/Items/${itemId}/Images/${type}?maxWidth=${maxWidth}&quality=90`);
}

function extractYouTubeId(url) {
  if (!url) return null;
  const match = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&?/]+)/i);
  return match ? match[1] : null;
}

function createEl(tag, attrs) {
  const el = document.createElement(tag);
  if (attrs) {
    if (attrs.className) el.className = attrs.className;
    if (attrs.src) el.src = toClientImageUrl(attrs.src);
    if (attrs.alt !== undefined) el.alt = attrs.alt;
    if (attrs.loading) el.loading = attrs.loading;
    if (attrs.innerHTML !== undefined) el.innerHTML = attrs.innerHTML;
    if (attrs.textContent !== undefined) el.textContent = attrs.textContent;
    if (attrs['data-item-id']) el.dataset.itemId = attrs['data-item-id'];
    if (attrs.type) el.type = attrs.type;
    Object.keys(attrs).forEach((k) => {
      if (['className', 'src', 'alt', 'loading', 'innerHTML', 'textContent', 'data-item-id', 'type'].includes(k)) return;
      if (k.startsWith('on') && typeof attrs[k] === 'function') el.addEventListener(k.slice(2).toLowerCase(), attrs[k]);
      else if (k === 'tabIndex') el.tabIndex = attrs[k];
      else if (k === 'ariaLabel') el.setAttribute('aria-label', attrs[k]);
    });
  }
  return el;
}

function buildOneSlide(item) {
  const itemId = item.itemId;
  const slide = createEl('div', { className: 'slide', 'data-item-id': itemId });

  const backdropUrl = item.backdropUrl || buildImageUrl(itemId, 'Backdrop', 1920);
  const backdrop = createEl('img', {
    className: 'backdrop high-quality',
    src: backdropUrl,
    alt: item.title || '',
    loading: 'eager',
  });
  backdrop.onerror = function () {
    this.src = toClientImageUrl(buildImageUrl(itemId, 'Primary', 1280));
  };

  const backdropOverlay = createEl('div', { className: 'backdrop-overlay' });
  const backdropContainer = createEl('div', { className: 'backdrop-container' });
  backdropContainer.append(backdrop, backdropOverlay);

  const logoUrl = item.logoUrl || buildImageUrl(itemId, 'Logo', 400);
  const logo = createEl('img', {
    className: 'logo high-quality',
    src: logoUrl,
    alt: item.title || '',
    loading: 'eager',
  });
  logo.onerror = function () {
    this.style.display = 'none';
  };
  const logoContainer = createEl('div', { className: 'logo-container' });
  logoContainer.appendChild(logo);

  const gradientOverlay = createEl('div', { className: 'gradient-overlay' });

  const plotText = (item.overview || '').slice(0, 360);
  const plotEl = createEl('div', { className: 'plot', textContent: plotText });
  const plotContainer = createEl('div', { className: 'plot-container' });
  plotContainer.appendChild(plotEl);

  const infoContainer = createEl('div', { className: 'info-container' });
  const miscInfo = createEl('div', { className: 'misc-info' });
  if (item.communityRating != null) {
    const star = createEl('span', { className: 'community-rating-star', textContent: '★ ' + Number(item.communityRating).toFixed(1) });
    miscInfo.appendChild(star);
  }
  if (item.year) {
    const year = createEl('span', { className: 'date', textContent: item.year });
    miscInfo.appendChild(year);
  }
  if (item.officialRating) {
    const age = createEl('span', { className: 'age-rating', textContent: item.officialRating });
    miscInfo.appendChild(age);
  }
  if (item.runtimeMinutes != null) {
    const h = Math.floor(item.runtimeMinutes / 60);
    const m = item.runtimeMinutes % 60;
    const run = createEl('span', { className: 'runTime', textContent: h > 0 ? `${h}h ${m}m` : `${m}m` });
    miscInfo.appendChild(run);
  }
  infoContainer.appendChild(miscInfo);

  const genreStr = Array.isArray(item.genres) ? item.genres.join(' · ') : (item.genres || '');
  const genreEl = createEl('div', { className: 'genre', textContent: genreStr });

  const buttonContainer = createEl('div', { className: 'button-container' });
  const playBtn = createEl('button', {
    className: 'play-button',
    innerHTML: ' Reproducir ',
    tabIndex: 0,
    onClick: (e) => {
      e.preventDefault();
      e.stopPropagation();
      if (itemId) window.location.href = `#!/details?id=${itemId}`;
    },
  });
  const detailBtn = createEl('button', { className: 'detail-button', tabIndex: 0, onClick: (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (itemId) window.location.href = `#!/details?id=${itemId}`;
  } });
  const favBtn = createEl('button', {
    className: `favorite-button${item.isFavorite ? ' favorited' : ''}`,
    tabIndex: 0,
    onClick: (e) => { e.preventDefault(); e.stopPropagation(); },
  });
  buttonContainer.append(detailBtn, playBtn, favBtn);

  slide.append(logoContainer, backdropContainer, gradientOverlay, plotContainer, infoContainer, genreEl, buttonContainer);

  if (allowTrailerAutoplay && item.trailerUrl) {
    const ytId = extractYouTubeId(item.trailerUrl);
    if (ytId) {
      const videoContainer = createEl('div', { className: 'video-container', id: `trailer-${itemId}` });
      const playerDiv = createEl('div', { className: 'video-player', id: `yt-player-${itemId}` });
      videoContainer.appendChild(playerDiv);
      slide.insertBefore(videoContainer, slide.firstChild);
      slide.dataset.ytId = ytId;
    }
  }

  return slide;
}

function setActiveSlide(slidesContainer, index) {
  const slides = slidesContainer.querySelectorAll('.slide');
  slides.forEach((s, i) => s.classList.toggle('active', i === index));
  const activeSlide = slides[index];
  if (activeSlide) {
    const backdrop = activeSlide.querySelector('.backdrop');
    const plotContainer = activeSlide.querySelector('.plot-container');
    const hasVideo = activeSlide.querySelector('.video-container');
    if (backdrop) backdrop.classList.toggle('with-video', !!hasVideo);
    if (plotContainer) plotContainer.classList.toggle('with-video', !!hasVideo);
  }
}

function updateDots(slidesContainer, activeIdx) {
  const dots = slidesContainer.querySelectorAll('.dot');
  dots.forEach((d, i) => d.classList.toggle('active', i === activeIdx));
}

export function buildHero(container, items, userId, options = {}) {
  heroRoot = container;
  heroItems = items || [];
  currentIndex = 0;
  slideIntervalMs = Math.max((options.intervalSeconds || 8) * 1000, 2000);
  allowTrailerAutoplay = options.autoPlayTrailer !== false;

  container.innerHTML = '';
  if (!heroItems.length) {
    container.style.display = 'none';
    return;
  }
  container.style.display = '';

  const slidesContainer = createEl('div', { id: 'slides-container' });
  container.appendChild(slidesContainer);

  heroItems.forEach((item) => {
    const slide = buildOneSlide(item);
    slidesContainer.appendChild(slide);
  });

  const firstSlide = slidesContainer.querySelector('.slide');
  if (firstSlide) firstSlide.classList.add('active');
  const firstBackdrop = firstSlide?.querySelector('.backdrop');
  const firstPlot = firstSlide?.querySelector('.plot-container');
  const firstHasVideo = firstSlide?.querySelector('.video-container');
  if (firstBackdrop) firstBackdrop.classList.toggle('with-video', !!firstHasVideo);
  if (firstPlot) firstPlot.classList.toggle('with-video', !!firstHasVideo);

  if (heroItems.length > 1) {
    const leftArrow = createEl('div', { className: 'arrow left-arrow', innerHTML: '&#10094;' });
    leftArrow.addEventListener('click', () => {
      currentIndex = (currentIndex - 1 + heroItems.length) % heroItems.length;
      setActiveSlide(slidesContainer, currentIndex);
      updateDots(slidesContainer, currentIndex);
      resetTimer();
    });
    const rightArrow = createEl('div', { className: 'arrow right-arrow', innerHTML: '&#10095;' });
    rightArrow.addEventListener('click', () => {
      currentIndex = (currentIndex + 1) % heroItems.length;
      setActiveSlide(slidesContainer, currentIndex);
      updateDots(slidesContainer, currentIndex);
      resetTimer();
    });
    slidesContainer.appendChild(leftArrow);
    slidesContainer.appendChild(rightArrow);

    const dotsContainer = createEl('div', { className: 'dots-container' });
    for (let i = 0; i < heroItems.length; i++) {
      const dot = createEl('button', { className: `dot${i === 0 ? ' active' : ''}`, type: 'button', ariaLabel: `Slide ${i + 1}` });
      dot.addEventListener('click', () => {
        currentIndex = i;
        setActiveSlide(slidesContainer, currentIndex);
        updateDots(slidesContainer, currentIndex);
        resetTimer();
      });
      dotsContainer.appendChild(dot);
    }
    slidesContainer.appendChild(dotsContainer);

    slideTimer = setInterval(() => {
      currentIndex = (currentIndex + 1) % heroItems.length;
      setActiveSlide(slidesContainer, currentIndex);
      updateDots(slidesContainer, currentIndex);
    }, slideIntervalMs);
  }
}

function resetTimer() {
  if (slideTimer) clearInterval(slideTimer);
  if (heroItems.length > 1 && heroRoot) {
    const slidesContainer = heroRoot.querySelector('#slides-container');
    if (slidesContainer) {
      slideTimer = setInterval(() => {
        currentIndex = (currentIndex + 1) % heroItems.length;
        setActiveSlide(slidesContainer, currentIndex);
        updateDots(slidesContainer, currentIndex);
      }, slideIntervalMs);
    }
  }
}

export function destroyHero() {
  if (slideTimer) {
    clearInterval(slideTimer);
    slideTimer = null;
  }
  heroItems = [];
  currentIndex = 0;
  heroRoot = null;
}
