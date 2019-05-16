import { DEBUG_ANALYTICS } from 'config'

import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { Vector2, ReadOnlyVector3, Vector3 } from 'decentraland-ecs/src'

import { chatObservable, ChatEvent } from './comms/chat'
import { avatarMessageObservable } from './comms/peers'
import { positionObservable } from './world/positionThings'

declare var window: any

export type SegmentEvent = {
  name: string
  data: string
}

const trackingQueue: SegmentEvent[] = []
let tracking = false

export async function initialize(
  segmentKey: string,
  { id, name, email }: { id: string; name: string; email: string }
): Promise<void> {
  hookObservables()
  if (!window.analytics) {
    return
  }

  window.analytics.load(segmentKey)
  window.analytics.page()

  return window.analytics.identify(id, {
    name,
    email
  })
}

export function queueTrackingEvent(eventName: string, eventData: any) {
  if (DEBUG_ANALYTICS) {
    console['log'](`Tracking event "${eventName}": `, eventData)
  }

  if (!window.analytics) {
    return
  }

  trackingQueue.push({ name: eventName, data: eventData })
  if (!tracking) {
    startTracking()
  }
}

function startTracking() {
  if (trackingQueue.length > 0) {
    tracking = true
    track(trackingQueue.shift()!)
  }
}

function track({ name, data }: SegmentEvent) {
  window.analytics.track(name, data, {}, () => {
    if (trackingQueue.length === 0) {
      tracking = false
      return
    }
    track(trackingQueue.shift()!)
  })
}

function hookObservables() {
  chatObservable.add((event: any) => {
    if (event.type === ChatEvent.MESSAGE_RECEIVED) {
      queueTrackingEvent('Chat message received', { lenght: event.data.message.lenght })
    } else if (event.type === ChatEvent.MESSAGE_SENT) {
      queueTrackingEvent('Send chat message', { messageId: event.data.messageId, length: event.data.message.length })
    }
  })

  avatarMessageObservable.add(({ type, ...data }) => queueTrackingEvent(type, data))

  let lastTime: number = performance.now()
  let seconds = 0
  let distanceTraveled = 0

  let previousPosition: string | null = null
  const gridPosition = Vector2.Zero()
  let previousWorldPosition: ReadOnlyVector3 | null = null

  positionObservable.add(({ position }) => {
    if (previousWorldPosition === null) {
      previousWorldPosition = { x: position.x, y: position.y, z: position.z }
    }

    if (seconds === 10 || distanceTraveled > 50) {
      queueTrackingEvent('User Position', { position: position, distance: distanceTraveled })
      seconds = 0
      distanceTraveled = 0
      previousWorldPosition = { x: position.x, y: position.y, z: position.z }
    }

    // Update seconds variable and check if new parcel
    if (performance.now() - lastTime > 1000) {
      distanceTraveled = Vector3.Distance(previousWorldPosition, position)
      worldToGrid(position, gridPosition)
      const currentPosition = `${gridPosition.x | 0},${gridPosition.y | 0}`
      seconds++
      if (previousPosition !== currentPosition) {
        queueTrackingEvent('Move to Parcel', { newParcel: currentPosition, oldParcel: previousPosition })
        previousPosition = currentPosition
      }
      lastTime = performance.now()
    }
  })
}
