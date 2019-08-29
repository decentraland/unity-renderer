import { Vector2Component } from 'atomicHelpers/landHelpers'
import { SceneLifeCycleController } from './scene'
import { EventEmitter } from 'events'
import { ParcelLifeCycleController } from './parcel'

export class PositionLifecycleController extends EventEmitter {
  private positionSettled: boolean = false
  private currentSceneId?: string
  private currentlySightedScenes: string[] = []

  constructor(public parcelController: ParcelLifeCycleController, public sceneController: SceneLifeCycleController) {
    super()
    sceneController.on('Scene status', () => this.checkPositionSettlement())
  }

  async reportCurrentPosition(position: Vector2Component, teleported: boolean) {
    const parcels = this.parcelController.reportCurrentPosition(position)

    this.currentSceneId = await this.sceneController.requestSceneId(`${position.x},${position.y}`)

    if (parcels) {
      const newlySightedScenes = await this.sceneController.reportSightedParcels(parcels.sighted, parcels.lostSight)

      if (!this.eqSet(this.currentlySightedScenes, newlySightedScenes.sighted)) {
        this.currentlySightedScenes = newlySightedScenes.sighted
      }
    }

    if (teleported) {
      this.positionSettled = false
      this.emit('Unsettled Position')
    }

    this.checkPositionSettlement()
  }

  private eqSet(as: Array<any>, bs: Array<any>) {
    if (as.length !== bs.length) return false
    for (const a of as) if (!bs.includes(a)) return false
    return true
  }

  private checkPositionSettlement() {
    if (!this.positionSettled) {
      const settling = this.currentlySightedScenes.every($ => this.sceneController.isRenderable($))

      if (settling) {
        this.positionSettled = settling
        this.emit('Settled Position', this.currentSceneId)
      }
    }
  }
}
