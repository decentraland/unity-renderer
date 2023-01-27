export function sleep(time: number) {
  return new Promise<null>((resolve) => {
    setTimeout(resolve, time)
  })
}

export function untilNextFrame() {
  return new Promise<number>((resolve) => {
    requestAnimationFrame(resolve)
  })
}
