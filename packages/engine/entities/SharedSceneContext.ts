import * as BABYLON from 'babylonjs'
import { loadTexture } from './loader'
import { scene } from '../renderer'
import { registerLoadingContext, removeLoadingContext } from 'engine/renderer/monkeyLoader'
import { resolveUrl } from 'atomicHelpers/parseUrl'
import { future } from 'fp-future'
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
  SetEntityParentPayload
} from 'shared/types'

import { ILogger, defaultLogger } from 'shared/logger'
import { IEvents, IEventNames } from 'shared/events'
import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { IParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { measureObject3D } from 'dcl/entities/utils/checkParcelSceneLimits'

function validateHierarchy(entity: BaseEntity) {
  let parent = entity

  if (!entity.parentEntity) {
    entity.context.logger.error('the entity has no parent')
    debugger
    return
  }

  do {
    parent = parent.parentEntity

    if (parent.isDisposed()) {
      entity.context.logger.error('parenting to a disposed entity')
      debugger
    }
  } while (parent.parentEntity)

  if (parent.uuid !== '0') {
    entity.context.logger.error('the entity has a root different to 0')
    debugger
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

  public mappings: Map<string, string | Blob | File> = new Map()
  public textures: Map<string, BABYLON.Texture> = new Map()

  public rootEntity = new BaseEntity('0', this)

  public logger: ILogger = defaultLogger

  public readonly internalBaseUrl: string

  // TODO(menduz): replace this by a properties/configuration bag
  hideAxis?: boolean
  isInternal?: boolean

  private textureSet: Set<BABYLON.Texture> = new Set()
  private _disposed = false
  private eventSubscriber = new EventDispatcher()

  constructor(public baseUrl: string, public readonly domain: string, public useMappings: boolean = true) {
    this.internalBaseUrl = domain ? `dcl://${domain}/` : baseUrl
    registerLoadingContext(this)
  }

  /**
   * Counts and aggregates the number of triangles materials and entities
   */
  updateMetrics() {
    let entities = 0
    let triangles = 0
    let bodies = 0
    let textures = 0
    let materials = 0
    let geometries = 0

    this.disposableComponents.forEach($ => {
      textures += $.contributions.textureCount
      materials += $.contributions.materialCount
      geometries += $.contributions.geometriesCount
    })

    textures += this.textures.size

    entities++

    const childrenMeshes = this.rootEntity.getChildTransformNodes(false, node => {
      if ('isDCLEntity' in node) {
        entities++
      }
      return node instanceof BABYLON.AbstractMesh
    })

    for (let i = 0; i < childrenMeshes.length; i++) {
      const r = measureObject3D(childrenMeshes[i])
      entities += r.entities
      triangles += r.triangles
      bodies += r.bodies
    }

    this.metrics = { entities, triangles, bodies, textures, materials, geometries }
  }

  on<T extends IEventNames>(event: T, cb: (data: IEvents[T]) => void) {
    return this.eventSubscriber.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]) {
    this.eventSubscriber.emit(event, data)
  }

  /// #ECS.UpdateEntityComponent: Updates an ephemeral component C by Name in the entity E
  UpdateEntityComponent(payload: UpdateEntityComponentPayload) {
    /// 0) Find the entity E by ID
    const entity = this.entities.get(payload.entityId)
    /// 1) If E doesn't exist, finalize.
    if (!entity) return this.logger.error(`UpdateEntityComponent: Entity ${payload.entityId} doesn't exist`)
    /// 2) E.UpdateComponent(Name, ClassID, JSON)
    entity.updateComponent(payload)
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
  }

  /// #ECS.ComponentRemoved: Removes a component by Name from an entity E
  ComponentRemoved(payload: ComponentRemovedPayload) {
    /// 0) Find the entity E by ID
    const entity = this.entities.get(payload.entityId)
    /// 1) If E doesn't exist, finalize
    if (!entity) return this.logger.error(`ComponentRemoved: Entity ${payload.entityId} doesn't exist`)
    /// 2) remove the component Name from E
    entity.removeComponentByName(payload.name)
  }

  /// #ECS.ComponentUpdated: Updates a disposable component C using a JSON payload
  ComponentUpdated(payload: ComponentUpdatedPayload) {
    /// 0) Find the disposable component C by ID
    const component = this.disposableComponents.get(payload.id)
    /// 1) If C doesn't exist, finalize
    if (!component) return this.logger.error(`ComponentUpdated: Component ${payload.id} doesn't exist`)
    /// 2) C.Update(JSON)
    component.updateData(JSON.parse(payload.json)).catch($ => this.logger.error('ComponentUpdated', $))
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

  public registerTexture(texture: BABYLON.Texture): any {
    this.textureSet.add(texture)
    texture.onDisposeObservable.add(this.textureGotRemoved)
  }

  public isDisposed() {
    return this._disposed
  }

  public async getTexture(path: string, _allowExternal = false): Promise<BABYLON.Texture> {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }

    if (!this.textures.has(path)) {
      if (path.match(/^data:[^\/]+\/[^;]+;base64,/)) {
        const defer = future<BABYLON.Texture>()
        const texture = new BABYLON.Texture(
          null,
          scene,
          false,
          false,
          BABYLON.Texture.BILINEAR_SAMPLINGMODE,
          () => {
            defer.resolve(texture)
          },
          (message, exception) => {
            defer.reject(message || exception || `Error loading ${path}`)
            this.textureGotRemoved(texture)
          },
          path,
          true
        )

        this.textures.set(path, texture)
        this.registerTexture(texture)

        return defer
      } else {
        const resolvedPath = resolveUrl(this.internalBaseUrl, path)
        const texture = await loadTexture(resolvedPath)
        this.textures.set(path, texture)
        this.registerTexture(texture)
      }
    }

    return this.textures.get(path)
  }

  public async getFile(url: string): Promise<File> {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }
    const blob = await this.getBlob(url)

    return new File([blob], url)
  }

  public resolveUrl(url: string) {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }
    const sanitizedUrl = this.sanitizeURL(url)
    if (this.useMappings) {
      const mapping = this.mappings.get(sanitizedUrl)
      if (!mapping || typeof mapping !== 'string') {
        throw new Error(`The mapping for ${this.internalBaseUrl}${sanitizedUrl} is not present.`)
      } else {
        return resolveUrl(this.baseUrl, mapping)
      }
    } else {
      return resolveUrl(this.baseUrl, sanitizedUrl)
    }
  }

  public async getBlob(url: string): Promise<Blob> {
    if (this._disposed) {
      throw new Error(`SharedSceneContext(${this.domain}) is disposed`)
    }
    const sanitizedUrl = this.sanitizeURL(url)

    if (this.useMappings) {
      if (!this.mappings.has(sanitizedUrl)) {
        throw new Error(`The mapping for ${this.internalBaseUrl}${sanitizedUrl} is not present.`)
      } else {
        return this.loadMapping(this.mappings.get(sanitizedUrl))
      }
    } else {
      return this.loadRelative(sanitizedUrl)
    }
  }

  public dispose() {
    if (this.rootEntity) {
      this.rootEntity.disposeTree(this.entities)

      if (this.entities.size) {
        this.logger.error('there are non-disposed entities in the scene being disposed', this.entities)
      }

      delete this.rootEntity
    }

    this.textureSet.forEach((texture, name) => {
      texture.dispose()
    })

    this.disposableComponents.forEach($ => $.dispose())

    this.textureSet.clear()
    this.textures.clear()

    this._disposed = true
    removeLoadingContext(this)
  }

  public registerMappings(mappings: Array<ContentMapping>) {
    for (const { file, hash } of mappings) {
      this.mappings.set(file.toLowerCase(), hash)
    }
  }

  private async loadRelative(relativePath: string | File | Blob): Promise<Blob> {
    if (relativePath instanceof Blob) {
      return relativePath
    }

    const urlToLoad = resolveUrl(this.baseUrl, relativePath)

    return this.fetch(urlToLoad)
  }

  private async loadMapping(mappingValue: string | File | Blob): Promise<Blob> {
    if (mappingValue instanceof Blob) {
      return mappingValue
    }

    return this.fetch(resolveUrl(this.baseUrl, mappingValue))
  }

  private async fetch(urlToLoad: string) {
    // TODO: cache fetch promise so we execute 1 concurrent request only
    const fetchResult = await fetch(urlToLoad)

    if (!fetchResult.ok) {
      throw new Error('Error loading ' + urlToLoad)
    }

    return fetchResult.blob()
  }

  private sanitizeURL(url: string): string {
    if (!url) {
      throw new Error('Invalid URL')
    }
    return decodeURIComponent(BABYLON.Tools.CleanUrl(url.replace(/^(\/+)/, '').toLowerCase()))
  }

  private textureGotRemoved = (texture: BABYLON.Texture) => {
    texture.getScene().removeTexture(texture)
    this.textureSet.delete(texture)
  }
}
