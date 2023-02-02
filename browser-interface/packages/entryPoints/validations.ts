export function isWebGLCompatible() {
  // Create canvas element. The canvas is not added to the document itself, so it is never displayed in the browser window.
  const canvas = <HTMLCanvasElement>document.createElement('canvas')
  const gl = canvas.getContext('webgl2')
  return gl && gl instanceof WebGL2RenderingContext
}

export function isMobile() {
  if (/Mobi/i.test(navigator.userAgent) || /Android/i.test(navigator.userAgent)) {
    return true
  }

  if (/iPad|iPhone|iPod/.test(navigator.platform)) {
    return true
  }

  if (/Macintosh/i.test(navigator.userAgent) && navigator.maxTouchPoints && navigator.maxTouchPoints > 1) {
    // iPad pro
    return true
  }

  return false
}
