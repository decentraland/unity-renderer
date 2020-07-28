import { LifecycleManager, getServer } from '../manager'

export async function fetchSceneIds(tiles: string[]) {
  const server: LifecycleManager = getServer()
  const promises = server.getSceneIds(tiles)
  return Promise.all(promises)
}
