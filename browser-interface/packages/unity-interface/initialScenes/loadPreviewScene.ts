import { Scene, sdk } from '@dcl/schemas'
import { rootURLPreviewMode } from 'config'
import defaultLogger from 'lib/logger'
import { reloadScenePortableExperience } from 'shared/portableExperiences/actions'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { store } from 'shared/store/isolatedStore'
import { wearableToSceneEntity } from 'shared/wearablesPortableExperience/sagas'
import { reloadScene } from 'shared/world/parcelSceneManager'

export async function loadPreviewScene(message: sdk.Messages) {
  async function oldReload() {
    const { sceneId, sceneBase } = await getPreviewSceneId()
    if (sceneId) {
      await reloadScene(sceneId)
    } else {
      defaultLogger.log(`Unable to load sceneId of ${sceneBase}`)
      debugger
    }
  }

  if (message.type === sdk.SCENE_UPDATE && sdk.SceneUpdate.validate(message)) {
    if (message.payload.sceneType === sdk.ProjectType.PORTABLE_EXPERIENCE) {
      try {
        const { sceneId } = message.payload
        const url = `${rootURLPreviewMode()}/preview-wearables/${sceneId}`
        const collection: { data: any[] } = await (await fetch(url)).json()

        if (!!collection.data.length) {
          const wearable = collection.data[0]

          const entity = await wearableToSceneEntity(wearable, wearable.baseUrl)

          store.dispatch(reloadScenePortableExperience(entity))
        }
      } catch (err) {
        defaultLogger.error(`Unable to loader the preview portable experience`, message, err)
      }
    } else {
      if (message.payload.sceneId) {
        await reloadScene(message.payload.sceneId)
      } else {
        await oldReload()
      }
    }
  } else if (message.type === 'update') {
    defaultLogger.log(`Please update your CLI version to 3.9.0 or more.`, { message })
    await oldReload()
  } else {
    defaultLogger.log(`Unable to process message in loadPreviewScene`, { message })
  }
}

export async function getPreviewSceneId(): Promise<{ sceneId: string | null; sceneBase: string }> {
  const result = await fetch(new URL('scene.json?nocache=' + Math.random(), rootURLPreviewMode()).toString())

  if (result.ok) {
    const scene = (await result.json()) as Scene

    const scenes = await fetchScenesByLocation([scene.scene.base])
    if (!scenes.length) throw new Error('cant find scene ' + scene.scene.base)
    return { sceneId: scenes[0].id, sceneBase: scene.scene.base }
  } else {
    throw new Error('Could not load scene.json')
  }
}
