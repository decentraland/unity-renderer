import type { IUnityInterface } from 'unity-interface/IUnityInterface'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { isRendererInitialized } from './selectors'
import { storeCondition } from 'lib/redux/storeCondition'

export async function getRendererInterface(): Promise<IUnityInterface> {
  await storeCondition(isRendererInitialized)
  return getUnityInstance()
}
