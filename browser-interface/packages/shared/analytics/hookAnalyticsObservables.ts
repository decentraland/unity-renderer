import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { worldToGrid } from 'lib/decentraland/parcels/worldToGrid'
import { trackEvent } from './trackEvent'
import { positionObservable } from 'shared/world/positionThings'

export function hookAnalyticsObservables() {
  let lastTime: number = performance.now()

  let previousPosition: string | null = null
  const gridPosition = { x: 0, y: 0, z: 0 }

  positionObservable.add(({ position }) => {
    // Update seconds variable and check if new parcel
    if (performance.now() - lastTime > 1000) {
      worldToGrid(position, gridPosition)
      const currentPosition = encodeParcelPosition(gridPosition)
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
