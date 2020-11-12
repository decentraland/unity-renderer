import { CLASS_ID } from "decentraland-ecs/src";
import { SceneStateDefinition } from "./SceneStateDefinition";
import { Component, ComponentId, EntityId } from "./types";

export class SceneStateDefinitionSerializer {

  static toStorableFormat(state: SceneStateDefinition): any {
    const entities = []
    for (const [entityId, entityComponents] of state.getState().entries()) {
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

const HUMAN_READABLE_TO_ID: Map<string, ComponentId> = new Map([
  ['Transform', CLASS_ID.TRANSFORM],
  ['GLTFShape', CLASS_ID.GLTF_SHAPE],
  ['NFTShape', CLASS_ID.NFT_SHAPE],
  ['Name', CLASS_ID.NAME]
])

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