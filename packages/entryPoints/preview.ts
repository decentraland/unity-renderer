declare var global: any & {
  preview: boolean
  // tslint:disable-next-line: no-use-before-declare
  DCL: typeof DCL
}
declare var window: Window & {
  preview: boolean
}
declare var UnityLoader: UnityLoaderType

// Must be initialized before importing any module that uses PREVIEW config
global.preview = window.preview = true

import { UnityLoaderType } from '../unity/types'
import * as DCL from '../unity/preview'

const gameInstance = UnityLoader.instantiate('gameContainer', '/unity/Build/unity.json')

DCL.setGameInstance(gameInstance)
global.DCL = DCL
