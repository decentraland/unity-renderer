/** squared trigonometric distance */
function distanceSquared(x1: number, y1: number, x2: number, y2: number) {
  let dx = x2 - x1
  let dy = y2 - y1
  return dx * dx + dy * dy
}

function sanitizeMinusZero(num: number) {
  // THANKS JS, REALLY, THANKS
  return num | 0
}

export function getParcelsInRadius(centerX: number, centerZ: number, radius: number) {
  const list: Map<string, number> = new Map()

  const _margin = Math.max(Math.round(radius) + 1, 1)
  const _marginSquared = _margin * _margin

  for (let x = -_margin; x < _margin; x++) {
    for (let z = -_margin; z < _margin; z++) {
      const dist = distanceSquared(centerX + x, centerZ + z, centerX, centerZ)
      if (dist <= _marginSquared) {
        list.set(`${sanitizeMinusZero(centerX + x)},${sanitizeMinusZero(centerZ + z)}`, dist)
      }
    }
  }

  return list
}
