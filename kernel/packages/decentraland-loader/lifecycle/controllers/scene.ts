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
  private enabledEmpty: boolean

  constructor(opts: { downloadManager: SceneDataDownloadManager; enabledEmpty: boolean }) {
    super()
    this.downloadManager = opts.downloadManager
    this.enabledEmpty = opts.enabledEmpty
  }

  contains(status: SceneLifeCycleStatus, position: Vector2Component) {
    return (
      status.sceneDescription &&
      status.sceneDescription.sceneJsonData.scene.parcels.includes(`${position.x},${position.y}`)
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

  diff<T>(a1: T[], a2: T[]): T[] {
    return a1.filter(i => a2.indexOf(i) < 0)
  }

  async reportSightedParcels(sightedParcels: string[], lostSightParcels: string[]) {
    const sighted = await this.fetchSceneIds(sightedParcels)
    const lostSight = await this.fetchSceneIds(lostSightParcels)

    await this.onSight(sighted)

    const difference = this.diff(lostSight, sighted)
    this.lostSight(difference)

    return { sighted, lostSight: difference }
  }

  async fetchSceneIds(positions: string[]): Promise<string[]> {
    const sceneIds = await this.requestSceneIds(positions)

    return sceneIds.filter($ => !!$).filter(this.distinct) as string[]
  }

  async onSight(sceneIds: string[]) {
    sceneIds.forEach(async sceneId => {
      try {
        if (!this.sceneStatus.has(sceneId)) {
          const data = await this.downloadManager.resolveLandData(sceneId)
          if (data) {
            this.sceneStatus.set(sceneId, new SceneLifeCycleStatus(data))
          }
        }

        if (this.sceneStatus.get(sceneId)!.isDead()) {
          this.emit('Preload scene', sceneId)
          this.sceneStatus.get(sceneId)!.status = 'awake'
        }
      } catch (e) {
        defaultLogger.error(`error while loading scene ${sceneId}`, e)
      }
    })
  }

  lostSight(sceneIds: string[]) {
    sceneIds.forEach(sceneId => {
      const sceneStatus = this.sceneStatus.get(sceneId)
      if (sceneStatus && sceneStatus.isAwake()) {
        sceneStatus.status = 'unloaded'
        this.emit('Unload scene', sceneId)
      }
    })
  }

  reportDataLoaded(sceneId: string) {
    if (this.sceneStatus.has(sceneId) && this.sceneStatus.get(sceneId)!.status === 'awake') {
      this.sceneStatus.get(sceneId)!.status = 'loaded'
      this.emit('Start scene', sceneId)
    }
  }

  isRenderable(sceneId: SceneId): boolean {
    const status = this.sceneStatus.get(sceneId)
    return !!status && (status.isReady() || status.isFailed())
  }

  reportStatus(sceneId: string, status: SceneLifeCycleStatusType) {
    const lifeCycleStatus = this.sceneStatus.get(sceneId)
    if (!lifeCycleStatus) {
      defaultLogger.info(`no lifecycle status for scene ${sceneId}`)
      return
    }
    lifeCycleStatus.status = status

    this.emit('Scene status', { sceneId, status })
  }

  async requestSceneIds(tiles: string[]): Promise<(string | undefined)[]> {
    const futures: Promise<string | undefined>[] = []

    const missingTiles: string[] = []

    for (const tile of tiles) {
      let promise: IFuture<string | undefined>

      if (this._positionToSceneId.has(tile)) {
        promise = this.futureOfPositionToSceneId.get(tile)!
      } else {
        promise = future()

        this.futureOfPositionToSceneId.set(tile, promise)
        missingTiles.push(tile)
      }

      futures.push(promise)
    }

    if (missingTiles.length > 0) {
      const pairs = await this.downloadManager.resolveSceneSceneIds(missingTiles)

      for (const [tile, sceneId] of pairs) {
        let result = sceneId ??
          // empty scene!
          (this.enabledEmpty ? ('Qm' + tile + 'm').padEnd(46, '0') : undefined)

        this.futureOfPositionToSceneId.get(tile)!.resolve(result)

        this._positionToSceneId.set(tile, result)
      }
    }

    return Promise.all(futures)
  }
}
