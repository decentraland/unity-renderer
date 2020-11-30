import { SerializedSceneState } from 'shared/apis/SceneStateStorageController/types'
import { SceneStateDefinition } from './SceneStateDefinition'
import { Component, EntityId } from './types'

export function serializeSceneState(state: SceneStateDefinition): SerializedSceneState {
  const entities = []
  for (const [entityId, entityComponents] of state.getState().entries()) {
    const components = []
    for (const [componentId, componentData] of entityComponents.entries()) {
      components.push({ type: componentId, value: componentData })
    }
    entities.push({ id: entityId, components })
  }
  return { entities }
}

export function deserializeSceneState(data: SerializedSceneState): SceneStateDefinition {
  const sceneState = new SceneStateDefinition()
  for (const entity of data.entities) {
    const id: EntityId = entity.id
    const components: Component[] | undefined = entity.components?.map((component: any) => ({
      componentId: component.type,
      data: component.value
    }))
    sceneState.addEntity(id, components)
  }
  return sceneState
}
