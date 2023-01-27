import 'unity-interface/UnityInterface'
import { store } from 'shared/store/isolatedStore'
import { BrowserInterface, browserInterface } from 'unity-interface/BrowserInterface'
import { getUnityInstance, IUnityInterface } from 'unity-interface/IUnityInterface'
import { isRendererInitialized } from './selectors'

export type RendererInterfaces = {
  unityInterface: IUnityInterface
  browserInterface: BrowserInterface
}

export async function ensureUnityInterface(): Promise<RendererInterfaces> {
  if (isRendererInitialized(store.getState())) {
    return { unityInterface: getUnityInstance(), browserInterface }
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      if (isRendererInitialized(store.getState())) {
        unsubscribe()
        return resolve({ unityInterface: getUnityInstance(), browserInterface })
      }
    })
  })
}
