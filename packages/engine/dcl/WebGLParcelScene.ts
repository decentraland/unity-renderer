import { LoadableParcelScene, EnvironmentData } from 'shared/types'
import { SceneWorker } from 'shared/world/SceneWorker'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { getParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { DEBUG, EDITOR } from 'config'
import { checkParcelSceneBoundaries } from '../entities/utils/checkParcelSceneLimits'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { encodeParcelSceneBoundaries, gridToWorld, worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { removeEntityHighlight, highlightEntity } from 'engine/components/ephemeralComponents/HighlightBox'
import { createAxisEntity, createParcelOutline } from '../entities/utils/debugEntities'
import { createLogger } from 'shared/logger'
import { DevTools } from 'shared/apis/DevTools'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { WebGLScene } from './WebGLScene'
import { isScreenSpaceComponent } from 'engine/components/helpers/ui'
import { IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { camera, scene } from 'engine/renderer'
import { positionObservable } from 'shared/world/positionThings'

const auxVec2 = BABYLON.Vector2.Zero()

export const getUserInPlace = () => {
  worldToGrid(camera.position, auxVec2)
  return `${auxVec2.x},${auxVec2.y}`
}

export class WebGLParcelScene extends WebGLScene<LoadableParcelScene> {
  public encodedPositions: string
  public setOfEntitiesOutsideBoundaries = new Set<BaseEntity>()
  public userInPlace: string | null = null

  private uiComponent: any = null
  private parcelCenters: BABYLON.Vector3[] = []
  private shouldValidateBoundaries = false

  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    super(
      data,
      new SharedSceneContext(
        data.baseUrl,
        (data.data.basePosition.x + '_' + data.data.basePosition.y + '_' + data.id).substr(0, 32).toLowerCase()
      )
    )

    // Initialize context and file mappings
    this.context.registerMappings(data.data.contents)

    if (data.data.land && data.data.land.scene && data.data.land.scene._mappings) {
      for (let file in data.data.land.scene._mappings) {
        const hash = data.data.land.scene._mappings[file]
        if (hash.startsWith('data:') || hash.startsWith('Qm')) {
          this.context.registerMappings([{ file, hash }])
        } else {
          this.logger.error(`Invalid injected mapping "${file}" it is not a data url or IPFS mapping`, {
            file,
            hash
          })
        }
      }
    }

    this.context.logger = this.logger = createLogger(data.data.basePosition.x + ',' + data.data.basePosition.y + ': ')

    this.encodedPositions = encodeParcelSceneBoundaries(data.data.basePosition, data.data.parcels)

    this.parcelCenters = data.data.parcels.map($ => {
      const vec = new BABYLON.Vector3()
      gridToWorld($.x, $.y, vec)
      return vec
    })

    Object.assign(this.context.metricsLimits, getParcelSceneLimits(data.data.parcels.length))

    // Set a debug name in the root entity
    this.context.rootEntity.name = this.context.rootEntity.id = `parcelScene:${this.data.data.basePosition.x},${
      this.data.data.basePosition.y
    }`

    // Set the parcel's initial position
    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, this.context.rootEntity.position)
    this.context.rootEntity.freezeWorldMatrix()

    this.userInPlace = getUserInPlace()

    this.context.on('metricsUpdate', data => {
      if (!EDITOR) {
        this.checkLimits(data)
      }
    })

    if (DEBUG) {
      this.context.onEntityMatrixChangedObservable.add(_entity => {
        this.shouldValidateBoundaries = true
      })
      this.initDebugEntities()
    }

    this.context.updateMetrics()

    scene.onAfterRenderObservable.add(this.afterRender)

    positionObservable.add(this.checkUserInPlace)
  }

  afterRender = () => {
    if (this.shouldValidateBoundaries) {
      this.shouldValidateBoundaries = false
      this.checkBoundaries()
    }
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, this.worker!.position)

    worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
      })
      .catch($ => this.logger.error('registerWorker', $))
  }

  dispose(): void {
    super.dispose()
    scene.onAfterRenderObservable.removeCallback(this.afterRender)
    positionObservable.removeCallback(this.checkUserInPlace)
  }

  /**
   * This method checks if we are passing the limits of the parcel.
   * It triggers an event in the context if we are exceeding the limit.
   */
  checkLimits(data: IEvents['metricsUpdate']) {
    if (
      data.limit.entities < data.given.entities ||
      data.limit.bodies < data.given.bodies ||
      data.limit.triangles < data.given.triangles ||
      data.limit.materials < data.given.materials ||
      data.limit.textures < data.given.textures ||
      data.limit.geometries < data.given.geometries
    ) {
      this.context.logger.error(
        `Unloading scene at ${this.data.data.basePosition.x},${this.data.data.basePosition.y} due to exceeded limits`,
        data
      )

      this.context.emit('limitsExceeded', data)

      if (this.worker) {
        this.worker.dispose()
      }
    }
  }

  checkUserInPlace = () => {
    if (this.uiComponent) {
      this.userInPlace = getUserInPlace()

      if (!this.uiComponent.isEnabled && this.encodedPositions.includes(this.userInPlace)) {
        this.uiComponent.isEnabled = true
      }

      if (this.uiComponent.isEnabled && !this.encodedPositions.includes(this.userInPlace)) {
        this.uiComponent.isEnabled = false
      }
    }
  }

  /**
   * Looks for entities outisde of the fences of the scene.
   * - **DEBUG**: it highlights the entities
   * - **NOT DEBUG**: it disables the entities
   * In both cases, when the entity re-enters the legal fences, it reapears.
   */
  checkBoundaries() {
    let didChange = false
    this.getUIComponent()

    const newSet = new Set<BaseEntity>()

    this.context.entities.forEach(entity => checkParcelSceneBoundaries(this.parcelCenters, newSet, entity))
    // remove the highlight from the entities that were outside but they are no longer outside
    this.setOfEntitiesOutsideBoundaries.forEach($ => {
      if (!newSet.has($)) {
        this.setOfEntitiesOutsideBoundaries.delete($)

        if (this.context.entities.has($.id)) {
          removeEntityHighlight($)

          $.setEnabled(true)
          this.context.emit('entityBackInScene', { entityId: $.uuid })
        }

        didChange = true
      }
    })

    // add the highlight to the entities outside fences
    newSet.forEach($ => {
      if (!this.setOfEntitiesOutsideBoundaries.has($)) {
        if (DEBUG || EDITOR) {
          highlightEntity($)
        } else {
          $.setEnabled(false)
        }
        this.context.emit('entityOutOfScene', { entityId: $.uuid })
        this.setOfEntitiesOutsideBoundaries.add($)
        didChange = true
      }
    })

    if (didChange) {
      const entities: string[] = []
      this.setOfEntitiesOutsideBoundaries.forEach($ => entities.push($.uuid))
      this.context.emit('entitiesOutOfBoundaries', { entities })
    }
  }

  private getUIComponent() {
    if (this.uiComponent) return

    for (let [, component] of this.context.disposableComponents) {
      if (isScreenSpaceComponent(component)) {
        this.uiComponent = component
        return
      }
    }
  }

  private initDebugEntities() {
    const outline = createParcelOutline(this.encodedPositions)

    this.context.rootEntity.setObject3D('ground', outline.ground)
    this.context.rootEntity.setObject3D('outline', outline.result)
    this.context.rootEntity.setObject3D('axis', createAxisEntity())
  }
}
