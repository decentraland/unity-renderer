import future from 'fp-future'
import { DEBUG_MESSAGES } from '../config'
import { initShared } from '../shared'
import { ReportFatalError } from '../shared/loading/ReportFatalError'
import { defaultLogger } from '../shared/logger'
import { initializeEngine } from './dcl'
import { Session } from '../shared/session'
import { waitingForRenderer } from '../shared/loading/types'
const queryString = require('query-string')

declare var global: any
declare var UnityLoader: UnityLoaderType

type UnityLoaderType = {
  // https://docs.unity3d.com/Manual/webgl-templates.html
  instantiate(divId: string | HTMLElement, manifest: string): UnityGame
}

type UnityGame = {
  SendMessage(object: string, method: string, args: number | string): void
  SetFullscreen(): void
}

/**
 * InstancedJS is the local instance of Decentraland
 */
let _instancedJS: ReturnType<typeof initializeEngine> | null = null

/**
 * UnityGame instance (Either Unity WebGL or Or Unity editor via WebSocket)
 */
let _gameInstance: UnityGame | null = null

export type InitializeUnityResult = {
  engine: UnityGame
  container: HTMLElement
  instancedJS: ReturnType<typeof initializeEngine>
}

const engineInitialized = future()

/** Initialize the engine in a container */
export async function initializeUnity(
  container: HTMLElement,
  buildConfigPath: string = 'unity/Build/unity.json'
): Promise<InitializeUnityResult> {
  const { essentials, all } = initShared()

  const session = await essentials

  if (!session) {
    throw new Error()
  }
  Session.current.resolve(session)
  const qs = queryString.parse(document.location.search)

  preventUnityKeyboardLock()

  if (qs.ws) {
    _gameInstance = initializeUnityEditor(qs.ws, container)
  } else {
    _gameInstance = UnityLoader.instantiate(container, buildConfigPath)
  }

  global['globalStore'].dispatch(waitingForRenderer())
  await all
  await engineInitialized

  return {
    engine: _gameInstance,
    container,
    instancedJS: _instancedJS!
  }
}

/**
 * Prevent unity from locking the keyboard when there is an
 * active element (like delighted textarea)
 */
function preventUnityKeyboardLock() {
  const originalFunction = window.addEventListener
  window.addEventListener = function(event: any, handler: any, options?: any) {
    if (['keypress', 'keydown', 'keyup'].includes(event)) {
      originalFunction.call(
        window,
        event,
        e => {
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

namespace DCL {
  // This function get's called by the engine
  export function EngineStarted() {
    if (!_gameInstance) throw new Error('There is no UnityGame')

    _instancedJS = initializeEngine(_gameInstance)

    _instancedJS
      .then($ => {
        // Expose the "kernel" interface as a global object to allow easier inspection
        global['browserInterface'] = $
        engineInitialized.resolve($)
      })
      .catch(error => {
        engineInitialized.reject(error)
        ReportFatalError('Unexpected fatal error')
      })
  }

  export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
    if (_instancedJS) {
      if (type === 'PerformanceReport') {
        _instancedJS.then($ => $.onMessage(type, jsonEncodedMessage)).catch(e => defaultLogger.error(e.message))
        return
      }
      _instancedJS
        .then($ => $.onMessage(type, JSON.parse(jsonEncodedMessage)))
        .catch(e => defaultLogger.error(e.message))
    } else {
      defaultLogger.error('Message received without initializing engine', type, jsonEncodedMessage)
    }
  }
}

// The namespace DCL is exposed to global because the unity template uses it to
// send the messages
global['DCL'] = DCL

/** This connects the local game to a native client via WebSocket */
function initializeUnityEditor(webSocketUrl: string, container: HTMLElement): UnityGame {
  defaultLogger.info(`Connecting WS to ${webSocketUrl}`)
  container.innerHTML = `<h3>Connecting...</h3>`
  const ws = new WebSocket(webSocketUrl)

  ws.onclose = function(e) {
    defaultLogger.error('WS closed!', e)
    container.innerHTML = `<h3 style='color:red'>Disconnected</h3>`
  }

  ws.onerror = function(e) {
    defaultLogger.error('WS error!', e)
    container.innerHTML = `<h3 style='color:red'>EERRORR</h3>`
  }

  ws.onmessage = function(ev) {
    if (DEBUG_MESSAGES) {
      defaultLogger.info('>>>', ev.data)
    }

    try {
      const m = JSON.parse(ev.data)
      if (m.type && m.payload) {
        const payload = m.type === 'PerformanceReport' ? m.payload : JSON.parse(m.payload)
        _instancedJS!.then($ => $.onMessage(m.type, payload)).catch(e => defaultLogger.error(e.message))
      } else {
        defaultLogger.error('Unexpected message: ', m)
      }
    } catch (e) {
      defaultLogger.error(e)
    }
  }

  const gameInstance: UnityGame = {
    SendMessage(_obj, type, payload) {
      if (ws.readyState === ws.OPEN) {
        const msg = JSON.stringify({ type, payload })
        ws.send(msg)
      }
    },
    SetFullscreen() {
      // stub
    }
  }

  ws.onopen = function() {
    container.classList.remove('dcl-loading')
    defaultLogger.info('WS open!')
    gameInstance.SendMessage('', 'Reset', '')
    container.innerHTML = `<h3  style='color:green'>Connected</h3>`
    DCL.EngineStarted()
  }

  return gameInstance
}
