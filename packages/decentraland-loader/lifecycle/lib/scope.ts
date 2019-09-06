import { Vector2Component } from 'atomicHelpers/landHelpers'

export interface ParcelConfigurationOptions {
  lineOfSightRadius: number
  secureRadius: number
}

export function squareAndSum(a: number, b: number) {
  return a * a + b * b
}

const cachedDeltas: { [limit: number]: Vector2Component[] } = {}

export function parcelsInScope(lineOfSightRadius: number, position: Vector2Component): string[] {
  const result: string[] = []
  if (!cachedDeltas[lineOfSightRadius]) {
    cachedDeltas[lineOfSightRadius] = []
  }
  let length = cachedDeltas[lineOfSightRadius].length
  if (!length) {
    calculateCachedDeltas(lineOfSightRadius)
    length = cachedDeltas[lineOfSightRadius].length
  }
  for (let i = 0; i < length; i++) {
    result.push(
      `${position.x + cachedDeltas[lineOfSightRadius][i].x},${position.y + cachedDeltas[lineOfSightRadius][i].y}`
    )
  }
  return result
}

function calculateCachedDeltas(limit: number) {
  const squaredRadius = limit * limit
  for (let x = -limit; x <= limit; x++) {
    for (let y = -limit; y <= limit; y++) {
      if (x * x + y * y <= squaredRadius) {
        cachedDeltas[limit].push({ x, y })
      }
    }
  }
  cachedDeltas[limit].sort((a, b) => squareAndSum(a.x, a.y) - squareAndSum(b.x, b.y))
}
