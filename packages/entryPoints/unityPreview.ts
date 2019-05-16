// tslint:disable:no-console
declare var global: any
declare var window: any
declare var UnityLoader: UnityLoaderType

global['preview'] = window['preview'] = true
global['avoidWeb3'] = window['avoidWeb3']

import { initializeEngine } from '../unity-interface/dcl'
import { ETHEREUM_NETWORK, DEBUG, AVOID_WEB3, DEBUG_MESSAGES } from '../config'
const queryString = require('query-string')
const qs = queryString.parse(document.location.search)

type UnityLoaderType = {
  instantiate(divId: string, manifest: string): UnityGame
}

type UnityGame = {
  SendMessage(object: string, method: string, args: number | string): void
  SetFullscreen(): void
}

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

  ws.onmessage = function(ev) {
    if (DEBUG_MESSAGES) {
      console.log('>>>', ev.data)
    }

    try {
      const m = JSON.parse(ev.data)
      if (m.type && m.payload) {
        const payload = JSON.parse(m.payload)
        instancedJS!.then($ => $.onMessage(m.type, payload))
      } else {
        console.error('Dont know what to do with ', m)
      }
    } catch (e) {
      console.error(e)
    }
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

    instancedJS
      .then(({ net, loadPreviewScene }) => {
        global['handleServerMessage'] = function(message: any) {
          if (message.type === 'update') {
            loadPreviewScene()
              .then()
              .catch(console.error)
          }
        }

        // Warn in case wallet is set in mainnet
        if (net === ETHEREUM_NETWORK.MAINNET && DEBUG && !AVOID_WEB3) {
          const style = document.createElement('style')
          style.appendChild(
            document.createTextNode(
              `body:before{content:'You are using Mainnet Ethereum Network, real transactions are going to be made.';background:#ff0044;color:#fff;text-align:center;text-transform:uppercase;height:24px;width:100%;position:fixed;padding-top:2px}#main-canvas{padding-top:24px};`
            )
          )
          document.head.appendChild(style)
        }
      })
      .catch(error => {
        document.body.classList.remove('dcl-loading')
        document.body.innerHTML = `<h3>${error.message}</h3>`
      })
  }

  export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
    if (instancedJS) {
      instancedJS
        .then($ => $.onMessage(type, JSON.parse(jsonEncodedMessage)))
        .catch(() => {
          console.error('Message received without initializing engine', type, jsonEncodedMessage)
        })
    } else {
      console.error('Message received without initializing engine', type, jsonEncodedMessage)
    }
  }
}

global['DCL'] = DCL
