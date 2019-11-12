import { SceneWorker } from './SceneWorker'
import { ChatController } from '../apis/ChatController'
import { SocialController } from '../apis/SocialController'

// Create instances of non-public-controllers
export async function ensureUiApis(worker: SceneWorker) {
  const system = await worker.system
  system.getAPIInstance(ChatController)
  system.getAPIInstance(SocialController)
}
