import { SceneLifeCycleStatus } from '../lib/scene.status'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import future, { IFuture } from 'fp-future'
import { EventEmitter } from 'events'
import { SceneDataDownloadManager } from './download'
import { createLogger } from 'shared/logger'

const logger = createLogger('SceneLifeCycleController: ')

export class SceneLifeCycleController extends EventEmitter {
  private downloadManager: SceneDataDownloadManager

  private _positionToSceneId = new Map<string, string | undefined>()
  private futureOfPositionToSceneId = new Map<string, IFuture<string | undefined>>()
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
      this._positionToSceneId.has(position) &&
      this.sceneStatus.has(this._positionToSceneId.get(position)!) &&
      this.sceneStatus.get(this._positionToSceneId.get(position)!)!.isAwake()
    )
  }

  async onSight(position: string) {
    let sceneId = await this.requestSceneId(position)

    if (sceneId) {
      const previousSightCount = this.sceneParcelSightCount.get(sceneId) || 0
      this.sceneParcelSightCount.set(sceneId, previousSightCount + 1)

      if (!this.sceneStatus.has(sceneId)) {
        const data = await this.downloadManager.getParcelData(position)
        if (data) {
          this.sceneStatus.set(sceneId, new SceneLifeCycleStatus(data))
        }
      }

      if (this.sceneStatus.get(sceneId)!.isDead()) {
        this.emit('Preload scene', sceneId)
        this.sceneStatus.get(sceneId)!.status = 'awake'
      }
    }
  }
  async lostSight(position: string) {
    let sceneId = await this.requestSceneId(position)
    if (!sceneId) {
      return
    }
    const previousSightCount = this.sceneParcelSightCount.get(sceneId) || 0
    const newSightCount = previousSightCount - 1
    this.sceneParcelSightCount.set(sceneId, newSightCount)

    if (newSightCount <= 0) {
      logger.log('Parcel out of sight killing', sceneId)
      const sceneStatus = this.sceneStatus.get(sceneId)
      if (sceneStatus && sceneStatus.isAwake()) {
        sceneStatus.status = 'unloaded'
        this.emit('Unload scene', sceneId)
      }
    }
  }

  reportDataLoaded(sceneId: string) {
    if (this.sceneStatus.has(sceneId) && this.sceneStatus.get(sceneId)!.status === 'awake') {
      this.sceneStatus.get(sceneId)!.status = 'ready'
      this.emit('Start scene', sceneId)
    }
  }

  async requestSceneId(position: string): Promise<string | undefined> {
    if (this._positionToSceneId.has(position)) {
      return this._positionToSceneId.get(position)!
    }
    if (!this.futureOfPositionToSceneId.has(position)) {
      this.futureOfPositionToSceneId.set(position, future<string | undefined>())
      try {
        const land = await this.downloadManager.getParcelData(position)

        if (!land) {
          this.futureOfPositionToSceneId.get(position)!.resolve(undefined)
          return this.futureOfPositionToSceneId.get(position)!
        }

        for (const pos of land.scene.scene.parcels) {
          if (!this._positionToSceneId.has(pos)) {
            this._positionToSceneId.set(pos, land.sceneId)
          }

          const futurePosition = this.futureOfPositionToSceneId.get(pos)
          if (futurePosition && futurePosition.isPending) {
            futurePosition.resolve(land.sceneId)
          }
        }
      } catch (e) {
        this.futureOfPositionToSceneId.get(position)!.reject(e)
      }
      return this.futureOfPositionToSceneId.get(position)!
    } else {
      const sceneId = await this.futureOfPositionToSceneId.get(position)!
      this._positionToSceneId.set(position, sceneId)
      return sceneId
    }
  }
}
