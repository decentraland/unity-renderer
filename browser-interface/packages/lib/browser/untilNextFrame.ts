export function untilNextFrame() {
  return new Promise<number>((resolve) => {
    requestAnimationFrame(resolve)
  })
}
