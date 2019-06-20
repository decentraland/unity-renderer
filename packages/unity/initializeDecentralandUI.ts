import { hudWorkerUrl, SceneWorker } from '../shared/world/SceneWorker'
import { ensureUiApis } from '../shared/world/uiSceneInitializer'
import { loadedParcelSceneWorkers } from '../shared/world/parcelSceneManager'
import EngineInterface from './EngineInterface'
import { UnityScene } from './unityParcelScene'

export default async function initializeDecentralandUI(unityInterface: EngineInterface) {
  const id = 'dcl-ui-scene'
  const scene = new UnityScene(
    id,
    {
      baseUrl: location.origin,
      main: hudWorkerUrl,
      data: { id },
      id,
      mappings: []
    },
    unityInterface
  )

  const worker = new SceneWorker(scene)
  worker.persistent = true
  await ensureUiApis(worker)
  loadedParcelSceneWorkers.add(worker)
  unityInterface.CreateUIScene({ id: scene.unitySceneId, baseUrl: scene.data.baseUrl })
}
