import { Component, ComponentData, ComponentId, EntityId, StateContainer } from "./types";

export class SceneStateDefinition implements StateContainer {

  private readonly entities: Map<EntityId, Map<ComponentId, ComponentData>> = new Map()

  addEntity(entityId: EntityId, components?: Component[]): void {
    const componentMap: Map<ComponentId, ComponentData> = new Map((components ?? []).map(({ componentId, data }) => [componentId, data]))
    this.entities.set(entityId, componentMap)
  }

  removeEntity(entityId: EntityId): void {
    this.entities.delete(entityId)
  }

  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void {
    const components = this.entities.get(entityId)
    if (!components) {
      this.entities.set(entityId, new Map([[componentId, data]]))
    } else {
      components.set(componentId, data)
    }
  }

  removeComponent(entityId: EntityId, componentId: ComponentId): void {
    this.entities.get(entityId)?.delete(componentId)
  }

  sendStateTo(container: StateContainer) {
    for (const [entityId, components] of this.entities.entries()) {
      const mappedComponents = Array.from(components.entries())
        .map(([componentId, data]) => ({ componentId, data }))
      container.addEntity(entityId, mappedComponents)
    }
  }

  /**
   * Returns a copy of the state
   */
  getState(): Map<EntityId, Map<ComponentId, ComponentData>> {
    const newEntries: [EntityId, Map<ComponentId, ComponentData>][] = Array.from(this.entities.entries())
      .map(([entityId, components]) => [entityId, new Map(components)])
    return new Map(newEntries)
  }

}
