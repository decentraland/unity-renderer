import { LifecycleManager, getServer } from '../manager'

export async function fetchSceneJson(sceneIds: string[]) {
  const server: LifecycleManager = getServer()
  const lands = await Promise.all(sceneIds.map((sceneId) => server.getParcelData(sceneId)))
  return lands
}
