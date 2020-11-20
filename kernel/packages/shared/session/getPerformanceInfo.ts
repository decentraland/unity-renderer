export function getPerformanceInfo(data: { samples: string; fpsIsCapped: boolean }) {
  const entries: number[] = []
  const length = data.samples.length
  let sum = 0
  for (let i = 0; i < length; i++) {
    entries[i] = data.samples.charCodeAt(i)
    sum += entries[i]
  }
  const sorted = entries.sort((a, b) => a - b)

  return {
    idle: document.hidden,
    fps: (1000 * length) / sum,
    avg: sum / length,
    total: sum,
    len: length,
    min: sorted[0],
    p50: sorted[Math.ceil(length * 0.5)],
    p75: sorted[Math.ceil(length * 0.75)],
    p90: sorted[Math.ceil(length * 0.9)],
    p95: sorted[Math.ceil(length * 0.95)],
    p99: sorted[Math.ceil(length * 0.99)],
    max: sorted[length - 1],
    capped: data.fpsIsCapped
  }
}

export function getRawPerformanceInfo(data: { samples: string; fpsIsCapped: boolean }) {
  const entries: number[] = []

  const length = data.samples.length

  for (let i = 0; i < length; i++) {
    entries[i] = data.samples.charCodeAt(i)
  }

  return {
    values: entries,
    capped: data.fpsIsCapped
  }
}
