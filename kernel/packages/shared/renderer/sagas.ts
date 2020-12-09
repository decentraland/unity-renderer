import { call, put, select, take, takeEvery } from 'redux-saga/effects'

import { DEBUG_MESSAGES } from 'config'
import { initializeEngine, setLoadingScreenVisible } from 'unity-interface/dcl'

import { waitingForRenderer, UNEXPECTED_ERROR } from 'shared/loading/types'
import { createLogger } from 'shared/logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { StoreContainer } from 'shared/store/rootTypes'

import { UnityLoaderType, UnityGame, RENDERER_INITIALIZED } from './types'
import {
  INITIALIZE_RENDERER,
  InitializeRenderer,
  engineStarted,
  ENGINE_STARTED,
  messageFromEngine,
  MessageFromEngineAction,
  MESSAGE_FROM_ENGINE,
  rendererEnabled
} from './actions'
import { isInitialized } from './selectors'

const queryString = require('query-string')

declare const globalThis: StoreContainer
declare const UnityLoader: UnityLoaderType
declare const global: any

const DEBUG = false
const logger = createLogger('renderer: ')

let _gameInstance: UnityGame | null = null

export function* rendererSaga() {
  let _instancedJS: ReturnType<typeof initializeEngine> | null = null
  yield takeEvery(MESSAGE_FROM_ENGINE, (action: MessageFromEngineAction) =>
    handleMessageFromEngine(_instancedJS, action)
  )

  const action: InitializeRenderer = yield take(INITIALIZE_RENDERER)
  const _gameInstance = yield call(initializeRenderer, action)

  yield take(ENGINE_STARTED)
  _instancedJS = yield call(wrapEngineInstance, _gameInstance)
}

export function* ensureRenderer() {
  while (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
}

function* initializeRenderer(action: InitializeRenderer) {
  const { container, buildConfigPath } = action.payload

  const qs = queryString.parse(document.location.search)

  preventUnityKeyboardLock()

  setLoadingScreenVisible(true)

  if (qs.ws) {
    _gameInstance = initializeUnityEditor(qs.ws, container)
  } else {
    _gameInstance = UnityLoader.instantiate(container, buildConfigPath)
  }

  yield put(waitingForRenderer())

  return _gameInstance
}

function* wrapEngineInstance(_gameInstance: UnityGame) {
  if (!_gameInstance) {
    throw new Error('There is no UnityGame')
  }

  const _instancedJS: ReturnType<typeof initializeEngine> = initializeEngine(_gameInstance)

  _instancedJS
    .then(($) => {
      globalThis.globalStore.dispatch(rendererEnabled(_instancedJS))
    })
    .catch((error) => {
      logger.error(error)
      ReportFatalError(UNEXPECTED_ERROR)
    })

  return _instancedJS
}

function* handleMessageFromEngine(
  _instancedJS: ReturnType<typeof initializeEngine> | null,
  action: MessageFromEngineAction
) {
  const { type, jsonEncodedMessage } = action.payload
  DEBUG && logger.info(`handleMessageFromEngine`, action.payload)
  if (_instancedJS) {
    _instancedJS
      .then(($) => {
        let parsedJson = null
        try {
          parsedJson = JSON.parse(jsonEncodedMessage)
        } catch (e) {
          // we log the whole message to gain visibility
          logger.error(e.message + ' messageFromEngine:' + JSON.stringify(action))
          throw e
        }
        $.onMessage(type, parsedJson)
      })
      .catch((e) => logger.error(e.message))
  } else {
    logger.error('Message received without initializing engine', type, jsonEncodedMessage)
  }
}

export namespace DCL {
  // This exposes JSEvents emscripten's object
  export let JSEvents: any

  // This function get's called by the engine
  export function EngineStarted() {
    globalThis.globalStore.dispatch(engineStarted())
  }

  export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
    globalThis.globalStore.dispatch(messageFromEngine(type, jsonEncodedMessage))
  }
}

// The namespace DCL is exposed to global because the unity template uses it to
// send the messages
global['DCL'] = DCL
;(window as any)['DCL'] = DCL

/** This connects the local game to a native client via WebSocket */
function initializeUnityEditor(webSocketUrl: string, container: HTMLElement): UnityGame {
  logger.info(`Connecting WS to ${webSocketUrl}`)
  container.innerHTML = `<h3>Connecting...</h3>`
  const ws = new WebSocket(webSocketUrl)

  ws.onclose = function (e) {
    logger.error('WS closed!', e)
    container.innerHTML = `<h3 style='color:red'>Disconnected</h3>`
  }

  ws.onerror = function (e) {
    logger.error('WS error!', e)
    container.innerHTML = `<h3 style='color:red'>EERRORR</h3>`
  }

  ws.onmessage = function (ev) {
    if (DEBUG_MESSAGES) {
      logger.info('>>>', ev.data)
    }

    try {
      const m = JSON.parse(ev.data)
      if (m.type && m.payload) {
        globalThis.globalStore.dispatch(messageFromEngine(m.type, m.payload))
      } else {
        logger.error('Unexpected message: ', m)
      }
    } catch (e) {
      logger.error(e)
    }
  }

  const gameInstance: UnityGame = {
    Module: null,
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

  ws.onopen = function () {
    container.classList.remove('dcl-loading')
    logger.info('WS open!')
    gameInstance.SendMessage('', 'Reset', '')
    container.innerHTML = `<h3  style='color:green'>Connected</h3>`
    DCL.EngineStarted()
  }

  return gameInstance
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
