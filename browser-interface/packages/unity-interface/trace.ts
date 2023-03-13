import type { UnityGame } from 'unity-interface/loader'
import { TRACE_RENDERER } from 'config'
import {
  incrementMessageFromKernelToRenderer,
  incrementMessageFromRendererToKernel
} from 'shared/session/getPerformanceInfo'
import defaultLogger from 'lib/logger'
import type { CommonRendererOptions } from './loader'

let pendingMessagesInTrace = 0
const currentTrace: string[] = []
let traceType: 'console' | 'file' = 'file'

export function traceDecoratorRendererOptions(options: CommonRendererOptions): CommonRendererOptions {
  const originalOnMessage = options.onMessage

  return {
    ...options,
    onMessage(type, payload) {
      incrementMessageFromRendererToKernel()
      if (pendingMessagesInTrace > 0) {
        logTrace(type, payload, 'RK')
      }
      return originalOnMessage.call(options, type, payload)
    }
  }
}

export function traceDecoratorUnityGame(game: UnityGame): UnityGame {
  const originalSendMessage = game.SendMessage
  game.SendMessage = function (obj, method, args) {
    if (pendingMessagesInTrace > 0) {
      logTrace(`${obj}.${method}`, args, 'KR')
    }
    incrementMessageFromKernelToRenderer()
    return originalSendMessage.call(this, obj, method, args)
  }
  return game
}

export function beginTrace(messagesCount: number, download: boolean = true) {
  if (messagesCount > 0) {
    currentTrace.length = 0
    pendingMessagesInTrace = messagesCount
    traceType = download ? 'file' : 'console'
    defaultLogger.log('[TRACING] Beginning trace')
  }
}

/**
 * RK: Renderer->Kernel
 * KR: Kernel->Renderer
 * KK: Kernel->Kernel
 */
export function logTrace(type: string, payload: string | number | undefined, direction: 'RK' | 'KR' | 'KK') {
  if (pendingMessagesInTrace > 0) {
    const now = performance.now().toFixed(1)

    function trace(text: string) {
      if (traceType === 'file') {
        currentTrace.push(text)
      } else if (traceType === 'console') {
        console.log('[TRACE]', text)
      }
    }

    if (direction === 'KK') {
      try {
        trace(`${direction}\t${now}\t${JSON.stringify(type)}\t${JSON.stringify(payload)}`)
      } catch (e) {
        trace(`${direction}\t${now}\t${JSON.stringify(type)}\tCIRCULAR`)
      }
    } else {
      trace(
        `${direction}\t${now}\t${JSON.stringify(type)}\t${
          payload === undefined ? '' : payload.toString().replace(/\n/g, '\\n')
        }`
      )
    }
    pendingMessagesInTrace--
    if (pendingMessagesInTrace % 11 === 0) {
      defaultLogger.log('[TRACING] Pending messages to download: ' + pendingMessagesInTrace)
    }
    if (pendingMessagesInTrace === 0) {
      endTrace()
    }
  }
}

export function endTrace() {
  pendingMessagesInTrace = 0
  const content = currentTrace.join('\n')
  const file = new File([content], 'decentraland-trace.csv', { type: 'text/csv' })
  const exportUrl = URL.createObjectURL(file)
  defaultLogger.log('[TRACING] Ending trace, downloading file: ', exportUrl, ' check your downloads folder.')
  window.location.assign(exportUrl)
  currentTrace.length = 0
}

;(globalThis as any).beginTrace = beginTrace
;(globalThis as any).endTrace = endTrace

const parametricTrace = parseInt(TRACE_RENDERER || '0', 10)
if (!isNaN(parametricTrace) && parametricTrace > 0) {
  beginTrace(parametricTrace)
}
