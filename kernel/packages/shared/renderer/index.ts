import { Store } from 'redux'

import { RootState, StoreContainer } from 'shared/store/rootTypes'
import { browserInterface } from 'unity-interface/BrowserInterface'
import { RendererInterfaces } from 'unity-interface/dcl'
import { unityInterface } from 'unity-interface/UnityInterface'

import { isInitialized } from './selectors'

declare const globalThis: StoreContainer

export function rendererInitialized() {
  const store: Store<RootState> = globalThis.globalStore

  const initialized = isInitialized(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise<void>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isInitialized(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export async function ensureUnityInterface(): Promise<RendererInterfaces> {
  const store: Store<RootState> = globalThis.globalStore

  const { engineStarted } = store.getState().renderer

  if (engineStarted) {
    return { unityInterface, browserInterface }
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const { engineStarted } = store.getState().renderer
      if (engineStarted) {
        unsubscribe()
        return resolve({ unityInterface, browserInterface })
      }
    })
  })
}
