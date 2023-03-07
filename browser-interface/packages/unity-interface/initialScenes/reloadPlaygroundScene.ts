import { unloadParcelSceneById } from 'shared/world/parcelSceneManager'
import { ContentMapping } from '@dcl/schemas'
import { sleep } from 'lib/javascript/sleep'
import { startGlobalScene } from './startGlobalScene'

export async function reloadPlaygroundScene() {
  const playgroundCode: string = (globalThis as any).PlaygroundCode
  const playgroundContentMapping: ContentMapping[] = (globalThis as any).PlaygroundContentMapping || []
  const playgroundBaseUrl: string = (globalThis as any).PlaygroundBaseUrl || location.origin

  if (!playgroundCode) {
    console.log('There is no playground code')
    return
  }

  const sceneId = 'dcl-sdk-playground'

  await unloadParcelSceneById(sceneId)
  await sleep(300)

  const hudWorkerBLOB = new Blob([playgroundCode])
  const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)
  await startGlobalScene(sceneId, 'SDK Playground', hudWorkerUrl, {
    content: playgroundContentMapping,
    baseUrl: playgroundBaseUrl,
    ecs7: true
  })
}
