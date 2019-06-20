// tslint:disable:no-console
import { ETHEREUM_NETWORK, DEBUG, AVOID_WEB3 } from 'config'
import { initializePreview } from './initializePreview'
import { handleError } from '../error'
import { UnityGame } from '../types'

declare var global: any & {
  handleServerMessage: (message: any) => void
}

let instancedJS: ReturnType<typeof initializePreview> | null = null
let gameInstance: UnityGame | null = null

export function setGameInstance(_gameInstance: UnityGame): void {
  gameInstance = _gameInstance
}

/**
 * Function executed by Unity DCL Engine when it's ready to start exchanging messages
 * with the Explorer
 */
export function EngineStarted() {
  if (gameInstance) {
    instancedJS = initializePreview(gameInstance)
    instancedJS.catch(handleError)
  }
}

export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
  if (!instancedJS) {
    console.error('Message received without initializing engine', type, jsonEncodedMessage)
    return
  }

  instancedJS
    .then(({ net, loadPreviewScene }) => {
      // this is set to avoid double loading scenes due queued messages
      let currentlyLoadingScene: Promise<any> | null = null

      global['handleServerMessage'] = function(message: any) {
        if (message.type === 'update') {
          // if a scene is currently loading we do not trigger another load
          if (currentlyLoadingScene) return

          currentlyLoadingScene = loadPreviewScene()

          currentlyLoadingScene
            .then(() => {
              currentlyLoadingScene = null
            })
            .catch(err => {
              currentlyLoadingScene = null
              console.error('Error loading scene')
              console.error(err)
            })
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
    .catch(handleError)
}
