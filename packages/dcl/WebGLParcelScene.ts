import { LoadableParcelScene, EnvironmentData } from 'shared/types'
import { SceneWorker } from 'shared/world/SceneWorker'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { getParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { DEBUG, EDITOR } from 'config'
import { checkParcelSceneBoundaries } from './entities/utils/checkParcelSceneLimits'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { encodeParcelSceneBoundaries, gridToWorld, encodeParcelPosition } from 'atomicHelpers/parcelScenePositions'
import { removeEntityHighlight, highlightEntity } from 'engine/components/ephemeralComponents/HighlightBox'
import { createAxisEntity, createParcelOutline } from './entities/utils/debugEntities'
import { createLogger } from 'shared/logger'
import { DevTools } from 'shared/apis/DevTools'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { WebGLScene } from './WebGLScene'
import { IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { scene } from 'engine/renderer'

export class WebGLParcelScene extends WebGLScene<LoadableParcelScene> {
  public encodedPositions: string

  private setOfEntitiesOutsideBoundaries = new Set<BaseEntity>()
  private parcelSet = new Set<string>()

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
          this.logger.error(`Invalid injected mapping "${file}" it is not a data url or IPFS mapping`, { file, hash })
        }
      }
    }

    this.context.logger = this.logger = createLogger(data.data.basePosition.x + ',' + data.data.basePosition.y + ': ')

    this.encodedPositions = encodeParcelSceneBoundaries(data.data.basePosition, data.data.parcels)

    this.parcelSet.add(encodeParcelPosition(data.data.basePosition))
    data.data.parcels.forEach($ => this.parcelSet.add(encodeParcelPosition($)))

    Object.assign(this.context.metricsLimits, getParcelSceneLimits(data.data.parcels.length))

    // Set a debug name in the root entity
    this.context.rootEntity.name = this.context.rootEntity.id = `parcelScene:${this.data.data.basePosition.x},${
      this.data.data.basePosition.y
    }`

    // Set the parcel's initial position
    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, this.context.rootEntity.position)
    this.context.rootEntity.freezeWorldMatrix()

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
  }

  afterRender = () => {
    if (this.shouldValidateBoundaries) {
      this.shouldValidateBoundaries = false
      this.checkBoundaries()
    }
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, this.worker.position)

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

  /**
   * Looks for entities outisde of the fences of the scene.
   * - **DEBUG**: it highlights the entities
   * - **NOT DEBUG**: it disables the entities
   * In both cases, when the entity re-enters the legal fences, it reapears.
   */
  checkBoundaries() {
    const newSet = new Set<BaseEntity>()
    this.context.entities.forEach(entity => checkParcelSceneBoundaries(this.parcelSet, newSet, entity))
    // remove the highlight from the entities that were outside but they are no longer outside
    this.setOfEntitiesOutsideBoundaries.forEach($ => {
      if (!newSet.has($) && this.context.entities.has($.id)) {
        removeEntityHighlight($)
        $.setEnabled(true)

        this.context.emit('entityBackInScene', {
          entityId: $.uuid
        })

        this.setOfEntitiesOutsideBoundaries.delete($)
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
        this.context.emit('entityOutOfScene', {
          entityId: $.uuid
        })
        this.setOfEntitiesOutsideBoundaries.add($)
      }
    })
  }

  private initDebugEntities() {
    const outline = createParcelOutline(this.encodedPositions)

    this.context.rootEntity.setObject3D('ground', outline.ground)
    this.context.rootEntity.setObject3D('outline', outline.result)
    this.context.rootEntity.setObject3D('axis', createAxisEntity())
  }
}
