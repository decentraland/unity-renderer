import { UnityGame } from './types'
import { LoadableParcelScene } from '../shared/types'

/**
 * This object is the one that use Explorer to communicate with Unity
 */
export default class EngineInterface {
  private lastParcelScenesSent = ''

  constructor(private gameInstance: UnityGame, private debug: boolean = false) {}

  SetDebug() {
    this.gameInstance.SendMessage('SceneController', 'SetDebug')
  }
  CreateUIScene(data: { id: string; baseUrl: string }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    this.gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data))
  }
  /** Sends the camera position to the engine */
  SetPosition(x: number, y: number, z: number) {
    let theY = y <= 0 ? 2 : y

    this.gameInstance.SendMessage('CharacterController', 'SetPosition', JSON.stringify({ x, y: theY, z }))
  }
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    const parcelScenes = JSON.stringify({ parcelsToLoad })
    if (parcelScenes !== this.lastParcelScenesSent) {
      this.lastParcelScenesSent = parcelScenes
      let finalJson: string = ''

      // NOTE(Brian): split json to be able to throttle the json parsing process in engine's side
      for (let i = 0; i < parcelsToLoad.length; i++) {
        const parcel = parcelsToLoad[i]
        const json = JSON.stringify(parcel)

        finalJson += json

        if (i < parcelsToLoad.length - 1) {
          finalJson += '}{'
        }
      }

      this.gameInstance.SendMessage('SceneController', 'LoadParcelScenes', finalJson)
    }
  }
  sendSceneMessage(parcelSceneId: string, method: string, payload: string) {
    if (this.debug) {
      // tslint:disable-next-line:no-console
      console.log(parcelSceneId, method, payload)
    }
    this.gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, `${parcelSceneId}\t${method}\t${payload}`)
  }

  SetSceneDebugPanel() {
    this.gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel')
  }

  SetEngineDebugPanel() {
    this.gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel')
  }
}
