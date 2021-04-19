export type EntityId = string
export type ComponentId = number
export type ComponentData = any
export type Component = {
  componentId: ComponentId
  data: ComponentData
}

/**
 * An object or entity that contains an updatable definition of the scene state
 */
export interface StateContainer {
  addEntity(entityId: EntityId, components?: Component[]): void
  removeEntity(entityId: EntityId): void
  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void
  removeComponent(entityId: EntityId, componentId: ComponentId): void
}

/**
 * An interface that listen to changes in the state of the scene
 */
export interface StateContainerListener {
  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void
  onRemoveEntity(listener: (entityId: EntityId) => void): void
  onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void
  onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void
}

/**
 * An actor that contains the state of the scene, but it can also generate updates to it
 */
export abstract class StatefulActor implements StateContainer {

  abstract addEntity(entityId: string, components?: Component[]): void
  abstract removeEntity(entityId: string): void
  abstract setComponent(entityId: string, componentId: number, data: any): void
  abstract removeComponent(entityId: string, componentId: number): void
}
