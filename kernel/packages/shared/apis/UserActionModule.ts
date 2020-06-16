import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { unityInterface } from 'unity-interface/dcl'
import { StoreContainer } from 'shared/store/rootTypes'
import defaultLogger from 'shared/logger'

declare const globalThis: StoreContainer

export interface IUserActionModule {
  requestTeleport(destination: string): Promise<void>
}

@registerAPI('UserActionModule')
export class UserActionModule extends ExposableAPI implements IUserActionModule {
  @exposeMethod
  async requestTeleport(destination: string): Promise<void> {
    if (destination === "magic" || destination === "crowd") {
      unityInterface.RequestTeleport({ destination })
      return
    } else if (!(/^\-?\d+\,\-?\d+$/).test(destination)) {
      defaultLogger.error(`teleportTo: invalid destination ${destination}`)
      return
    }

    let sceneThumbnailUrl: string = ""
    let sceneName: string = destination
    let sceneCreator: string = "Unknown"
    let sceneEvent = {}

    const mapSceneData = globalThis.globalStore.getState().atlas.tileToScene[destination]

    if (mapSceneData) {
      sceneName = mapSceneData.name
      if (mapSceneData.sceneJsonData?.contact?.name) {
        sceneCreator = mapSceneData.sceneJsonData.contact.name
      }
    }

    if (mapSceneData && mapSceneData.sceneJsonData?.display?.navmapThumbnail) {
      sceneThumbnailUrl = mapSceneData.sceneJsonData.display.navmapThumbnail
    } else {
      let sceneParcels = [destination]
      if (mapSceneData && mapSceneData.sceneJsonData?.scene.parcels) {
        sceneParcels = mapSceneData.sceneJsonData.scene.parcels
      }
      sceneThumbnailUrl = `https://api.decentraland.org/v1/map.png?width=480&height=237&size=10&center=${destination}&selected=${sceneParcels.join(";")}`
    }

    try {
      const response = await fetch(`https://events.decentraland.org/api/events/?position=${destination}`)
      const json = await response.json()
      if (json.data.length > 0) {
        sceneEvent = {
          name: json.data[0].name,
          total_attendees: json.data[0].total_attendees,
          start_at: json.data[0].start_at,
          finish_at: json.data[0].finish_at
        }
      }
    } catch (e) {
      defaultLogger.error(e)
    }

    unityInterface.RequestTeleport({
      destination,
      sceneEvent,
      sceneData: {
        name: sceneName,
        owner: sceneCreator,
        previewImageUrl: sceneThumbnailUrl
      }
    })
  }
}
