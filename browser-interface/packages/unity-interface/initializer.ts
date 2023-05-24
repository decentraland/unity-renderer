// This file contains code that selects which renderer to use and loads it
import { storeCondition } from 'lib/redux/storeCondition'
import { initializeRenderer } from 'shared/renderer/actions'
import { CommonRendererOptions, loadUnity } from './loader'
import type { UnityGame } from 'unity-interface/loader'
import type { KernelOptions } from '@dcl/kernel-interface'

import { initializeUnityEditor } from './wsEditorAdapter'
import { traceDecoratorRendererOptions } from './trace'
import {
  BringDownClientAndShowError,
  ErrorContext,
  ReportFatalErrorWithUnityPayloadAsync,
  UserError
} from 'shared/loading/ReportFatalError'
import { store } from 'shared/store/isolatedStore'
import defaultLogger from 'lib/logger'
import { trackEvent } from 'shared/analytics/trackEvent'
import { browserInterface } from './BrowserInterface'
import { webTransport } from 'renderer-protocol/transports/webTransport'
import { Transport } from '@dcl/rpc'
import { isRendererInitialized } from 'shared/renderer/selectors'
import { ALLOW_SWIFT_SHADER } from 'config'

export type InitializeUnityResult = {
  container: HTMLElement
}

const rendererOptions: Partial<KernelOptions['rendererOptions']> = {}

const defaultOptions: CommonRendererOptions = traceDecoratorRendererOptions({
  onMessage(type: string, jsonEncodedMessage: string) {
    let parsedJson = null
    try {
      parsedJson = JSON.parse(jsonEncodedMessage)
    } catch (e: any) {
      // we log the whole message to gain visibility
      defaultLogger.error(e.message + ' messageFromEngine: ' + type + ' ' + jsonEncodedMessage)
      trackEvent('non_json_message_from_engine', { type, payload: jsonEncodedMessage })
      return
    }
    // this is outside of the try-catch to enable V8 path optimizations
    // keep the following line outside the `try`
    browserInterface.handleUnityMessage(type, parsedJson)
  }
})

async function loadInjectedUnityDelegate(
  container: HTMLElement
): Promise<{ renderer: UnityGame; transport: Transport }> {
  // inject unity loader
  const rootArtifactsUrl = rendererOptions.baseUrl || ''

  const { createWebRenderer } = await loadUnity(rootArtifactsUrl, defaultOptions)

  preventUnityKeyboardLock()

  const canvas = document.createElement('canvas')
  canvas.id = '#canvas'
  container.appendChild(canvas)

  const { originalUnity, engineStartedFuture } = await createWebRenderer(canvas)

  const ctx: WebGL2RenderingContext = (originalUnity.Module as any).ctx

  const debug_ext = ctx.getExtension('WEBGL_debug_renderer_info')
  if (debug_ext) {
    const renderer = ctx.getParameter(debug_ext.UNMASKED_RENDERER_WEBGL)
    if (renderer.indexOf('SwiftShader') >= 0 && !ALLOW_SWIFT_SHADER) {
      throw new UserError(
        'Your browser is using an emulated software renderer (SwiftShader). This prevents Decentraland from working. This is usually fixed by restarting the computer. In any case, we recommend you to use the Desktop Client instead for a better overall experience. You can find it in https://decentraland.org/download'
      )
    }
  }

  canvas.addEventListener(
    'webglcontextlost',
    function (event) {
      event.preventDefault()
      BringDownClientAndShowError(
        'The rendering engine failed. This is an unrecoverable error that is subject to the available memory and resources of your browser.\n' +
          'For a better experience, we recommend using the Native Desktop Client. You can find it in https://decentraland.org/download'
      )
    },
    false
  )

  // TODO: move to unity-renderer js project
  originalUnity.Module.errorHandler = (message: string, filename: string, lineno: number) => {
    console['error'](message, filename, lineno)

    if (message.includes('The error you provided does not contain a stack trace')) {
      // This error is something that react causes only on development, with unhandled promises and strange errors with no stack trace (i.e, matrix errors).
      // Some libraries (i.e, matrix client) don't handle promises well and we shouldn't crash the explorer because of that
      return true
    }

    const error = new Error(`${message} ... file: ${filename} - lineno: ${lineno}`)
    ReportFatalErrorWithUnityPayloadAsync(error, ErrorContext.RENDERER_ERRORHANDLER)
    return true
  }

  const transport = webTransport({ wasmModule: originalUnity.Module }, (globalThis as any).DCL)

  await engineStartedFuture

  document.body.addEventListener('click', () => {
    browserInterface.onUserInteraction.resolve()
  })
  document.body.addEventListener('pointerdown', () => {
    browserInterface.onUserInteraction.resolve()
  })

  return { renderer: originalUnity, transport }
}

/** Initialize engine using WS transport (UnityEditor) */
async function loadWsEditorDelegate(container: HTMLElement) {
  const queryParams = new URLSearchParams(document.location.search)

  return initializeUnityEditor(queryParams.get('ws')!, container, defaultOptions)
}

/** Initialize the injected engine in a container */
export async function initializeUnity(options: KernelOptions['rendererOptions']): Promise<InitializeUnityResult> {
  const queryParams = new URLSearchParams(document.location.search)

  Object.assign(rendererOptions, options)
  const { container } = rendererOptions

  if (queryParams.has('ws')) {
    // load unity renderer using WebSocket
    store.dispatch(initializeRenderer(loadWsEditorDelegate, container))
    browserInterface.onUserInteraction.resolve()
  } else {
    // load injected renderer
    store.dispatch(initializeRenderer(loadInjectedUnityDelegate, container))
  }

  // wait until the renderer is fully loaded before returning, this
  // is important because once this function returns, it is assumed
  // that the renderer will be ready
  await storeCondition(isRendererInitialized)

  return {
    container
  }
}

/**
 * Prevent unity from locking the keyboard when there is an
 * active element (like delighted textarea)
 */
function preventUnityKeyboardLock() {
  const originalFunction = window.addEventListener
  window.addEventListener = function (event: any, handler: any, options?: any) {
    if (['keypress', 'keydown', 'keyup'].includes(event)) {
      originalFunction.call(
        window,
        event,
        (e) => {
          if (!document.activeElement || document.activeElement === document.body) {
            handler(e)
          }
        },
        options
      )
    } else {
      originalFunction.call(window, event, handler, options)
    }
    return true
  }
}
