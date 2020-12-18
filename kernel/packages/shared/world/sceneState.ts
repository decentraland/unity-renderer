import { WORLD_EXPLORER } from 'config'
import { Observable } from 'decentraland-ecs/src/ecs/Observable'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { ILand } from 'shared/types'
import { parcelObservable } from './positionThings'

export type SceneReport = {
  /** Scene where the user was */
  previousScene?: ILand
  /** Scene the user just entered */
  newScene: ILand
}

// Called each time the user changes scene
export const sceneObservable = new Observable<SceneReport>()
export let lastPlayerScene: ILand

// TODO: fetchSceneIds and fetchSceneJson don't work on preview mode, so we are disabling this for now. We need to figure out a way to make those queries in a way that they work in preview mode also
if (WORLD_EXPLORER) {
  // Listen to parcel changes, and notify if the scene changed
  parcelObservable.add(async ({ newParcel }) => {
    const parcelString = `${newParcel.x},${newParcel.y}`
    if (!lastPlayerScene || !lastPlayerScene.sceneJsonData.scene.parcels.includes(parcelString)) {
      const scenesId = await fetchSceneIds([parcelString])
      const sceneId = scenesId[0]
      if (sceneId) {
        const land = (await fetchSceneJson([sceneId]))[0]
        sceneObservable.notifyObservers({ previousScene: lastPlayerScene, newScene: land })
        lastPlayerScene = land
      }
    }
  })
}
