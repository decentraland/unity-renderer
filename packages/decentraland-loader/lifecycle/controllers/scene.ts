import { SceneLifeCycleStatus, SceneLifeCycleStatusType } from '../lib/scene.status'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import future, { IFuture } from 'fp-future'
import { EventEmitter } from 'events'
import { SceneDataDownloadManager } from './download'
import { Observable } from 'decentraland-ecs/src/ecs/Observable'
import defaultLogger from 'shared/logger'

export type SceneLifeCycleStatusReport = { sceneId: string; status: SceneLifeCycleStatusType }

export const sceneLifeCycleObservable = new Observable<Readonly<SceneLifeCycleStatusReport>>()

type SceneId = string

export class SceneLifeCycleController extends EventEmitter {
  private downloadManager: SceneDataDownloadManager

  private _positionToSceneId = new Map<string, SceneId | undefined>()
  private futureOfPositionToSceneId = new Map<string, IFuture<SceneId | undefined>>()
  private sceneStatus = new Map<SceneId, SceneLifeCycleStatus>()

  private sceneParcelSightCount = new Map<SceneId, number>()

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

  distinct(value: any, index: number, self: Array<any>) {
    return self.indexOf(value) === index
  }

  async onSight(positions: string[]) {
    const positionSceneIds = (await Promise.all(
      positions.map(position => this.requestSceneId(position).then(sceneId => ({ position, sceneId })))
    )).filter(({ sceneId }) => sceneId !== undefined) as { position: string; sceneId: SceneId }[]

    positionSceneIds.forEach(async ({ position, sceneId }) => {
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
    })

    return positionSceneIds.map($ => $.sceneId).filter(this.distinct)
  }

  lostSight(positions: string[]) {
    positions.forEach(async position => {
      let sceneId = await this.requestSceneId(position)
      if (!sceneId) {
        return
      }
      const previousSightCount = this.sceneParcelSightCount.get(sceneId) || 0
      const newSightCount = previousSightCount - 1
      this.sceneParcelSightCount.set(sceneId, newSightCount)

      if (newSightCount <= 0) {
        const sceneStatus = this.sceneStatus.get(sceneId)
        if (sceneStatus && sceneStatus.isAwake()) {
          sceneStatus.status = 'unloaded'
          this.emit('Unload scene', sceneId)
        }
      }
    })
  }

  reportDataLoaded(sceneId: string) {
    if (this.sceneStatus.has(sceneId) && this.sceneStatus.get(sceneId)!.status === 'awake') {
      this.sceneStatus.get(sceneId)!.status = 'loaded'
      this.emit('Start scene', sceneId)
    }
  }

  isReady(sceneId: SceneId): boolean {
    const status = this.sceneStatus.get(sceneId)
    return !!status && status.isReady()
  }

  reportStatus(sceneId: string, status: SceneLifeCycleStatusType) {
    const lifeCycleStatus = this.sceneStatus.get(sceneId)
    if (!lifeCycleStatus) {
      defaultLogger.info(`no lifecycle status for scene ${sceneId}`)
      return
    }
    lifeCycleStatus.status = status

    this.emit('Scene ready', sceneId)
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
