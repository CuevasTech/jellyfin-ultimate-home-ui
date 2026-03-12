/**
 * UHUI — Top Navigation Tabs
 * Renders configurable navigation tabs with active state management.
 */

const DEFAULT_TABS = [
  { tabId: 'home', label: 'Inicio', linkType: 'Home', order: 0, isVisible: true },
  { tabId: 'movies', label: 'Películas', linkType: 'Library', order: 1, isVisible: true },
  { tabId: 'series', label: 'Series', linkType: 'Library', order: 2, isVisible: true },
  { tabId: 'mylist', label: 'Mi Lista', linkType: 'MyList', order: 3, isVisible: true },
];

export function buildTabs(container, tabs) {
  container.innerHTML = '';

  const effectiveTabs = (tabs && tabs.length > 0) ? tabs : DEFAULT_TABS;
  const sorted = effectiveTabs
    .filter(function(t) { return t.isVisible !== false; })
    .sort(function(a, b) { return (a.order || 0) - (b.order || 0); });

  for (const tab of sorted) {
    const btn = document.createElement('button');
    btn.className = 'uhui-tabs__btn';
    btn.textContent = tab.label || tab.tabId;
    btn.dataset.tabId = tab.tabId;
    btn.dataset.linkType = tab.linkType || 'Home';
    btn.setAttribute('data-focusable', '');

    if (tab.tabId === 'home') {
      btn.classList.add('uhui-tabs__btn--active');
    }

    if (tab.targetId) {
      btn.dataset.targetId = tab.targetId;
    }

    if (tab.genreName) {
      btn.dataset.genreName = tab.genreName;
    }

    btn.addEventListener('click', function() {
      handleTabClick(container, tab);
    });

    container.appendChild(btn);
  }
}

function handleTabClick(container, tab) {
  container.querySelectorAll('.uhui-tabs__btn').forEach(function(b) {
    b.classList.remove('uhui-tabs__btn--active');
  });

  const clicked = container.querySelector('[data-tab-id="' + tab.tabId + '"]');
  if (clicked) clicked.classList.add('uhui-tabs__btn--active');

  switch (tab.linkType) {
    case 'Home':
      break;
    case 'Collection':
      if (tab.targetId) {
        window.location.href = '#!/item?id=' + tab.targetId + '&serverId=';
      }
      break;
    case 'Genre':
      if (tab.genreName) {
        window.location.href = '#!/list.html?genreId=' + encodeURIComponent(tab.genreName);
      }
      break;
    case 'Playlist':
      if (tab.targetId) {
        window.location.href = '#!/item?id=' + tab.targetId + '&serverId=';
      }
      break;
    case 'MyList':
      window.location.href = '#!/list.html?type=Favorite';
      break;
    case 'Library':
      if (tab.targetId) {
        window.location.href = '#!/item?id=' + tab.targetId + '&serverId=';
      }
      break;
    default:
      break;
  }
}
