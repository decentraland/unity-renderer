
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 60;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    window.requestAnimationFrame = function (rafCallback) {
      if (document.hidden) {
        backgroundRAF(rafCallback);
      } else {
        var rafId = __requestAnimationFrame(function(stamp) {
          rafCallback(stamp);
          delete __CURRENT_RAFS[rafId];
        });
        __CURRENT_RAFS[rafId] = rafCallback;
      }
    };

    function switchToBackground() {
      Object.keys(__CURRENT_RAFS).forEach(function(rafId) {
        window.cancelAnimationFrame(rafId);
        rafCallback = __CURRENT_RAFS[rafId];
        delete __CURRENT_RAFS[rafId];
        backgroundRAF(rafCallback);
      });
    }

    document.addEventListener("visibilitychange", function() {
      if(document.hidden) {
        switchToBackground();
      }
    })
  }
})();
