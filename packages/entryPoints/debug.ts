// tslint:disable:no-console
import { initializeEngine } from '../unity/init'
import { UnityGame, UnityLoaderType } from '../unity/types'
const queryString = require('query-string')

declare var UnityLoader: UnityLoaderType
declare var global: any

const qs = queryString.parse(document.location.search)
let instancedJS: ReturnType<typeof initializeEngine> | null = null
let gameInstance: UnityGame | null = null

if (qs.ws) {
  console.info(`Connecting WS to ${qs.ws}`)
  document.body.innerHTML = `<h3>Connecting...</h3>`
  const ws = new WebSocket(qs.ws)

  ws.onclose = function(e) {
    console.error('WS closed!', e)
    document.body.innerHTML = `<h3 style='color:red'>Disconnected</h3>`
  }

  ws.onerror = function(e) {
    console.error('WS error!', e)
    document.body.innerHTML = `<h3 style='color:red'>EERRORR</h3>`
  }

  gameInstance = {
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
    document.body.classList.remove('dcl-loading')
    console.info('WS open!')
    gameInstance!.SendMessage('', 'Reset', '')
    document.body.innerHTML = `<h3  style='color:green'>Connected</h3>`
    DCL.EngineStarted()
  }
} else {
  gameInstance! = UnityLoader.instantiate('gameContainer', '/unity/Build/unity.json')
}

namespace DCL {
  export function EngineStarted() {
    instancedJS = initializeEngine(gameInstance!)

    instancedJS.catch(error => {
      document.body.classList.remove('dcl-loading')
      document.body.innerHTML = `<h3>${error.message}</h3>`
    })
  }

  export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
    if (instancedJS) {
      instancedJS.then($ => $.onMessage(type, JSON.parse(jsonEncodedMessage)))
    } else {
      // tslint:disable-next-line:no-console
      console.error('Message received without initializing engine', type, jsonEncodedMessage)
    }
  }
}

global['DCL'] = DCL
