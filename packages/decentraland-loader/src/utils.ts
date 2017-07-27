/** trigonometric distance */
function distance(x1: number, y1: number, x2: number, y2: number) {
  let dx = x2 - x1
  let dy = y2 - y1
  return Math.sqrt(dx * dx + dy * dy)
}

function sanitizeMinusZero(num: number) {
  // THANKS JS, REALLY, THANKS
  return num | 0
}

export function getParcelsInRadius(centerX: number, centerZ: number, radius: number) {
  const list: Map<string, number> = new Map()

  const _margin = Math.max(Math.round(radius) + 1, 1)

  for (let x = -_margin; x < _margin; x++) {
    for (let z = -_margin; z < _margin; z++) {
      const dist = distance(centerX + x, centerZ + z, centerX, centerZ)
      if (dist <= _margin) {
        list.set(`${sanitizeMinusZero(centerX + x)},${sanitizeMinusZero(centerZ + z)}`, dist)
      }
    }
  }

  return list
}
