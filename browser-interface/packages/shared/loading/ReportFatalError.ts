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
import { stringify } from 'lib/javascript/stringify'

export function BringDownClientAndShowError(event: ExecutionLifecycleEvent | string) {
  if (ExecutionLifecycleEventsList.includes(event as any)) {
    store.dispatch(action(event))
  }

  const targetError =
    event === COMMS_COULD_NOT_BE_ESTABLISHED ? 'comms' : event === NETWORK_MISMATCH ? 'networkmismatch' : 'fatal'

  store.dispatch(fatalError(targetError))

  globalObservable.emit('error', {
    error: new Error(event),
    code: targetError,
    level: 'fatal'
  })
}

export namespace ErrorContext {
  export const WEBSITE_INIT = `website#init`
  export const COMMS_INIT = `comms#init`
  export const COMMS = `comms`
  export const KERNEL_INIT = `kernel#init`
  export const KERNEL_SAGA = `kernel#saga`
  export const KERNEL_SCENE = `kernel#scene`
  export const KERNEL_WARNING = `kernel#warning`
  export const RENDERER_AVATARS = `renderer#avatars`
  export const RENDERER_ERRORHANDLER = `renderer#errorHandler`
  export const RENDERER_NEWERRORHANDLER = `renderer#newErrorHandler`
}

export type ErrorContextTypes =
  | typeof ErrorContext.WEBSITE_INIT
  | typeof ErrorContext.COMMS_INIT
  | typeof ErrorContext.COMMS
  | typeof ErrorContext.KERNEL_INIT
  | typeof ErrorContext.KERNEL_SAGA
  | typeof ErrorContext.KERNEL_SCENE
  | typeof ErrorContext.KERNEL_WARNING
  | typeof ErrorContext.RENDERER_AVATARS
  | typeof ErrorContext.RENDERER_ERRORHANDLER
  | typeof ErrorContext.RENDERER_NEWERRORHANDLER

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
    message: error.message || turnToString(error),
    stack: getStack(error).slice(0, 10000),
    saga_stack: sagaStack
  })

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

/**
 * Turns `arg` into a string.
 *
 * @param arg
 */
function turnToString(arg: any) {
  // Uses JSON.stringify if `typeof arg === 'object'`, but `''+arg` for native types for performance
  if (typeof arg === 'object') return stringify(arg)
  return '' + arg
}
