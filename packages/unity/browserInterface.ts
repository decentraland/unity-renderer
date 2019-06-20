import { playerConfigurations } from 'config'
import { ReadOnlyVector3, ReadOnlyQuaternion, Vector3, Quaternion } from 'decentraland-ecs/src/decentraland/math'
import { IEventNames } from 'decentraland-ecs/src/decentraland/Types'
import { positionObservable } from 'shared/world/positionThings'
import { getSceneWorkerByBaseCoordinates } from 'shared/world/parcelSceneManager'
import { ParcelSceneAPI } from 'shared/world/SceneWorker'

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height
}

const preloadedScenes = new Set<string>()

/**
 * This object is the one used by Unity to send messages to the browser.
 * Methods are being executed by `DCL` class with the `MessageFromEngine` method
 */
export default {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height
    positionObservable.notifyObservers(positionEvent)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerByBaseCoordinates(data.sceneId)

    if (scene) {
      const parcelScene = scene.parcelScene as (ParcelSceneAPI & { emit: (type: any, data: any) => void })
      parcelScene.emit(data.eventType as IEventNames, data.payload)
    }
  },

  PreloadFinished(data: { sceneId: string }) {
    preloadedScenes.add(data.sceneId)
  }
}
