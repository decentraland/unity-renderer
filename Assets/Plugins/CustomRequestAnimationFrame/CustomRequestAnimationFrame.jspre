
(function () {
  if (window.requestAnimationFrame && document) {
    // limit or not FPS
    window.capFPS = false;
    
    // float precision when comparing times affects calculations, "target fps - 10" -> 40 -> 30FPS
    window.targetFPS = 40;

    // how much time (ms) should take to render a frame
    const FRAME_MS = 1000 / window.targetFPS;

    // store a backup of the original requestAnimationFrame just in case
    const originalRaf = window.__requestAnimationFrame = window.__requestAnimationFrame || window.requestAnimationFrame;
    
    // callbacks sent to requestAnimationFrame. The list is cleared once per frame
    var callbacks = [];
    var prevTime = 0;

    // keep track of the last created handler (raf or timeout) to reschedule a timeout when the document looses visibility
    var lastHandler = null;
    var lastHandlerWasRaf = false;

    // called every frame
    function tick(time) {
      lastHandler = null;

      if (!window.capFPS || time - prevTime >= FRAME_MS) {
        var oldCallbacks = callbacks;
        callbacks = [];

        for (var i = 0; i < oldCallbacks.length; i++) {
            oldCallbacks[i](time);
        }

        oldCallbacks.length = 0;
        prevTime = time;
      }

      scheduleNext();
    }
    
    function scheduleNext() {
      // if we had a scheduled tick, we cancel it and reschedule again
      if (lastHandler !== null) {
        if (lastHandlerWasRaf) {
          window.cancelAnimationFrame(lastHandler);
        } else {
          clearTimeout(lastHandler);
        }
        lastHandler = null;
      }

      // depending on the document visibility, we schedule a setTimeout or rAF
      if (document.hidden) {
        lastHandler = setTimeout(function() { tick(performance.now()); }, FRAME_MS);
        lastHandlerWasRaf = false;
      } else {
        lastHandler = originalRaf(tick);
        lastHandlerWasRaf = true;
      }
    }

    window.requestAnimationFrame = function (cb) {
      return callbacks.push(cb);
    };

    // if the document looses visibility, the render loop should keep working. but rAF doesn't
    // We reschedule the next frame, with a setTimeout this time.
    document.addEventListener("visibilitychange", function() {
      if (document.hidden) {
        scheduleNext();
      }
    })

    scheduleNext();
  }
})();
