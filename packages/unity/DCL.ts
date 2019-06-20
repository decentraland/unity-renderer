import { initializeEngine } from './init'
import { UnityGame } from './types'
import { handleError } from './error'

let instancedJS: ReturnType<typeof initializeEngine> | null = null
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
    instancedJS = initializeEngine(gameInstance)
    instancedJS.catch(handleError)
  }
}

/**
 * Function executed by Unity DCL Engine when there is a new message for the Explorer to process
 * It parses the message and send it to `browserInterface` object
 */
export function MessageFromEngine(type: string, jsonEncodedMessage: string) {
  if (!instancedJS) {
    // tslint:disable-next-line: no-console
    console.error('Message received without initializing engine', type, jsonEncodedMessage)
    return
  }
  instancedJS.then($ => $.onMessage(type, JSON.parse(jsonEncodedMessage))).catch(handleError)
}
