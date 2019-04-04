import * as BABYLON from 'babylonjs'

import { error } from '../logger'

import { componentRegistry, BaseComponent } from '../components'
import { scene, engineMicroQueue } from '../renderer'
import { DisposableComponent, BasicShape } from 'engine/components/disposableComponents/DisposableComponent'
import { UpdateEntityComponentPayload } from 'shared/types'

import { CLASS_ID, Observable } from 'decentraland-ecs/src'
import { IEventNames, IEvents } from 'decentraland-ecs/src/decentraland/Types'

// tslint:disable-next-line:whitespace
type SharedSceneContext = import('./SharedSceneContext').SharedSceneContext

export type ConstructorOf<T> = {
  new (...args: any[]): T
}

export type Props = { [name: string]: any }

function matrixWorldDidUpdate(entity: BaseEntity): void {
  if (entity.sendPositionsPending || entity.previousWorldMatrix.equals(entity.worldMatrixFromCache)) {
    // it is scheduled or it shares the same worldMatrix. Do nothing
  } else {
    entity.previousWorldMatrix.copyFrom(entity._worldMatrix)
    entity.sendPositionsPending = true
    engineMicroQueue.queueMicroTask(entity.sendUpdatePositions)
  }
}

export class BaseEntity extends BABYLON.AbstractMesh {
  get parentEntity(): BaseEntity | null {
    return findParentEntity(this)
  }

  assetContainer!: BABYLON.AssetContainer | void
  isDCLEntity = true

  attrs: Props = {}

  components: { [key: string]: BaseComponent<any> } = {}

  onChangeObject3DObservable = new Observable<{ type: string; object: BABYLON.TransformNode | null }>()

  sendPositionsPending = false
  loadingDone = true
  previousWorldMatrix = BABYLON.Matrix.Zero()

  uuidEvents: Map<IEventNames, string> = new Map()
  events: Map<IEventNames, Function[]> = new Map()
  children: BaseEntity[] = []
  object3DMap: { [key: string]: BABYLON.TransformNode | BaseEntity } = {}
  _listeners: Record<string, Function[]> = {}
  disposableComponents = new Map<string, DisposableComponent>()

  constructor(public uuid: string, public context: SharedSceneContext) {
    super(uuid)
    context.entities.set(uuid, this)
    this.onAfterWorldMatrixUpdateObservable.add(matrixWorldDidUpdate as any)
  }

  removeUUIDEvent(type: IEventNames): void {
    this.uuidEvents.delete(type)
  }

  addUUIDEvent(type: IEventNames, uuid: string): void {
    this.uuidEvents.set(type, uuid)
  }

  attachDisposableComponent(name: string, component: DisposableComponent) {
    const current = this.disposableComponents.get(name)
    if (current && current !== component) {
      current.removeFrom(this)
    }
    component.attachTo(this)
    this.disposableComponents.set(name, component)
  }

  getLoadingEntity(): BaseEntity | null {
    if (!this.loadingDone) {
      return this
    }

    for (let [, component] of this.disposableComponents) {
      if (!component.loadingDone(this)) {
        return this
      }
    }

    for (let child of this.childEntities()) {
      const loadingEntity = child.getLoadingEntity()
      if (loadingEntity) {
        return loadingEntity
      }
    }

    return null
  }

  setParent(a: BABYLON.TransformNode): BABYLON.TransformNode {
    const e = new Error('Cannot call setParent in baseEntity, use setParentEntity instead')
    this.context.logger.error('setParent', e)
    throw e
  }

  setParentEntity(newParent: BaseEntity | null): void {
    const currentParent = this.parentEntity

    if (currentParent === newParent) {
      return
    }

    if (currentParent) {
      currentParent.removeEntity(this)
    }

    if (newParent) {
      if (!(newParent instanceof BaseEntity)) {
        throw new Error('setParentEntity called with non entity')
      }

      if (newParent.children.indexOf(this) === -1) {
        newParent.children.push(this)
      }
    }

    super.parent = newParent

    return
  }

  removeEntity(entity: BaseEntity) {
    const ix = this.children.indexOf(entity)
    if (ix !== -1) {
      this.children.splice(ix, 1)
    }
  }

  getObject3D(type: string) {
    return this.object3DMap[type]
  }

  /**
   * Set a BABYLON.Mesh into the map.
   *
   * @param {string} type - Developer-set name of the type of object, will be unique per type.
   * @param {BABYLON.TransformNode} obj - A .
   */
  setObject3D(type: string, obj: BABYLON.TransformNode | BaseEntity) {
    // Remove existing object of the type.
    let oldObj = this.getObject3D(type)

    if (oldObj) {
      if ('setParentEntity' in oldObj) {
        oldObj.setParentEntity(null)
        oldObj.disposeTree()
      } else {
        oldObj.parent = null
        oldObj.dispose()
      }
    }

    if (!obj) return

    if (!(obj instanceof BABYLON.TransformNode)) {
      throw new Error('`Entity.setObject3D` was called with an object that was not an instance of BABYLON.Mesh.')
    }

    // Add.

    if ('setParentEntity' in obj) {
      obj.setParentEntity(this)
    } else {
      obj.parent = this
    }

    this.object3DMap[type] = obj

    this.onChangeObject3DObservable.notifyObservers({
      type,
      object: obj
    })

    if (type === BasicShape.nameInEntity) {
      this.sendUpdatePositions()
      this.sendUpdateMetrics()
    }
  }

  /**
   * This method is called after processing an input. It first searches
   * for a collision mesh, then it traverses up the tree until it finds an entity
   * (this entity) and then it calls this method.
   */
  handleClick(pointerEvent: 'pointerUp' | 'pointerDown', pointerId: number, pickingResult: BABYLON.PickingInfo): any {
    if (pointerEvent === 'pointerDown') {
      this.dispatchUUIDEvent('onClick', { entityId: this.uuid })
    }
    this.context.sendPointerEvent(pointerEvent, pointerId, this.uuid, pickingResult)
  }

  /**
   * Remove object from scene and entity Object3D map.
   */
  removeObject3D(type: string, dispose: boolean = true) {
    let obj = this.getObject3D(type)
    if (!obj) {
      return
    }

    if ('setParentEntity' in obj) {
      obj.setParentEntity(null)
      if (dispose) obj.disposeTree()
    } else {
      obj.parent = null
      if (dispose) obj.dispose()
    }

    delete this.object3DMap[type]

    this.onChangeObject3DObservable.notifyObservers({
      type,
      object: null
    })

    if (type === BasicShape.nameInEntity) {
      this.sendUpdatePositions()
      this.sendUpdateMetrics()
    }
  }

  /**
   * Removes an attribute, if a component is attached to the attribute, it also tear down the component.
   */
  removeComponentByName(name: string) {
    const current = this.disposableComponents.get(name)

    if (current) {
      current.removeFrom(this)
      this.disposableComponents.delete(name)
    }

    this.removeUUIDEvent(name as any)

    if (name in this.attrs) {
      delete this.attrs[name]
    }
    if (name in this.components) {
      this.removeBehavior(this.components[name])
      delete this.components[name]
    }
  }

  /**
   * Returns the children that extends BaseEntity, filtering any othewr Object3D
   */
  childEntities(): Array<BaseEntity> {
    const ret = []
    for (let i = 0; i < this.children.length; i++) {
      const element = this.children[i]
      if (element.isDCLEntity) {
        ret.push(element)
      }
    }
    return ret
  }

  toJSON(): any {
    return {
      id: this.id,
      components: this.attrs ? Object.keys(this.attrs) : [],
      children: this.children.map($ => $.toJSON()),
      disposed: this._isDisposed
    }
  }

  disposeTree(map?: Map<string, BaseEntity>) {
    for (const E of this.childEntities()) {
      E.disposeTree(map)
    }
    this.dispose()
    if (map) {
      map.delete(this.uuid)
    }
  }

  dispose() {
    if (this._isDisposed) {
      return
    }

    const parent = this.parent

    if (parent) {
      this.setParentEntity(null)
    }

    if (this.children.length) {
      error(`Warning, disposing an entity with children. This should not happen`)
    }

    for (let type in this.object3DMap) {
      this.removeObject3D(type)
    }

    this.disposableComponents.forEach($ => $.removeFrom(this))
    this.disposableComponents.clear()
    this.events.clear()
    this.uuidEvents.clear()

    this.parent = null
    super.setParent(null)

    // Remove the components, behaviors and other stuff
    super.dispose(true, false)

    if (this.context) {
      this.context.entities.delete(this.uuid)
      delete this.context
    }

    delete this.attrs
    delete this.components

    if (this.onChangeObject3DObservable) {
      this.onChangeObject3DObservable.clear()
    }

    scene.removeTransformNode(this)
  }

  /**
   * Pre-order traversing of the entity tree.
   * @param fn the delegated function used to traverse.
   */
  traverse(fn: (entity: BaseEntity) => void) {
    if (!this.isDisposed()) {
      fn(this)

      for (let child of this.children) {
        if (!child.isDisposed()) {
          child.traverse(fn)
        }
      }
    }
  }

  /**
   * Pre-order traversing of the entity tree.
   * It stops traversing the branch if the result of the function === 'BREAK'
   * @param fn the delegated function used to traverse.
   */
  traverseControl(fn: (entity: BaseEntity) => 'BREAK' | 'CONTINUE') {
    if (!this.isDisposed()) {
      if (fn(this) === 'BREAK') return

      for (let child of this.children) {
        if (!child.isDisposed()) {
          child.traverseControl(fn)
        }
      }
    }
  }

  dispatchUUIDEvent<T extends IEventNames>(event: T, data: IEvents[T]) {
    const uuid = this.uuidEvents.get(event)

    if (uuid) {
      this.context.dispatchUUIDEvent(uuid, data)
    }

    const listenerList = this.events.get(event)
    if (listenerList && listenerList.length) {
      listenerList.forEach($ => $(data))
    }
  }

  addListener<T extends IEventNames>(event: T, fn: (data: IEvents[T]) => void) {
    let listenerList = this.events.get(event)
    if (!listenerList) {
      listenerList = []
      this.events.set(event, listenerList)
    }
    listenerList.push(fn)
  }

  removeListener<T extends IEventNames>(event: T, fn: (data: IEvents[T]) => void) {
    let listenerList = this.events.get(event)
    if (listenerList) {
      const ix = listenerList.indexOf(fn)
      if (ix !== -1) {
        listenerList.splice(ix, 1)
      }
    }
  }

  updateComponent(payload: UpdateEntityComponentPayload) {
    const name = payload.name
    this.attrs[name] = payload

    if (payload.classId === CLASS_ID.UUID_CALLBACK) {
      const uuidPayload: { type: IEventNames; uuid: string } = JSON.parse(payload.json)
      this.addUUIDEvent(uuidPayload.type, uuidPayload.uuid)
    } else if (name in this.components) {
      this.components[name].setValue(JSON.parse(payload.json))
    } else if (payload.classId in componentRegistry) {
      const behavior: BaseComponent<any> = new componentRegistry[payload.classId](this)
      behavior.setValue(JSON.parse(payload.json))
      this.components[name] = behavior
      this.addBehavior(behavior, true)
    }
  }

  sendUpdatePositions = () => {
    this.sendPositionsPending = false
    if (!this._isDisposed) {
      this.previousWorldMatrix.copyFrom(this._worldMatrix)
      this.context.onEntityMatrixChangedObservable.notifyObservers(this)
    }
  }

  sendUpdateMetrics() {
    this.context && this.context.updateMetrics()
  }

  getMeshesBoundingBox() {
    let children = this.getChildMeshes(false)
    let boundingInfo = children[0].getBoundingInfo()
    let min = boundingInfo.boundingBox.minimumWorld
    let max = boundingInfo.boundingBox.maximumWorld
    for (let i = 1; i < children.length; i++) {
      boundingInfo = children[i].getBoundingInfo()
      min = BABYLON.Vector3.Minimize(min, boundingInfo.boundingBox.minimumWorld)
      max = BABYLON.Vector3.Maximize(max, boundingInfo.boundingBox.maximumWorld)
    }
    return new BABYLON.BoundingInfo(min, max)
  }
}

/**
 * Finds the closest parent that is or extends a BaseEntity
 * @param object the object to start looking
 */
export function findParentEntity(object: BABYLON.Node): BaseEntity | null {
  return findParentEntityOfType(object, BaseEntity)
}

/**
 * Finds the closest parent that is instance of the second parameter (constructor)
 * @param object the object to start looking
 * @param desiredClass the constructor of the kind of parent we want to find
 */
export function findParentEntityOfType<T extends BaseEntity>(
  object: BABYLON.Node,
  desiredClass: ConstructorOf<T>
): T | null {
  // Find the next entity parent to dispatch the event
  let parent: T | BABYLON.Node | null = object.parent

  while (parent && !(parent instanceof desiredClass)) {
    parent = parent.parent

    // If the element has no parent, stop execution
    if (!parent) return null
  }

  return ((parent as any) as T) || null
}
