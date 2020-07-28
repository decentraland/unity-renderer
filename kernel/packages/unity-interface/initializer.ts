import { initShared } from 'shared'
import { USE_UNITY_INDEXED_DB_CACHE } from 'shared/meta/types'
import { initializeRenderer } from 'shared/renderer/actions'
import { StoreContainer } from 'shared/store/rootTypes'
import { rendererEnabled } from 'shared/renderer'

import { UnityInterfaceContainer } from './dcl'

declare const globalThis: StoreContainer

export type InitializeUnityResult = {
  container: HTMLElement
  instancedJS: Promise<UnityInterfaceContainer>
}

/** Initialize the engine in a container */
export async function initializeUnity(
  container: HTMLElement,
  buildConfigPath: string = 'unity/Build/unity.json'
): Promise<InitializeUnityResult> {
  initShared()

  globalThis.globalStore.dispatch(initializeRenderer(container, buildConfigPath))
  ;(window as any).USE_UNITY_INDEXED_DB_CACHE = USE_UNITY_INDEXED_DB_CACHE

  await rendererEnabled()

  const instancedJS = globalThis.globalStore.getState().renderer.instancedJS!

  return {
    container,
    instancedJS
  }
}
