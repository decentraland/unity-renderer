import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { ExposableAPI } from './ExposableAPI'
import { defaultLogger } from '../logger'
import { DEBUG } from '../../config'

type SceneState = any

@registerAPI('SceneStateStorageController')
export class SceneStateStorageController extends ExposableAPI {

  @exposeMethod
  async storeState(sceneId: string, sceneState: SceneState): Promise<void> {
    if (DEBUG) {
      saveToLocalStorage(`scene-state-${sceneId}`, sceneState)
    } else {
      defaultLogger.error('Content server storage not yet supported')
    }
  }

  @exposeMethod
  async getStoredState(sceneId: string): Promise<SceneState> {
    if (DEBUG) {
      const sceneState = getFromLocalStorage(`scene-state-${sceneId}`)
      if (!sceneState) {
        defaultLogger.warn(`Couldn't find a stored scene state for scene ${sceneId}`)
        return TEST_SCENE
      }
      return sceneState
    } else {
      defaultLogger.error('Content server storage not yet supported')
    }
  }
}

// This is just here until the renderer integrates with scene state. Until then, it's easier to have a default scene
const TEST_SCENE = {
  entities: [
    {
      id: 'E1',
      components: [
        {
          type: "Transform",
          value: {
            position: {
              x: 8,
              y: 0,
              z: 8
            }
          }
        },
        {
          type: "GLTFShape",
          value: {
            src: 'models/BlockDog.glb'
          }
        }
      ]
    }
  ]
}