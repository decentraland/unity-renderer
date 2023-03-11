import { DEBUG_ANALYTICS } from 'config'
import { defaultLogger } from 'lib/logger'
import { globalObservable } from '../observables'
import { TrackEvents } from './types'

export function trackEvent<K extends keyof TrackEvents>(eventName: K, eventData: TrackEvents[K]) {
  if (DEBUG_ANALYTICS) {
    defaultLogger.info(`Tracking event "${eventName}": `, eventData)
  }

  globalObservable.emit('trackingEvent', {
    eventName,
    eventData
  })
}

export function trackError(context: string, error?: Error, message?: string) {
  trackEvent('error', {
    context,
    message: message || error?.message || 'unknown',
    stack: error?.stack || '',
    saga_stack: error?.toString()
  })
}
