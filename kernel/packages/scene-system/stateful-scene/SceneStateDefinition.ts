import { CLASS_ID } from "decentraland-ecs/src";
import { Component, ComponentData, ComponentId, EntityId, StateContainer } from "./types";

export class SceneStateDefinition implements StateContainer {

  private readonly entities: Map<EntityId, Map<ComponentId, ComponentData>> = new Map()

  addEntity(entityId: EntityId, components?: Component[]): void {
    const componentMap: Map<ComponentId, ComponentData> = new Map((components ?? []).map(({ id, data }) => [id, data]))
    this.entities.set(entityId, componentMap)
  }

  removeEntity(entityId: EntityId): void {
    this.entities.delete(entityId)
  }

  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void {
    this.entities.get(entityId)?.set(componentId, data)
  }

  removeComponent(entityId: EntityId, componentId: ComponentId): void {
    this.entities.get(entityId)?.delete(componentId)
  }

  sendStateTo(container: StateContainer) {
    for (const [entityId, components] of this.entities.entries()) {
      const mappedComponents = Array.from(components.entries())
        .map(([id, data]) => ({ id, data }))
      container.addEntity(entityId, mappedComponents)
    }
  }

  toStorableFormat(): any {
    const entities = []
    for (const [entityId, entityComponents] of this.entities.entries()) {
      const components = []
      for (const [componentId, componentData] of entityComponents.entries()) {
        components.push({ type: idToHumanReadableType(componentId), value: componentData })
      }
      entities.push({ id: entityId, components })
    }
    return { entities }
  }

  static fromStorableFormat(data: any): SceneStateDefinition {
    const sceneState = new SceneStateDefinition()
    for (const entity of data.entities) {
      const id: EntityId = entity.id
      const components: Component[] | undefined = entity.components
        ?.map((component: any) => ({
          id: humanReadableTypeToId(component.type),
          data: component.value,
        }))
      sceneState.addEntity(id, components)
    }
    return sceneState
  }
}


/**
 * We are converting from numeric ids to a more human readable format. It might make sense to change this in the future,
 * but until this feature is stable enough, it's better to store it in a way that it is easy to debug.
 */

const HUMAN_READABLE_TO_ID: Map<string, ComponentId> = new Map([['Transform', CLASS_ID.TRANSFORM], ['GLTFShape', CLASS_ID.GLTF_SHAPE]])

function idToHumanReadableType(id: ComponentId): string {
  const type = Array.from(HUMAN_READABLE_TO_ID.entries())
    .filter(([, componentId]) => componentId === id)
    .map(([type]) => type)[0]
  if (!type) {
    throw new Error(`Unknown id ${id}`)
  }
  return type
}

function humanReadableTypeToId(type: string): ComponentId {
  const componentId = HUMAN_READABLE_TO_ID.get(type)
  if (!componentId) {
    throw new Error(`Unknown type ${type}`)
  }
  return componentId
}