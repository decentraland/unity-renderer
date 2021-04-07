// This file decides and loads the renderer of choice

import { initShared } from 'shared'
import { USE_UNITY_INDEXED_DB_CACHE } from 'shared/meta/types'
import { initializeRenderer } from 'shared/renderer/actions'
import { StoreContainer } from 'shared/store/rootTypes'
import { ensureUnityInterface } from 'shared/renderer'
import { loadUnity, UnityGame } from './loader'

import { unityBuildConfigurations } from 'config'
import { initializeUnityEditor } from './wsEditorAdapter'
import future from 'fp-future'
import { ReportFatalError } from 'shared/loading/ReportFatalError'

declare const globalThis: StoreContainer & { Hls: any }
// HLS is required to make video texture and streaming work in Unity
globalThis.Hls = require('hls.js')

export type InitializeUnityResult = {
  container: HTMLElement
}

async function loadInjectedUnityDelegate(
  container: HTMLElement,
  onMessage: (type: string, payload: string) => void
): Promise<UnityGame> {
  const queryParams = new URLSearchParams(document.location.search)

  ;(window as any).USE_UNITY_INDEXED_DB_CACHE = USE_UNITY_INDEXED_DB_CACHE

  const engineStartedFuture = future<void>()

  // The namespace DCL is exposed to global because the unity template uses it to send the messages
  // @see https://github.com/decentraland/unity-renderer/blob/bc2bf1ee0d685132c85606055e592bac038b3471/unity-renderer/Assets/Plugins/JSFunctions.jslib#L6-L29
  ;(globalThis as any)['DCL'] = {
    // This function get's called by the engine
    EngineStarted() {
      engineStartedFuture.resolve()
    },

    // This function is called from the unity renderer to send messages back to the scenes
    MessageFromEngine(type: string, jsonEncodedMessage: string) {
      onMessage(type, jsonEncodedMessage)
    }
  }

  // inject unity loader
  const { baseUrl, createUnityInstance } = await loadUnity(queryParams.get('renderer') || undefined)

  preventUnityKeyboardLock()

  const config = {
    dataUrl: baseUrl + unityBuildConfigurations.UNITY_DATA_PATH,
    frameworkUrl: baseUrl + unityBuildConfigurations.UNITY_FRAMEWORK_PATH,
    codeUrl: baseUrl + unityBuildConfigurations.UNITY_CODE_PATH,
    streamingAssetsUrl: unityBuildConfigurations.UNITY_STREAMING_ASSETS_URL,
    companyName: unityBuildConfigurations.UNITY_ORGANIZATION_NAME,
    productName: unityBuildConfigurations.UNITY_PRODUCT_NAME,
    productVersion: unityBuildConfigurations.UNITY_PRODUCT_VERSION
  }

  const canvas = document.createElement('canvas')
  canvas.addEventListener('contextmenu', function (e) {
    e.preventDefault()
  })
  canvas.id = '#canvas'
  container.appendChild(canvas)

  const instanceFuture = createUnityInstance(canvas, config, function (_progress) {
    // In the future we could report progress of the loading: console.log('progress', _progress)
  })

  const instance = await instanceFuture

  instance.Module.errorHandler = (message: string, filename: string, lineno: number) => {
    console['error'](message, filename, lineno)

    if (message.includes('The error you provided does not contain a stack trace')) {
      // This error is something that react causes only on development, with unhandled promises and strange errors with no stack trace (i.e, matrix errors).
      // Some libraries (i.e, matrix client) don't handle promises well and we shouldn't crash the explorer because of that
      return true
    }

    ReportFatalError(message as any)
    return true
  }

  await engineStartedFuture

  return instanceFuture
}

/** Initialize engine using WS transport (UnityEditor) */
async function loadWsEditorDelegate(
  container: HTMLElement,
  onMessage: (type: string, payload: string) => void
): Promise<UnityGame> {
  const queryParams = new URLSearchParams(document.location.search)

  return initializeUnityEditor(queryParams.get('ws')!, container, onMessage)
}

/** Initialize the injected engine in a container */
export async function initializeUnity(container: HTMLElement): Promise<InitializeUnityResult> {
  const queryParams = new URLSearchParams(document.location.search)

  initShared()

  if (queryParams.has('ws')) {
    // load unity renderer using WebSocket
    globalThis.globalStore.dispatch(initializeRenderer(loadWsEditorDelegate, container))
  } else {
    // load injected renderer
    globalThis.globalStore.dispatch(initializeRenderer(loadInjectedUnityDelegate, container))
  }

  await ensureUnityInterface()

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
