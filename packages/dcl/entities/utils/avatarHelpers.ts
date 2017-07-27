import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { uuid } from 'atomicHelpers/math'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { CLASS_ID } from 'decentraland-ecs/src'
import { GLTFShape } from 'engine/components/disposableComponents/GLTFShape'

export const avatarTypes = new Set(['fox', 'round-robot', 'square-robot'])

export function getGLTFModelInContext(src: string, context: SharedSceneContext) {
  for (let [id, component] of context.disposableComponents) {
    if (component instanceof GLTFShape) {
      if (component.src === src) {
        return id
      }
    }
  }

  const gltfId = uuid()

  context.ComponentCreated({
    id: gltfId,
    classId: CLASS_ID.GLTF_SHAPE,
    name: 'shape'
  })

  context.ComponentUpdated({
    id: gltfId,
    json: JSON.stringify({ src })
  })

  return gltfId
}

export function createGltfChild(src: string, context: SharedSceneContext) {
  const entityId = uuid()
  const gltfId = getGLTFModelInContext(src, context)
  const ret = new BaseEntity(entityId, context)

  context.AttachEntityComponent({
    name: 'shape',
    id: gltfId,
    entityId
  })

  return Object.assign(ret, { gltfId })
}

export function loadAvatarModel(avatarName: string, context: SharedSceneContext) {
  const head = createGltfChild(`models/avatar/${avatarName}/head.glb`, context)
  const body = createGltfChild(`models/avatar/${avatarName}/body.glb`, context)
  const leftHand = createGltfChild(`models/avatar/${avatarName}/left-hand.glb`, context)
  const rightHand = createGltfChild(`models/avatar/${avatarName}/right-hand.glb`, context)

  return { head, body, leftHand, rightHand }
}
