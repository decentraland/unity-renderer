import { DEBUG_ANALYTICS } from 'config'
import { defaultLogger } from 'shared/logger'
import { globalObservable } from '../observables'
import { TrackEvents } from './types'

export type SegmentEvent = {
  name: string
  data: string
}

export function trackEvent<K extends keyof TrackEvents>(eventName: K, eventData: TrackEvents[K]) {
  if (DEBUG_ANALYTICS) {
    defaultLogger.info(`Tracking event "${eventName}": `, eventData)
  }

  globalObservable.emit('trackingEvent', {
    eventName,
    eventData
  })
}
