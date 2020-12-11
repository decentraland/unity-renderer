import { LifecycleManager, getServer } from '../manager'

export function reloadScene(sceneId: string) {
  const server: LifecycleManager = getServer()
  return server.reloadScene(sceneId)
}
