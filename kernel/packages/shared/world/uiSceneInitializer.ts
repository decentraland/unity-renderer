import { SceneWorker } from './SceneWorker'
import { SocialController } from '../apis/SocialController'

// Create instances of non-public-controllers
export async function ensureUiApis(worker: SceneWorker) {
  const system = await worker.system
  system.getAPIInstance(SocialController)
}
