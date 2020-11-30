import { CLASS_ID } from 'decentraland-ecs/src'
import { SerializedSceneState } from './types'

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
