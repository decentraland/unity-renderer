export function isWebGLCompatible() {
  // Create canvas element. The canvas is not added to the document itself, so it is never displayed in the browser window.
  var canvas = <HTMLCanvasElement>document.createElement("canvas")
  var gl = canvas.getContext("webgl2")
  return gl && gl instanceof WebGL2RenderingContext
}

export function isMobile() {
  return /Mobi/i.test(navigator.userAgent) || /Android/i.test(navigator.userAgent)
}
