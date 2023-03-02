import { Vector2 } from 'lib/math/Vector2'

export function getTilesRectFromCenter(parcelCoords: Vector2, rectSize: number): string[] {
  const result: string[] = []

  for (let x = parcelCoords.x - rectSize; x < parcelCoords.x + rectSize; x++) {
    for (let y = parcelCoords.y - rectSize; y < parcelCoords.y + rectSize; y++) {
      result.push(`${x},${y}`)
    }
  }

  return result
}
