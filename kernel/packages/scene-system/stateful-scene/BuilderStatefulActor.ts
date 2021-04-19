import { SceneStateDefinition } from './SceneStateDefinition'
import { ILand } from 'shared/types'
import { deserializeSceneState } from './SceneStateDefinitionSerializer'
import { ISceneStateStorageController } from 'shared/apis/SceneStateStorageController/ISceneStateStorageController'

export class BuilderStatefulActor {
  constructor(
    protected readonly land: ILand,
    private readonly sceneStorage: ISceneStateStorageController
  ) {}

  async getInititalSceneState(): Promise<SceneStateDefinition> {
    const sceneState = await this.getContentLandDefinition()
    return sceneState ? sceneState : new SceneStateDefinition()
  }

  private async getContentLandDefinition(): Promise<SceneStateDefinition | undefined> {

    //First we search the definition in the builder server filtering by land coordinates
    const builderProjectByCoordinates = await this.sceneStorage.getProjectManifestByCoordinates(
      this.land.sceneJsonData.scene.base
    )
    if (builderProjectByCoordinates) {
      return deserializeSceneState(builderProjectByCoordinates)
    }

    //if there is no project associated to the land, we search the last builder project deployed in the land
    if (this.land.sceneJsonData.source?.projectId) {
      const builderProject = await this.sceneStorage.getProjectManifest(this.land.sceneJsonData.source?.projectId)
      if (builderProject) {
        return deserializeSceneState(builderProject)
      }
    }

    //If there is no builder project deployed in the land, we just create a new one
    await this.sceneStorage.createProjectWithCoords(this.land.sceneJsonData.scene.base)
    return new SceneStateDefinition()
  }
}
