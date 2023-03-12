import defaultLogger from 'lib/logger'
import { getSceneWorkerBySceneID, getSceneWorkerBySceneNumber } from 'shared/world/parcelSceneManager'

export function handleSceneEvent(data: { sceneId: string; sceneNumber: number; eventType: string; payload: any }) {
  const scene = data.sceneNumber ? getSceneWorkerBySceneNumber(data.sceneNumber) : getSceneWorkerBySceneID(data.sceneId)

  if (scene) {
    scene.rpcContext.sendSceneEvent(data.eventType as IEventNames, data.payload)

    // Keep backward compatibility with old scenes using deprecated `pointerEvent`
    if (data.eventType === 'actionButtonEvent') {
      const { payload } = data.payload
      // CLICK, PRIMARY or SECONDARY
      if (payload.buttonId >= 0 && payload.buttonId <= 2) {
        scene.rpcContext.sendSceneEvent('pointerEvent', data.payload)
      }
    }
  } else {
    if (data.eventType !== 'metricsUpdate') {
      if (data.sceneId) {
        defaultLogger.error(`SceneEvent: Scene id ${data.sceneId} not found`, data)
      } else {
        defaultLogger.error(`SceneEvent: Scene number ${data.sceneNumber} not found`, data)
      }
    }
  }
}
