export function sleep(time: number) {
  return new Promise<null>((resolve) => {
    setTimeout(resolve, time)
  })
}
