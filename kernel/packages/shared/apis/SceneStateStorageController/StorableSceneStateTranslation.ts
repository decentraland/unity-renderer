import { CLASS_ID } from 'decentraland-ecs/src'
import { SceneStateDefinition } from 'scene-system/stateful-scene/SceneStateDefinition'
import { Component } from 'scene-system/stateful-scene/types'
import { uuid } from 'decentraland-ecs/src/ecs/helpers'
import { BuilderComponent, BuilderEntity, BuilderManifest, BuilderScene, SerializedSceneState } from './types'

const CURRENT_SCHEMA_VERSION = 1

export type StorableSceneState = {
  schemaVersion: number
  entities: StorableEntity[]
}

type StorableEntity = {
  id: string
  components: StorableComponent[]
}

type StorableComponent = {
  type: string
  value: any
}

export function toBuilderFromStateDefinitionFormat(
  scene: SceneStateDefinition,
  builderManifest: BuilderManifest
): BuilderManifest {
  let entities: Record<string, BuilderEntity> = {}
  let builderComponents: Record<string, BuilderComponent> = {}

  // Iterate every entity to get the components for builder
  for (const [entityId, components] of scene.getState().entries()) {
    let builderComponentsIds: string[] = []

    //Iterate the entity components to transform them to the builder format
    const mappedComponents = Array.from(components.entries()).map(([componentId, data]) => ({ componentId, data }))
    for (let component of mappedComponents) {
      //We generate a new uuid for the component since there is no uuid for components in the stateful scheme
      let newId = uuid()

      let componentType = toHumanReadableType(component.componentId)
      builderComponentsIds.push(newId)

      //we add the component to the builder format
      let builderComponent: BuilderComponent = {
        id: newId,
        type: componentType,
        data: component.data
      }
      builderComponents[builderComponent.id] = builderComponent
    }

    //we add the entity to builder format
    let builderEntity: BuilderEntity = {
      id: entityId,
      components: builderComponentsIds
    }
    entities[builderEntity.id] = builderEntity
  }

  // We create the scene and add it to the manifest
  const sceneState: BuilderScene = {
    id: builderManifest.scene.id,
    entities: entities,
    components: builderComponents,
    assets: builderManifest.scene.assets,
    metrics: builderManifest.scene.metrics,
    limits: builderManifest.scene.limits,
    ground: builderManifest.scene.ground
  }

  builderManifest.scene = sceneState
  return builderManifest
}

export function fromBuildertoStateDefinitionFormat(scene: BuilderScene): SceneStateDefinition {
  const sceneState = new SceneStateDefinition()

  const componentMap = new Map(Object.entries(scene.components))

  for (let entity of Object.values(scene.entities)) {
    let components: Component[] = []
    for (let componentId of entity.components.values()) {
      if (componentMap.has(componentId)) {

        let component: Component = {
          componentId: fromHumanReadableType(componentMap.get(componentId)!.type),
          data: componentMap.get(componentId)?.data
        }
        components.push(component)
      }
    }

    sceneState.addEntity(entity.id, components)
  }
  return sceneState
}

export function fromSerializedStateToStorableFormat(state: SerializedSceneState): StorableSceneState {
  const entities = state.entities.map(({ id, components }) => ({
    id,
    components: components.map(({ type, value }) => ({ type: toHumanReadableType(type), value }))
  }))
  return {
    schemaVersion: CURRENT_SCHEMA_VERSION,
    entities
  }
}

export function fromStorableFormatToSerializedState(state: StorableSceneState): SerializedSceneState {
  const entities = state.entities.map(({ id, components }) => ({
    id,
    components: components.map(({ type, value }) => ({ type: fromHumanReadableType(type), value }))
  }))
  return { entities }
}

/**
 * We are converting from numeric ids to a more human readable format. It might make sense to change this in the future,
 * but until this feature is stable enough, it's better to store it in a way that it is easy to debug.
 */

const HUMAN_READABLE_TO_ID: Map<string, number> = new Map([
  ['Transform', CLASS_ID.TRANSFORM],
  ['GLTFShape', CLASS_ID.GLTF_SHAPE],
  ['NFTShape', CLASS_ID.NFT_SHAPE],
  ['Name', CLASS_ID.NAME],
  ['LockedOnEdit', CLASS_ID.LOCKED_ON_EDIT],
  ['VisibleOnEdit', CLASS_ID.VISIBLE_ON_EDIT]
])

function toHumanReadableType(type: number): string {
  const humanReadableType = Array.from(HUMAN_READABLE_TO_ID.entries())
    .filter(([, componentId]) => componentId === type)
    .map(([type]) => type)[0]
  if (!humanReadableType) {
    throw new Error(`Unknown type ${type}`)
  }
  return humanReadableType
}

function fromHumanReadableType(humanReadableType: string): number {
  const type = HUMAN_READABLE_TO_ID.get(humanReadableType)
  if (!type) {
    throw new Error(`Unknown human readable type ${humanReadableType}`)
  }
  return type
}
