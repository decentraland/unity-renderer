import * as BABYLON from 'babylonjs'
import { loadTexture, registerContextInResourceManager, loadFile } from '../renderer/monkeyLoader'
import { scene } from '../renderer'
import { registerLoadingContext } from 'engine/renderer/monkeyLoader'
import { resolveUrl } from 'atomicHelpers/parseUrl'
import { DisposableComponent } from 'engine/components/disposableComponents/DisposableComponent'
import { BaseEntity } from './BaseEntity'

import {
  ContentMapping,
  UpdateEntityComponentPayload,
  AttachEntityComponentPayload,
  ComponentCreatedPayload,
  ComponentDisposedPayload,
  ComponentRemovedPayload,
  ComponentUpdatedPayload,
  CreateEntityPayload,
  RemoveEntityPayload,
  SetEntityParentPayload,
  SceneStartedPayload
} from 'shared/types'

import { ILogger, defaultLogger } from 'shared/logger'
import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { IParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { measureObject3D, areBoundariesIgnored } from './utils/checkParcelSceneLimits'
import { IEventNames, IEvents, InputEventResult } from 'decentraland-ecs/src/decentraland/Types'
import { Observable, ReadOnlyVector3 } from 'decentraland-ecs/src'
import { colliderMaterial } from './utils/colliders'
import future from 'fp-future'

function validateHierarchy(entity: BaseEntity) {
  let parent: BaseEntity = entity

  if (!entity.parentEntity) {
    entity.context.logger.error('the entity has no parent')
    return
  }

  do {
    parent = parent.parentEntity!

    if (parent.isDisposed()) {
      entity.context.logger.error('parenting to a disposed entity')
    }
  } while (parent.parentEntity)

  if (parent.uuid !== '0') {
    entity.context.logger.error('the entity has a root different to 0')
    return
  }
}

export class SharedSceneContext implements BABYLON.IDisposable {
  public disposableComponents = new Map<string, DisposableComponent>()
  public entities = new Map<string, BaseEntity>()

  public metrics: IParcelSceneLimits = {
    triangles: 0,
    bodies: 0,
    entities: 0,
    materials: 0,
    textures: 0,
    geometries: 0
  }

  /**
   * Limits calculed based on the parcels of the parcelScene. Zero if no limits should be applied to the scene
   */
  public metricsLimits: IParcelSceneLimits = {
    triangles: 0,
    entities: 0,
    bodies: 0,
    materials: 0,
    textures: 0,
    geometries: 0
  }

  public onEntityMatrixChangedObservable = new Observable<BaseEntity>()
  public registeredMappings: Map<string, string | Blob | File> = new Map()

  public rootEntity = new BaseEntity('0', this)

  public logger: ILogger = defaultLogger

  public readonly internalBaseUrl: string
  public onDisposeObservable = new Observable<SharedSceneContext>()

  private _disposed = false
  private eventSubscriber = new EventDispatcher()
  private shouldUpdateMetrics = true
  private sceneStarted = future<SceneStartedPayload>()

  constructor(public baseUrl: string, public readonly domain: string, public useMappings: boolean = true) {
    this.internalBaseUrl = domain ? `dcl://${domain}/` : baseUrl
    registerLoadingContext(this)
    registerContextInResourceManager(this)
    scene.onAfterRenderObservable.add(this.afterRenderScene)
  }

  afterRenderScene = () => {
    if (this.shouldUpdateMetrics === true) {
      this.shouldUpdateMetrics = false
      const entities = this.entities.size - 1
      let triangles = 0
      let bodies = 0
      const currentUsage = this.getCurrentUsages()

      const childrenMeshes = new Set<BABYLON.AbstractMesh>()

      this.entities.forEach((entity, key) => {
        if (key === '0') return

        entity.getChildMeshes().forEach($ => {
          if (!areBoundariesIgnored($)) childrenMeshes.add($)
        })
      })

      childrenMeshes.forEach(mesh => {
        const r = measureObject3D(mesh)
        triangles += r.triangles
        bodies += r.bodies
      })

      const newMetrics = {
        entities,
        triangles,
        bodies,
        textures: currentUsage.textures.size,
        materials: currentUsage.materials.size,
        geometries: currentUsage.geometries.size
      }

      if (JSON.stringify(newMetrics) === JSON.stringify(this.metrics)) {
        return
      }

      this.metrics = newMetrics

      this.emit('metricsUpdate', {
        given: this.metrics,
        limit: this.metricsLimits
      })
    }
  }

  getCurrentUsages() {
    let textures = new Set<BABYLON.Texture>()
    let materials = new Set<BABYLON.Material>()
    let geometries = new Set<BABYLON.Geometry>()
    let audioClips = new Set<any>()

    this.disposableComponents.forEach($ => {
      $.contributions.textures.forEach($ => textures.add($))
      $.contributions.materials.forEach($ => materials.add($))
      $.contributions.geometries.forEach($ => geometries.add($))
      $.contributions.audioClips.forEach($ => audioClips.add($))
    })

    materials.delete(scene.defaultMaterial)
    materials.delete(colliderMaterial)

    return { textures, materials, geometries, audioClips }
  }

  /**
   * Counts and aggregates the number of triangles materials and entities
   */
  updateMetrics() {
    this.shouldUpdateMetrics = true
  }

  on<T extends IEventNames>(event: T, cb: (data: IEvents[T]) => void) {
    return this.eventSubscriber.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]) {
    this.eventSubscriber.emit(event, data)
  }

  /// #ECS.InitMessagesFinished: This message is sent after the scene ends executing the initialization code. Before the render loop.
  InitMessagesFinished(payload: SceneStartedPayload) {
    this.sceneStarted.resolve(payload)
  }

  /// #ECS.UpdateEntityComponent: Updates an ephemeral component C by Name in the entity E
  UpdateEntityComponent(payload: UpdateEntityComponentPayload) {
    /// 0) Find the entity E by ID
    const entity = this.entities.get(payload.entityId)
    /// 1) If E doesn't exist, finalize.
    if (!entity) return this.logger.error(`UpdateEntityComponent: Entity ${payload.entityId} doesn't exist`)
    /// 2) E.UpdateComponent(Name, ClassID, JSON)
    entity.updateComponent(payload)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.AttachEntityComponent: Attach the disposable component C to the entity E
  AttachEntityComponent(payload: AttachEntityComponentPayload) {
    /// 0) Find the entity E by ID
    const entity = this.entities.get(payload.entityId)
    /// 1) If E doesn't exist, finalize.
    if (!entity) return this.logger.error(`AttachEntityComponent: Entity ${payload.entityId} doesn't exist`)
    /// 2) Find the component C by ID
    const component = this.disposableComponents.get(payload.id)
    /// 3) If C doesn't exist, finalize.
    if (!component) return this.logger.error(`AttachEntityComponent: Component ${payload.id} doesn't exist`)
    /// 4) E.attachDisposableComponent(slotName, C)
    entity.attachDisposableComponent(payload.name, component)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.ComponentCreated: Creates a disposable component C by `classID` and stores it by ID
  ComponentCreated(payload: ComponentCreatedPayload) {
    // TODO: Define behavior for existent disposable component by ID
    if (this.disposableComponents.has(payload.id)) {
      this.logger.warn(`Component ${payload.id}(${payload.name}) is already registered`)
    }
    /// 0) Find the disposable component constructor K by `classId`
    const K = DisposableComponent.constructors.get(payload.classId)
    /// 1) If K doesn't exist, finalize.
    if (!K) {
      return this.logger.error(`instantiateComponent(classId: ${payload.classId}) is not registered`)
    }
    /// 2) Create instance C using K
    const C = new (K as any)(this, payload.id)
    /// 3) Register C in the map of disposable components
    this.disposableComponents.set(payload.id, C)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.ComponentDisposed: Disposes a disposable component C and releases all of it allocated resources
  ComponentDisposed(payload: ComponentDisposedPayload) {
    /// 0) Find the disposable component C by ID
    const component = this.disposableComponents.get(payload.id)
    /// 1) If C doesn't exist, finalize
    if (!component) return this.logger.error(`ComponentDisposed: Component ${payload.id} doesn't exist`)
    /// 2) Dispose the component C
    component.dispose()
    /// 3) Remove component C from map of disposable components
    this.disposableComponents.delete(payload.id)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.ComponentRemoved: Removes a component by Name from an entity E
  ComponentRemoved(payload: ComponentRemovedPayload) {
    /// 0) Find the entity E by ID
    const entity = this.entities.get(payload.entityId)
    /// 1) If E doesn't exist, finalize
    if (!entity) return this.logger.error(`ComponentRemoved: Entity ${payload.entityId} doesn't exist`)
    /// 2) remove the component Name from E
    entity.removeComponentByName(payload.name)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.ComponentUpdated: Updates a disposable component C using a JSON payload
  ComponentUpdated(payload: ComponentUpdatedPayload) {
    /// 0) Find the disposable component C by ID
    const component = this.disposableComponents.get(payload.id)
    /// 1) If C doesn't exist, finalize
    if (!component) return this.logger.error(`ComponentUpdated: Component ${payload.id} doesn't exist`)
    /// 2) C.Update(JSON)
    component.updateData(JSON.parse(payload.json)).catch($ => this.logger.error('ComponentUpdated', $))

    this.shouldUpdateMetrics = true
  }

  /// #ECS.CreateEntity
  CreateEntity(payload: CreateEntityPayload) {
    /// 0) If the created ID will be '0', finalize.
    if (payload.id === '0') return
    /// 1) If the entity is already created, finalize.
    if (this.entities.has(payload.id)) return
    /// 2) Create the entity E
    const E = new BaseEntity(payload.id, this)
    /// 3) Attach entity E to rootEntity
    E.setParentEntity(this.rootEntity)
    /// 4) Add entity E to the entity map
    this.entities.set(payload.id, E)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.RemoveEntity
  RemoveEntity(payload: RemoveEntityPayload) {
    /// 0) If the ECS tries to remove the root entity, finalize.
    if (payload.id === '0') return this.logger.error(`RemoveEntity: Trying to remove entity '0'`)
    /// 1) Get the entity E
    const entity = this.entities.get(payload.id)
    /// 2) If E doesn't exist, finalize.
    if (!entity) return this.logger.error(`RemoveEntity: Entity ${payload.id} doesn't exist`)
    /// 3) For all entity C in P.children
    for (let C of entity.childEntities()) {
      /// 3.0) Set C as child of rootEntity
      C.setParentEntity(this.rootEntity)
    }
    /// 4) E.Dispose()
    entity.dispose()
    /// 5) Remove E from the entity list
    this.entities.delete(payload.id)

    this.shouldUpdateMetrics = true
  }

  /// #ECS.SetEntityParent: Set the entity E as child of entity P
  SetEntityParent(payload: SetEntityParentPayload) {
    /// 0) If entityId is '0', finalize.
    if (payload.entityId === '0') return
    /// 1) Find the entity E
    const E = this.entities.get(payload.entityId)
    /// 2) If E doesn't exist, finalize.
    if (!E) return this.logger.error(`SetEntityParent: Entity ${payload.entityId} doesn't exist`)
    /// 3) If parentId is '0'.
    if (payload.parentId === '0') {
      /// 3.0) If True, Set E as child of rootEntity
      E.setParentEntity(this.rootEntity)
      validateHierarchy(E)
    } else {
      /// 3.1) If False, Find entity P
      const parent = this.entities.get(payload.parentId)
      /// 3.2) If P exists
      if (parent) {
        /// 3.2.0) If True, Set E as child of P
        E.setParentEntity(parent)
        validateHierarchy(E)
      } else {
        this.logger.error(`SetEntityParent: Parent ${payload.parentId} doesn't exist`)
        /// 3.2.1) If Flase, Set E as child of rootEntity
        E.setParentEntity(this.rootEntity)
        validateHierarchy(E)
      }
    }
  }

  dispatchUUIDEvent(uuid: string, payload: any): void {
    this.eventSubscriber.emit('uuidEvent', {
      uuid,
      payload
    })
  }

  sendPointerEvent(
    pointerEventType: 'pointerDown' | 'pointerUp',
    pointerId: number,
    entityId: string,
    pickingResult: BABYLON.PickingInfo
  ) {
    if (!pickingResult.ray) return

    const event: InputEventResult = {
      pointerId,
      origin: {
        x: pickingResult.ray.origin.x - this.rootEntity.position.x,
        y: pickingResult.ray.origin.y - this.rootEntity.position.y,
        z: pickingResult.ray.origin.z - this.rootEntity.position.z
      },
      direction: pickingResult.ray.direction
    }

    if (pickingResult.hit && entityId) {
      event.hit = {
        length: pickingResult.distance,
        hitPoint: {
          x: pickingResult.pickedPoint!.x - this.rootEntity.position.x,
          y: pickingResult.pickedPoint!.y - this.rootEntity.position.y,
          z: pickingResult.pickedPoint!.z - this.rootEntity.position.z
        },
        normal: pickingResult.getNormal() as ReadOnlyVector3,
        worldNormal: pickingResult.getNormal(true) as ReadOnlyVector3,
        meshName: pickingResult.pickedMesh!.name,
        entityId
      }
    }

    if (pointerEventType === 'pointerDown') {
      this.emit('pointerDown', event)
    } else if (pointerEventType === 'pointerUp') {
      this.emit('pointerUp', event)
    }
  }

  public isDisposed() {
    return this._disposed
  }

  public getComponent(id: string): DisposableComponent | null {
    return this.disposableComponents.get(id) || null
  }

  public getTexture(
    path: string,
    samplerData?: BABYLON.GLTF2.Loader._ISamplerData & { invertY?: boolean }
  ): BABYLON.Texture {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }

    let pathToLoad = path

    if (path.match(/^data:[^\/]+\/[^;]+;base64,/)) {
      pathToLoad = path
    } else {
      pathToLoad = this.resolveUrl(path)
    }

    return loadTexture(pathToLoad, samplerData)
  }

  public resolveUrl(url: string): string {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }
    const sanitizedUrl = this.sanitizeURL(url)
    if (this.useMappings) {
      const mapping = this.registeredMappings.get(sanitizedUrl)
      if (!mapping || typeof mapping !== 'string') {
        throw new Error(`File not found: ${sanitizedUrl}`)
      } else {
        return resolveUrl(this.baseUrl, mapping)
      }
    } else {
      return resolveUrl(this.baseUrl, sanitizedUrl)
    }
  }

  public async getBlob(url: string): Promise<Blob> {
    return this.fetchBlob(this.resolveUrl(url))
  }

  public async getArrayBuffer(url: string): Promise<ArrayBuffer> {
    return this.fetchArrayBuffer(this.resolveUrl(url))
  }

  public async getText(url: string): Promise<ArrayBuffer> {
    return this.fetchText(this.resolveUrl(url))
  }

  public dispose() {
    this.onDisposeObservable.notifyObservers(this)
    this.onDisposeObservable.clear()

    scene.onAfterRenderObservable.removeCallback(this.afterRenderScene)

    if (this.rootEntity) {
      this.rootEntity.disposeTree(this.entities)

      if (this.entities.size) {
        this.logger.error('there are non-disposed entities in the scene being disposed', this.entities)
      }

      delete this.rootEntity
    }

    this.disposableComponents.forEach($ => $.dispose())
    this.disposableComponents.clear()

    this._disposed = true
  }

  public registerMappings(mappings: Array<ContentMapping>) {
    for (const { file, hash } of mappings) {
      if (file && hash) {
        this.registeredMappings.set(file.toLowerCase(), hash)
      }
    }
  }

  private async fetchBlob(urlToLoad: string) {
    // TODO: cache fetch promise so we execute 1 concurrent request only
    const data = await loadFile(urlToLoad)

    return new Blob([new Uint8Array(data)])
  }

  private async fetchArrayBuffer(urlToLoad: string) {
    // TODO: cache fetch promise so we execute 1 concurrent request only
    return loadFile(urlToLoad)
  }

  private async fetchText(urlToLoad: string) {
    // TODO: cache fetch promise so we execute 1 concurrent request only
    return loadFile(urlToLoad, false)
  }

  private sanitizeURL(url: string): string {
    if (url.startsWith('data:')) {
      return url
    }
    if (!url) {
      throw new Error('Invalid URL')
    }
    return decodeURIComponent(BABYLON.Tools.CleanUrl(url.replace(/^(\/+)/, '').toLowerCase()))
  }
}
