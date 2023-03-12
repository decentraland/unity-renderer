import { SceneWorker, SceneWorkerReadyState } from 'shared/world/SceneWorker'

export function getPendingAndCountableScenes(sceneWorkers: Map<string, SceneWorker>) {
  const pendingScenes = new Set<string>()
  let countableScenes = 0

  for (const [sceneId, { ready }] of Object.entries(sceneWorkers)) {
    const isPending = (ready & SceneWorkerReadyState.STARTED) === 0
    const failedLoading = (ready & SceneWorkerReadyState.LOADING_FAILED) !== 0
    countableScenes++

    if (isPending && !failedLoading) {
      pendingScenes.add(sceneId)
    }
  }

  return { pendingScenes, countableScenes }
}
