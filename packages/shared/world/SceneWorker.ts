import { future } from 'fp-future'
import { Vector3 } from 'babylonjs'
import { ScriptingHost } from 'decentraland-rpc/lib/host'
import { error } from '../../engine/logger'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc'
import { playerConfigurations } from '../../config'
import { EntityAction, EnvironmentData } from 'shared/types'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'

// tslint:disable-next-line:whitespace
type EngineAPI = import('../apis/EngineAPI').EngineAPI

export type ParcelSceneAPI = {
  data: EnvironmentData<any>
  sendBatch(ctions: EntityAction[]): void
  registerWorker(event: SceneWorker): void
  dispose(): void
  on(event: string, cb: (event: any) => void): void
}

const gamekitWorkerRaw = require('raw-loader!../../../static/systems/scene.system.js')
const gamekitWorkerBLOB = new Blob([gamekitWorkerRaw])
const gamekitWorkerUrl = URL.createObjectURL(gamekitWorkerBLOB)

// this function is used in a onSystemReady.then(unmountSystem).
// we keep it separated and global because it is highly reusable
function unmountSystem(system: ScriptingHost) {
  try {
    system.unmount()
  } catch (e) {
    error('Error unmounting system', e)
  }
}

export class SceneWorker {
  public readonly system = future<ScriptingHost>()

  public engineAPI: EngineAPI | null = null
  public enabled = true

  public readonly position: Vector3 = new Vector3()
  private readonly lastSentPosition = new BABYLON.Vector3(0, 0, 0)
  private readonly lastSentRotation = new BABYLON.Quaternion(0, 0, 0, 1)

  constructor(public parcelScene: ParcelSceneAPI, transport?: ScriptingTransport) {
    parcelScene.registerWorker(this)

    this.loadSystem(transport)
      .then($ => this.system.resolve($))
      .catch($ => this.system.reject($))
  }

  dispose() {
    if (this.enabled) {
      this.enabled = false

      // Unmount the system
      if (this.system) {
        this.system.then(unmountSystem).catch(e => error('Unable to unmount system', e))
      }

      this.parcelScene.dispose()
    }
  }

  sendUserViewMatrix(
    obj: Readonly<{ position: BABYLON.Vector3; rotation: BABYLON.Vector3; quaternion: BABYLON.Quaternion }>
  ) {
    if (this.engineAPI && 'positionChanged' in this.engineAPI.subscribedEvents) {
      if (!this.lastSentPosition.equals(obj.position)) {
        this.engineAPI.sendSubscriptionEvent('positionChanged', {
          position: {
            x: obj.position.x - this.position.x,
            z: obj.position.z - this.position.z,
            y: obj.position.y - playerConfigurations.height
          },
          cameraPosition: obj.position,
          playerHeight: playerConfigurations.height
        })
        this.lastSentPosition.copyFrom(obj.position)
      }
    }

    if (this.engineAPI && 'rotationChanged' in this.engineAPI.subscribedEvents) {
      if (obj.quaternion && !this.lastSentRotation.equals(obj.quaternion)) {
        this.engineAPI.sendSubscriptionEvent('rotationChanged', {
          rotation: obj.rotation,
          quaternion: obj.quaternion
        })
        this.lastSentRotation.copyFrom(obj.quaternion)
      }
    }
  }

  sendPointerEvent(evt: BABYLON.PointerInfoPre) {
    if (!this.engineAPI) return
    if (!evt.ray) return

    if (evt.type === BABYLON.PointerEventTypes.POINTERDOWN && 'pointerDown' in this.engineAPI.subscribedEvents) {
      this.engineAPI.sendSubscriptionEvent('pointerDown', {
        from: {
          x: evt.ray.origin.x - this.position.x,
          y: evt.ray.origin.y - this.position.y,
          z: evt.ray.origin.z - this.position.z
        },
        direction: evt.ray.direction,
        length: evt.ray.length,
        pointerId: (evt.event as any).pointerId
      })
      return
    }

    if (evt.type === BABYLON.PointerEventTypes.POINTERUP && 'pointerUp' in this.engineAPI.subscribedEvents) {
      this.engineAPI.sendSubscriptionEvent('pointerUp', {
        from: {
          x: evt.ray.origin.x - this.position.x,
          y: evt.ray.origin.y - this.position.y,
          z: evt.ray.origin.z - this.position.z
        },
        direction: evt.ray.direction,
        length: evt.ray.length,
        pointerId: (evt.event as any).pointerId
      })
      return
    }
  }

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    system.enable()

    return system
  }

  private async loadSystem(transport?: ScriptingTransport): Promise<ScriptingHost> {
    const worker = new (Worker as any)(gamekitWorkerUrl, { name: 'ParcelSceneWorker' })
    return this.startSystem(transport || WebWorkerTransport(worker))
  }
}
