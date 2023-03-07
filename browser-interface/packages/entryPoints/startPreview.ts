import { sdk } from '@dcl/schemas'
import { getClientDebug } from 'unity-interface/ClientDebug'
import type { IUnityInterface } from 'unity-interface/IUnityInterface'
import { DEBUG_WS_MESSAGES } from 'config/index'
import { getPreviewSceneId } from 'unity-interface/initialScenes/loadPreviewScene'
import { loadPreviewScene } from 'unity-interface/initialScenes/loadPreviewScene'
import { reloadPlaygroundScene } from 'unity-interface/initialScenes/reloadPlaygroundScene'
import { logger } from './logger'

export async function startPreview(unityInterface: IUnityInterface) {
  try {
    const sceneData = await getPreviewSceneId()
    if (sceneData.sceneId) {
      unityInterface.SetKernelConfiguration({
        debugConfig: {
          sceneDebugPanelTargetSceneId: sceneData.sceneId,
          sceneLimitsWarningSceneId: sceneData.sceneId
        }
      })
      getClientDebug()
        .ToggleSceneBoundingBoxes(sceneData.sceneId, false)
        .catch((e) => logger.error(e))
      unityInterface.SendMessageToUnity('Main', 'TogglePreviewMenu', JSON.stringify({ enabled: true }))
    }
  } catch (err) {
    logger.info('Warning: cannot get preview scene id', err)
  }

  function handleServerMessage(message: sdk.Messages) {
    if (DEBUG_WS_MESSAGES) {
      logger.info('Message received: ', message)
    }
    if (message.type === sdk.UPDATE || message.type === sdk.SCENE_UPDATE) {
      void loadPreviewScene(message)
    }
  }

  const ws = new WebSocket(`${location.protocol === 'https:' ? 'wss' : 'ws'}://${document.location.host}`)

  ws.addEventListener('message', (msg) => {
    if (msg.data.startsWith('{')) {
      logger.log('Update message from CLI', msg.data)
      const message: sdk.Messages = JSON.parse(msg.data)
      handleServerMessage(message)
    }
  })

  window.addEventListener('message', async (msg) => {
    if (typeof msg.data === 'string') {
      if (msg.data === 'sdk-playground-update') {
        await reloadPlaygroundScene()
      }
    }
  })
}
