import { action } from 'typesafe-actions'
import {
  ExecutionLifecycleEvent,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  NOT_INVITED,
  NO_WEBGL_COULD_BE_CREATED,
  MOBILE_NOT_SUPPORTED,
  NEW_LOGIN,
  NETWORK_MISMATCH
} from './types'
import { StoreContainer } from 'shared/store/rootTypes'

declare const globalThis: StoreContainer

export let aborted = false

export function bringDownClientAndShowError(event: ExecutionLifecycleEvent) {
  if (aborted) {
    return
  }
  const body = document.body
  const container = document.getElementById('gameContainer')
  container!.setAttribute('style', 'display: none !important')
  const progressBar = document.getElementById('progress-bar')
  progressBar!.setAttribute('style', 'display: none !important')

  body.setAttribute('style', 'background-image: none !important;')

  const targetError =
    event === COMMS_COULD_NOT_BE_ESTABLISHED
      ? 'comms'
      : event === NOT_INVITED
      ? 'notinvited'
      : event === NO_WEBGL_COULD_BE_CREATED
      ? 'notsupported'
      : event === MOBILE_NOT_SUPPORTED
      ? 'nomobile'
      : event === NEW_LOGIN
      ? 'newlogin'
      : event === NETWORK_MISMATCH
      ? 'networkmismatch'
      : 'fatal'

  document.getElementById('error-' + targetError)!.setAttribute('style', 'display: block !important')
  aborted = true
}

export function ReportFatalError(event: ExecutionLifecycleEvent) {
  bringDownClientAndShowError(event)
  globalThis.globalStore && globalThis.globalStore.dispatch(action(event))
}
