import { getSceneWorkerBySceneID, getSceneWorkerBySceneNumber } from 'shared/world/parcelSceneManager'
import { AvatarRendererMessage, AvatarRendererMessageType, AvatarRendererPositionMessage } from 'shared/types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'

export function* getInSceneAvatarsUserId(sceneId: string): Iterable<string> {
  for (const [userId, avatarData] of rendererAvatars) {
    if (avatarData.sceneId === sceneId) yield userId
  }
}

type RendererAvatarData = {
  sceneId?: string
  sceneNumber?: number
}

const rendererAvatars: Map<string, RendererAvatarData> = new Map<string, RendererAvatarData>()
// Tracks avatar state on the renderer side.
// Set if avatar has change the scene it's in or removed on renderer's side.
export function setRendererAvatarState(evt: AvatarRendererMessage) {
  const userId = evt.avatarShapeId

  if (evt.type === AvatarRendererMessageType.SCENE_CHANGED) {
    // If changed to a scene not loaded on renderer side (sceneId null or empty)
    // we handle it as removed. We will receive another event when the scene where the
    // avatar is in is loaded.
    if (!evt.sceneNumber) {
      handleRendererAvatarRemoved(userId)
      return
    }

    // Handle avatars spawning or moving to a scene already loaded by the renderer.
    handleRendererAvatarSceneChanged(evt)
  } else if (evt.type === AvatarRendererMessageType.REMOVED) {
    handleRendererAvatarRemoved(userId)
  }
}

function handleRendererAvatarSceneChanged(evt: AvatarRendererPositionMessage) {
  const avatarData: RendererAvatarData | undefined = rendererAvatars.get(evt.avatarShapeId)

  if (avatarData?.sceneId) {
    const selfUser = evt.avatarShapeId.toLowerCase() === getCurrentIdentity(store.getState())?.address.toLowerCase()
    if (!selfUser) {
      // this is handled by the scene-loader saga for the selfUser
      getSceneWorkerBySceneID(avatarData.sceneId)?.onLeave(evt.avatarShapeId, selfUser)
    }
  }

  const sceneWorker = getSceneWorkerBySceneNumber(evt.sceneNumber ?? 0)
  const sceneId = sceneWorker?.rpcContext.sceneData.id
  sceneWorker?.onEnter(evt.avatarShapeId)

  rendererAvatars.set(evt.avatarShapeId, { ...evt, sceneId })
}

function handleRendererAvatarRemoved(userId: string) {
  const avatarData: RendererAvatarData | undefined = rendererAvatars.get(userId)
  if (avatarData && avatarData.sceneId) {
    const sceneWorker = getSceneWorkerBySceneID(avatarData.sceneId)
    const selfUser = userId.toLowerCase() === getCurrentIdentity(store.getState())?.address.toLowerCase()
    if (!selfUser) {
      // this is handled by the scene-loader saga for the selfUser
      sceneWorker?.onLeave(userId, selfUser)
    }
    rendererAvatars.delete(userId)
  }
}
