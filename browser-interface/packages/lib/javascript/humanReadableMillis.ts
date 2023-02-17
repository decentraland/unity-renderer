export function humanReadableMillis(millis: number) {
  return (millis / 1000).toFixed(3) + 's'
}
