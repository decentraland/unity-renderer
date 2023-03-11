import { now } from 'lib/javascript/now'
import defaultLogger from 'lib/logger'
import { updateStatusMessage } from 'shared/loading/actions'
import { getLastUpdateTime } from 'shared/loading/selectors'
import { store } from 'shared/store/isolatedStore'
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

const TIME_BETWEEN_SCENE_LOADING_UPDATES = 1_000
export function handleScenesLoadingFeedback(data: { message: string; loadPercentage: number }) {
  const { message, loadPercentage } = data
  const currentTime = now()
  const last = getLastUpdateTime(store.getState())
  const elapsed = currentTime - (last || 0)
  if (elapsed > TIME_BETWEEN_SCENE_LOADING_UPDATES) {
    store.dispatch(updateStatusMessage(message, loadPercentage, currentTime))
  }
}
