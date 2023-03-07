/**
 * Prevent unity from locking the keyboard when there is an
 * active element (like delighted textarea)
 */
export function preventUnityKeyboardLock() {
  const originalFunction = window.addEventListener
  window.addEventListener = function (event: any, handler: any, options?: any) {
    if (['keypress', 'keydown', 'keyup'].includes(event)) {
      originalFunction.call(
        window,
        event,
        (e) => {
          if (!document.activeElement || document.activeElement === document.body) {
            handler(e)
          }
        },
        options
      )
    } else {
      originalFunction.call(window, event, handler, options)
    }
    return true
  }
}
