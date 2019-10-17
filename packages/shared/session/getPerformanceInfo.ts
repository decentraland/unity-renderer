export function getPerformanceInfo(samples: string) {
  const entries: number[] = []
  const length = samples.length
  let sum = 0
  for (let i = 0; i < length; i++) {
    entries[i] = samples.charCodeAt(i)
    sum += entries[i]
  }
  entries.sort()

  return {
    fps: length / sum,
    avg: sum / length,
    total: sum,
    len: length,
    min: entries[0],
    p50: entries[Math.ceil(length * 0.5)],
    p75: entries[Math.ceil(length * 0.75)],
    p90: entries[Math.ceil(length * 0.9)],
    p95: entries[Math.ceil(length * 0.95)],
    p99: entries[Math.ceil(length * 0.99)],
    max: entries[length - 1]
  }
}
