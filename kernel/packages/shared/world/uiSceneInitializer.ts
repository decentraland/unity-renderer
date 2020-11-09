import { SceneWorker } from './SceneWorker'
import { SocialController } from '../apis/SocialController'

// Create instances of non-public-controllers
export async function ensureUiApis(worker: SceneWorker) {
  await worker.getAPIInstance(SocialController)
}
