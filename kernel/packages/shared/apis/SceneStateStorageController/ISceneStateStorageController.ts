import { SerializedSceneState, DeploymentResult } from './types'

export interface ISceneStateStorageController {
  storeState(sceneId: string, sceneState: SerializedSceneState): Promise<DeploymentResult>
  getStoredState(sceneId: string): Promise<SerializedSceneState | undefined>
}
