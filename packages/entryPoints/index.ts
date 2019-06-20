import { UnityLoaderType } from '../unity/types'
import * as DCL from '../unity/DCL'

declare var global: any
declare var UnityLoader: UnityLoaderType

const gameInstance = UnityLoader.instantiate('gameContainer', '/unity/Build/unity.json')

DCL.setGameInstance(gameInstance)
global['DCL'] = DCL
