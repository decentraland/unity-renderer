import { Vector2Component } from 'atomicHelpers/landHelpers'

export function getTilesRectFromCenter(parcelCoords: Vector2Component, rectSize: number): string[] {
  let result: string[] = []

  for (let x = parcelCoords.x - rectSize; x < parcelCoords.x + rectSize; x++) {
    for (let y = parcelCoords.y - rectSize; y < parcelCoords.y + rectSize; y++) {
      result.push(`${x},${y}`)
    }
  }

  return result
}
