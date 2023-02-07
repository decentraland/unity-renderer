import { worldToGrid } from 'lib/decentraland/parcels/worldToGrid'
import { positionObservable } from '../world/positionThings'
import { trackEvent } from '.'

export function hookAnalyticsObservables() {
  let lastTime: number = performance.now()

  let previousPosition: string | null = null
  const gridPosition = { x: 0, y: 0, z: 0 }

  positionObservable.add(({ position }) => {
    // Update seconds variable and check if new parcel
    if (performance.now() - lastTime > 1000) {
      worldToGrid(position, gridPosition)
      const currentPosition = `${gridPosition.x | 0},${gridPosition.y | 0}`
      if (previousPosition !== currentPosition) {
        trackEvent('Move to Parcel', {
          newParcel: currentPosition,
          oldParcel: previousPosition,
          exactPosition: { x: position.x, y: position.y, z: position.z }
        })
        previousPosition = currentPosition
      }
      lastTime = performance.now()
    }
  })
}
