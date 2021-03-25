import { initShared } from 'shared'
import { USE_UNITY_INDEXED_DB_CACHE } from 'shared/meta/types'
import { initializeRenderer } from 'shared/renderer/actions'
import { StoreContainer } from 'shared/store/rootTypes'
import { rendererEnabled } from 'shared/renderer'
import { loadUnity } from './loader'

import { UnityInterfaceContainer } from './dcl'

declare const globalThis: StoreContainer & { Hls: any }
// HLS is required to make video texture and streaming work in Unity
globalThis.Hls = require('hls.js')

export type InitializeUnityResult = {
  container: HTMLElement
  instancedJS: Promise<UnityInterfaceContainer>
}

/** Initialize the engine in a container */
export async function initializeUnity(container: HTMLElement): Promise<InitializeUnityResult> {
  const urlParams = new URLSearchParams(document.location.search)

  const { buildConfigPath } = await loadUnity(urlParams.get('renderer') || undefined)

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
