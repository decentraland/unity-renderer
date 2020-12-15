import { DEBUG_ANALYTICS, getTLD } from 'config'

import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { ReadOnlyVector3, Vector2, Vector3 } from 'decentraland-ecs/src'
import { defaultLogger } from 'shared/logger'

import { avatarMessageObservable } from './comms/peers'
import { AvatarMessageType } from './comms/interface/types'
import { positionObservable } from './world/positionThings'
import { uuid } from '../decentraland-ecs/src/ecs/helpers'
import { AnalyticsContainer } from './types'

declare const window: Window & AnalyticsContainer

export type SegmentEvent = {
  name: string
  data: string
}

const sessionId = uuid()

const trackingQueue: SegmentEvent[] = []
let tracking = false

enum AnalyticsAccount {
  PRD = '1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU',
  DEV = 'a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc'
}

// TODO fill with segment keys and integrate identity server
export function initializeAnalytics() {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      if (
        globalThis.location.host === 'play.decentraland.org' ||
        globalThis.location.host === 'explorer.decentraland.org'
      ) {
        return initialize(AnalyticsAccount.PRD)
      }
      return initialize(AnalyticsAccount.DEV)
    case 'today':
      return initialize(AnalyticsAccount.DEV)
    case 'zone':
      return initialize(AnalyticsAccount.DEV)
    default:
      return initialize(AnalyticsAccount.DEV)
  }
}

export async function initialize(segmentKey: string): Promise<void> {
  hookObservables()
  if (!window.analytics) {
    return
  }

  if (window.analytics.load) {
    // loading client for the first time
    window.analytics.load(segmentKey)
    window.analytics.page()
    window.analytics.ready(() => {
      window.analytics.timeout(10000)
    })
  }
}

function baseAnalyticsIdentifierContext() {
  return {
    explorer_commit_hash: (window as any)['VERSION']
  }
}

export function identifyUser(id: string) {
  if (window.analytics) {
    window.analytics.identify(id, baseAnalyticsIdentifierContext())
  }
}

export function identifyEmail(email: string, userId?: string) {
  if (userId) {
    window.analytics.identify(userId, { email, ...baseAnalyticsIdentifierContext() })
  } else {
    window.analytics.identify({ email, ...baseAnalyticsIdentifierContext() })
  }
}

export function queueTrackingEvent(eventName: string, eventData: any) {
  const data = { ...eventData, time: new Date().toISOString(), sessionId, version: (window as any)['VERSION'] }

  if (DEBUG_ANALYTICS) {
    defaultLogger.info(`Tracking event "${eventName}": `, data)
  }

  if (!window.analytics) {
    return
  }

  trackingQueue.push({ name: eventName, data })
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
  window.analytics.track(name, data, { integrations: { Klaviyo: false } }, () => {
    if (trackingQueue.length === 0) {
      tracking = false
      return
    }
    track(trackingQueue.shift()!)
  })
}

const TRACEABLE_AVATAR_EVENTS = [
  AvatarMessageType.ADD_FRIEND,
  AvatarMessageType.USER_DATA,
  AvatarMessageType.USER_EXPRESSION,
  AvatarMessageType.USER_REMOVED,
  AvatarMessageType.SET_LOCAL_UUID,
  AvatarMessageType.USER_MUTED,
  AvatarMessageType.USER_UNMUTED,
  AvatarMessageType.USER_BLOCKED,
  AvatarMessageType.USER_UNBLOCKED
]

function hookObservables() {
  avatarMessageObservable.add(({ type, ...data }) => {
    if (!TRACEABLE_AVATAR_EVENTS.includes(type)) {
      return
    }

    queueTrackingEvent(type, data)
  })

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
