import {
  COMMS_COULD_NOT_BE_ESTABLISHED,
  fatalError,
  ExecutionLifecycleEvent,
  NETWORK_MISMATCH,
  ExecutionLifecycleEventsList
} from './types'
import { trackEvent } from 'shared/analytics/trackEvent'
import { action } from 'typesafe-actions'
import { globalObservable } from '../observables'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { store } from 'shared/store/isolatedStore'
import defaultLogger from 'lib/logger'
import { setRealmAdapter } from 'shared/realm/actions'
import { setRoomConnection } from 'shared/comms/actions'

export class UserError extends Error {
  constructor(msg: string) {
      super(msg);
  }
}

export function BringDownClientAndShowError(event: ExecutionLifecycleEvent | string) {
  if (ExecutionLifecycleEventsList.includes(event as any)) {
    store.dispatch(action(event))
  }

  const targetError =
    event === COMMS_COULD_NOT_BE_ESTABLISHED ? 'comms' : event === NETWORK_MISMATCH ? 'networkmismatch' : 'fatal'

  store.dispatch(fatalError(targetError))
  store.dispatch(setRealmAdapter(undefined))
  store.dispatch(setRoomConnection(undefined))

  globalObservable.emit('error', {
    error: new Error(event),
    code: targetError,
    level: 'fatal'
  })
}

export namespace ErrorContext {
  export const WEBSITE_INIT = `website#init`
  export const COMMS_INIT = `comms#init`
  export const KERNEL_INIT = `kernel#init`
  export const KERNEL_SAGA = `kernel#saga`
  export const KERNEL_SCENE = `kernel#scene`
  export const RENDERER_AVATARS = `renderer#avatars`
  export const RENDERER_ERRORHANDLER = `renderer#errorHandler`
  export const RENDERER_NEWERRORHANDLER = `renderer#newErrorHandler`
}

export type ErrorContextTypes =
  | typeof ErrorContext.WEBSITE_INIT
  | typeof ErrorContext.COMMS_INIT
  | typeof ErrorContext.KERNEL_INIT
  | typeof ErrorContext.KERNEL_SAGA
  | typeof ErrorContext.KERNEL_SCENE
  | typeof ErrorContext.RENDERER_AVATARS
  | typeof ErrorContext.RENDERER_ERRORHANDLER
  | typeof ErrorContext.RENDERER_NEWERRORHANDLER

export function ReportFatalErrorWithCatalystPayload(error: Error, context: ErrorContextTypes) {
  // TODO(Brian): Get some useful catalyst payload to append here
  BringDownClientAndReportFatalError(error, context)
}

export function ReportFatalErrorWithCommsPayload(error: Error, context: ErrorContextTypes) {
  // TODO(Brian): Get some useful comms payload to append here
  BringDownClientAndReportFatalError(error, context)
}

Object.assign(globalThis, {
  BringDownClientAndShowError,
  ReportFatalErrorWithUnityPayloadAsync,
  ReportFatalError: BringDownClientAndReportFatalError,
  BringDownClientAndReportFatalError
})

export function ReportFatalErrorWithUnityPayloadAsync(error: Error, context: ErrorContextTypes) {
  getUnityInstance()
    .CrashPayloadRequest()
    .then((payload) => {
      defaultLogger.error(payload)
      BringDownClientAndReportFatalError(error, context, { rendererPayload: payload })
    })
    .catch((err) => {
      defaultLogger.error(err)
      BringDownClientAndReportFatalError(error, context, { rendererPayload: err })
    })
}

export function BringDownClientAndReportFatalError(
  error: Error,
  context: ErrorContextTypes,
  payload: Record<string, any> = {}
) {
  let sagaStack: string | undefined = payload['sagaStack']

  debugger

  if (sagaStack) {
    // first stringify
    sagaStack = '' + sagaStack
    // then crop
    sagaStack = sagaStack.slice(0, 10000)
  }

  // segment requires less information than rollbar
  trackEvent('error_fatal', {
    context,
    // this is on purpose, if error is not an actual Error, it has no message, so we use the ''+error to call a
    // toString, we do that because it may be also null. and (null).toString() is invalid, but ''+null works perfectly
    message: error.message || '' + error,
    stack: getStack(error).slice(0, 10000),
    saga_stack: sagaStack
  })

  store.dispatch(fatalError(error.message || 'fatal error'))
  store.dispatch(setRealmAdapter(undefined))
  store.dispatch(setRoomConnection(undefined))

  globalObservable.emit('error', {
    error,
    level: 'fatal',
    extra: { context, ...payload }
  })
}

function getStack(error?: any) {
  if (error && error.stack) {
    return error.stack
  } else {
    try {
      throw new Error((error && error.message) || error || '<nullish error>')
    } catch (e: any) {
      return e.stack || '' + error
    }
  }
}
