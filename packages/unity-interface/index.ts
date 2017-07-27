import { initializeEngine } from './dcl'

type UnityLoaderType = {
  instantiate(divId: string, manifest: string): UnityGame
}

type UnityGame = {
  SendMessage(object: string, method: string, ...args: (number | string)[])
  SetFullscreen()
}

declare var UnityLoader: UnityLoaderType

const gameInstance = UnityLoader.instantiate('gameContainer', '/unity/Build/unity.json')

let instancedJS: ReturnType<typeof initializeEngine> | null = null

export namespace DCL {
  export function EngineStarted() {
    instancedJS = initializeEngine(gameInstance)

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
