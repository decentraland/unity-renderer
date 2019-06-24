import { SceneLifeCycleStatus } from '../lib/scene.status'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import future, { IFuture } from 'fp-future'
import { EventEmitter } from 'events'
import { SceneDataDownloadManager } from './download'

export class SceneLifeCycleController extends EventEmitter {
  private downloadManager: SceneDataDownloadManager

  private _positionToSceneCID = new Map<string, string | undefined>()
  private futureOfPositionToCID = new Map<string, IFuture<string | undefined>>()
  private sceneStatus = new Map<string, SceneLifeCycleStatus>()

  private sceneParcelSightCount = new Map<string, number>()

  constructor(opts: { downloadManager: SceneDataDownloadManager }) {
    super()
    this.downloadManager = opts.downloadManager
  }

  contains(status: SceneLifeCycleStatus, position: Vector2Component) {
    return (
      status.sceneDescription && status.sceneDescription.scene.scene.parcels.includes(`${position.x},${position.y}`)
    )
  }

  hasStarted(position: string) {
    return (
      this._positionToSceneCID.has(position) &&
      this.sceneStatus.has(this._positionToSceneCID.get(position)!) &&
      this.sceneStatus.get(this._positionToSceneCID.get(position)!)!.isAwake()
    )
  }

  async onSight(position: string) {
    let sceneCID = await this.requestSceneCID(position)
    if (sceneCID) {
      const previousSightCount = this.sceneParcelSightCount.get(sceneCID) || 0
      this.sceneParcelSightCount.set(sceneCID, previousSightCount + 1)

      if (!this.sceneStatus.has(sceneCID)) {
        const data = await this.downloadManager.getParcelData(position)
        if (data) {
          this.sceneStatus.set(sceneCID, new SceneLifeCycleStatus(data))
        }
      }
      if (this.sceneStatus.get(sceneCID)!.isDead()) {
        this.emit('Preload scene', sceneCID)
        this.sceneStatus.get(sceneCID)!.status = 'awake'
      }
    }
  }
  async lostSight(position: string) {
    let sceneCID = await this.requestSceneCID(position)
    if (!sceneCID) {
      return
    }
    const previousSightCount = this.sceneParcelSightCount.get(sceneCID) || 0
    this.sceneParcelSightCount.set(sceneCID, previousSightCount - 1)

    if (this.sceneParcelSightCount.get(sceneCID)! <= 0) {
      if (this.sceneStatus.has(sceneCID) && this.sceneStatus.get(sceneCID)!.isAwake()) {
        this.sceneStatus.get(sceneCID)!.status = 'unloaded'
        this.emit('Unload scene', sceneCID)
      }
    }
  }

  reportDataLoaded(sceneCID: string) {
    if (this.sceneStatus.has(sceneCID) && this.sceneStatus.get(sceneCID)!.status === 'awake') {
      this.sceneStatus.get(sceneCID)!.status = 'ready'
      this.emit('Start scene', sceneCID)
    }
  }

  async requestSceneCID(position: string): Promise<string | undefined> {
    if (this._positionToSceneCID.has(position)) {
      return this._positionToSceneCID.get(position)!
    }
    if (!this.futureOfPositionToCID.has(position)) {
      this.futureOfPositionToCID.set(position, future<string | undefined>())
      try {
        const land = await this.downloadManager.getParcelData(position)
        if (!land) {
          this.futureOfPositionToCID.get(position)!.resolve(undefined)
          return this.futureOfPositionToCID.get(position)!
        }
        const sceneCID = land.mappingsResponse.contents.filter($ => $.file === 'scene.json')[0].hash
        for (const pos of land.scene.scene.parcels) {
          if (!this._positionToSceneCID.has(pos)) {
            this._positionToSceneCID.set(pos, sceneCID)
          }
          if (!this.futureOfPositionToCID.has(pos)) {
            continue
          }
          if (this.futureOfPositionToCID.get(pos)!.isPending) {
            this.futureOfPositionToCID.get(pos)!.resolve(sceneCID)
          }
        }
      } catch (e) {
        this.futureOfPositionToCID.get(position)!.reject(e)
      }
      return this.futureOfPositionToCID.get(position)!
    } else {
      const cid = await this.futureOfPositionToCID.get(position)!
      this._positionToSceneCID.set(position, cid)
      return cid
    }
  }
}
