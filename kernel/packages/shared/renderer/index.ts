import { Store } from 'redux'

import { RootState, StoreContainer } from 'shared/store/rootTypes'

import { isInitialized } from './selectors'
import { UnityInterface } from "unity-interface/UnityInterface"

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

export function rendererEnabled(): Promise<void> {
  const store: Store<RootState> = globalThis.globalStore

  const instancedJS = store.getState().renderer.instancedJS
  if (instancedJS) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const instancedJS = store.getState().renderer.instancedJS
      if (instancedJS) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export async function ensureUnityInterface(): Promise<UnityInterface> {
  await rendererEnabled()
  return globalThis.globalStore.getState().renderer.instancedJS!.then(({ unityInterface }) => unityInterface)
}
