/**
 * JellyMoods sidebar injector
 * Loaded globally by Jellyfin via config.json plugins array.
 * Inserts a "JellyMoods" nav item into the main sidebar
 * under the Music entry, on every page, for all users.
 */
(function () {
  'use strict';

  var ITEM_ID  = 'jellymoods-nav-item';
  var PAGE_URL = '/web/index.html#!/configurationpage?name=jellymoods';

  function buildItem() {
    var li = document.createElement('li');
    li.className = 'navMenuOption';

    var a = document.createElement('a');
    a.id        = ITEM_ID;
    a.href      = PAGE_URL;
    a.className = 'navMenuOption flex align-items-center';
    a.style.cssText = 'display:flex;align-items:center;padding:8px 20px 8px 18px;cursor:pointer;text-decoration:none;color:inherit;';
    a.setAttribute('data-itemid', 'jellymoods');

    a.innerHTML =
      '<span class="material-icons navMenuOptionIcon" style="margin-right:14px;font-size:1.4em;">music_note</span>' +
      '<span class="navMenuOptionText">JellyMoods</span>';

    a.addEventListener('click', function (e) {
      e.preventDefault();
      window.location.href = PAGE_URL;
    });

    li.appendChild(a);
    return li;
  }

  function inject() {
    if (document.getElementById(ITEM_ID)) return;

    // Jellyfin renders nav items in a <ul> or <div> inside the drawer
    // Try every selector used across Jellyfin versions
    var nav =
      document.querySelector('.navMenuOptions') ||
      document.querySelector('.mainDrawer-scrollContainer .navMenuOptions') ||
      document.querySelector('.mainDrawer .navMenuOptions') ||
      document.querySelector('[data-role="panel"] .navMenuOptions') ||
      document.querySelector('.mainDrawerContent .navMenuOptions');

    if (!nav) return;

    // Find the Music link to insert after it
    var musicLink =
      nav.querySelector('[data-itemid="music"]') ||
      Array.from(nav.querySelectorAll('a,[data-itemid]')).find(function (el) {
        return (el.getAttribute('data-itemid') || '').toLowerCase() === 'music' ||
               el.textContent.trim().toLowerCase() === 'music';
      });

    var item = buildItem();

    if (musicLink) {
      // Insert after Music's parent <li> or the link itself
      var target = musicLink.closest('li') || musicLink;
      target.parentNode.insertBefore(item, target.nextSibling);
    } else {
      // Fallback: append to nav
      nav.appendChild(item);
    }
  }

  // Run immediately in case DOM is ready
  inject();

  // MutationObserver survives SPA navigation — re-injects if nav re-renders
  var observer = new MutationObserver(function () { inject(); });
  observer.observe(document.body, { childList: true, subtree: true });

})();
