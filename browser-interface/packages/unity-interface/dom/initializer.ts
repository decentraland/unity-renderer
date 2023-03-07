// This file contains code that selects which renderer to use and loads it
import type { UnityGame } from '@dcl/unity-renderer/src/index'
import type { KernelOptions } from 'kernel-web-interface'
import { loadUnity } from '../loader'

import { Transport } from '@dcl/rpc'
import { ALLOW_SWIFT_SHADER } from 'config'
import { CommonRendererOptions } from 'kernel-web-interface/renderer'
import defaultLogger from 'lib/logger'
import { webTransport } from 'renderer-protocol/transports/webTransport'
import { traceDecoratorRendererOptions } from 'shared/analytics/trace'
import { trackEvent } from 'shared/analytics/trackEvent'
import {
  BringDownClientAndShowError,
  ErrorContext,
  ReportFatalErrorWithUnityPayloadAsync
} from 'shared/loading/ReportFatalError'
import { browserInterface } from '../BrowserInterface'
import { initializeUnityEditor } from '../wsEditorAdapter'
import { preventUnityKeyboardLock } from './preventUnityKeyboardLock'

export const rendererOptions: Partial<KernelOptions['rendererOptions']> = {}

export async function loadInjectedUnityDelegate(
  container: HTMLElement
): Promise<{ renderer: UnityGame; transport: Transport }> {
  // inject unity loader
  const rootArtifactsUrl = rendererOptions.baseUrl || ''

  const { createWebRenderer } = await loadUnity(rootArtifactsUrl, defaultOptions)
  preventUnityKeyboardLock()
  const canvas = createDOMCanvas(container)

  const { originalUnity, rendererInitializationFuture: engineStartedFuture } = await createWebRenderer(canvas)
  ensureNoSwiftShader(originalUnity)
  hookToCriticalWebGLErrors(canvas)
  addUnityEngineErrorHandler(originalUnity)

  const transport = webTransport({ wasmModule: originalUnity.Module }, (globalThis as any).DCL)

  await engineStartedFuture
  addPointerEventListeners()

  return { renderer: originalUnity, transport }
}

function addPointerEventListeners() {
  document.body.addEventListener('click', () => {
    browserInterface.onUserInteraction.resolve()
  })
  document.body.addEventListener('pointerdown', () => {
    browserInterface.onUserInteraction.resolve()
  })
}

function createDOMCanvas(container: HTMLElement) {
  const canvas = document.createElement('canvas')
  canvas.id = '#canvas'
  container.appendChild(canvas)
  return canvas
}

function addUnityEngineErrorHandler(originalUnity: UnityGame) {
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
}

function hookToCriticalWebGLErrors(canvas: HTMLCanvasElement) {
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
}

/** Initialize engine using WS transport (UnityEditor) */
export async function loadWsEditorDelegate(container: HTMLElement) {
  const queryParams = new URLSearchParams(document.location.search)

  return initializeUnityEditor(queryParams.get('ws')!, container, defaultOptions)
}

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

function ensureNoSwiftShader(originalUnity: UnityGame) {
  const ctx: WebGL2RenderingContext = (originalUnity.Module as any).ctx

  const debug_ext = ctx.getExtension('WEBGL_debug_renderer_info')
  if (debug_ext) {
    const renderer = ctx.getParameter(debug_ext.UNMASKED_RENDERER_WEBGL)
    if (renderer.includes('SwiftShader') && !ALLOW_SWIFT_SHADER) {
      throw new Error(
        'Your browser is using an emulated software renderer (SwiftShader). This prevents Decentraland from working. This is usually fixed by restarting the computer. In any case, we recommend you to use the Desktop Client instead for a better overall experience. You can find it in https://decentraland.org/download'
      )
    }
  }
}
