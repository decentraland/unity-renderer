export function isWebGLCompatible() {
  // Create canvas element. The canvas is not added to the document itself, so it is never displayed in the browser window.
  const canvas = <HTMLCanvasElement>document.createElement('canvas')
  const gl = canvas.getContext('webgl2')
  return gl && gl instanceof WebGL2RenderingContext
}