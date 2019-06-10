import { future } from 'fp-future'
import { ScriptingHost } from 'decentraland-rpc/lib/host'
import { error } from '../../engine/logger'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc'
import { playerConfigurations } from '../../config'
import { EntityAction, EnvironmentData } from 'shared/types'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import { Vector3, Quaternion, ReadOnlyVector3, ReadOnlyQuaternion } from 'decentraland-ecs/src/decentraland/math'

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

const hudWorkerRaw = require('raw-loader!../../../static/systems/decentraland-ui.scene.js')
const hudWorkerBLOB = new Blob([hudWorkerRaw])
export const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)

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

  /** false if this worker part of a dynamically loaded scene */
  public persistent = false

  public readonly position: Vector3 = new Vector3()
  private readonly lastSentPosition = new Vector3(0, 0, 0)
  private readonly lastSentRotation = new Quaternion(0, 0, 0, 1)

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
    obj: Readonly<{ position: ReadOnlyVector3; rotation: ReadOnlyVector3; quaternion: ReadOnlyQuaternion }>
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

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    system.enable()

    return system
  }

  private async loadSystem(transport?: ScriptingTransport): Promise<ScriptingHost> {
    const worker = new (Worker as any)(gamekitWorkerUrl, { name: `ParcelSceneWorker(${this.parcelScene.data.id})` })
    return this.startSystem(transport || WebWorkerTransport(worker))
  }
}
