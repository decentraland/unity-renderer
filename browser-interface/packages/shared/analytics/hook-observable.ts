import { Vector2 } from '@dcl/ecs-math'

import { worldToGrid } from '../../atomicHelpers/parcelScenePositions'
import { avatarMessageObservable } from '../comms/peers'
import { AvatarMessageType } from '../comms/interface/types'
import { positionObservable } from '../world/positionThings'
import { trackEvent } from '.'
import { TrackEvents } from './types'

const TRACEABLE_AVATAR_EVENTS = [
  AvatarMessageType.USER_MUTED,
  AvatarMessageType.USER_UNMUTED,
  AvatarMessageType.USER_BLOCKED,
  AvatarMessageType.USER_UNBLOCKED
] as const

function toTrackingEvent<K extends keyof TrackEvents>(event: AvatarMessageType): K {
  return ('Control ' + event.toString()) as K
}

export function hookAnalyticsObservables() {
  avatarMessageObservable.add(({ type, ...data }) => {
    const event = TRACEABLE_AVATAR_EVENTS.find((a) => a === type)
    if (!event) {
      return
    }

    const controlEvent = toTrackingEvent(event)

    trackEvent(controlEvent, data)
  })

  let lastTime: number = performance.now()

  let previousPosition: string | null = null
  const gridPosition = Vector2.Zero()

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
