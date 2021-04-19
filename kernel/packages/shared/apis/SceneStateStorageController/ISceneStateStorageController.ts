import { SerializedSceneState, DeploymentResult } from './types'

export interface ISceneStateStorageController {
  publishSceneState(sceneId: string, sceneState: SerializedSceneState): Promise<DeploymentResult>
  getStoredState(sceneId: string): Promise<SerializedSceneState | undefined>
  saveSceneState(serializedSceneState: SerializedSceneState) :  Promise<DeploymentResult>
  getProjectManifest(projectId: string): Promise<SerializedSceneState | undefined> 
  getProjectManifestByCoordinates(land: string): Promise<SerializedSceneState | undefined>
  createProjectWithCoords(coordinates: string): Promise<boolean>
}
