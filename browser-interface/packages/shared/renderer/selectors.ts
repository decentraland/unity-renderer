import { storeCondition } from 'lib/redux'
import type { RendererModules, RootRendererState } from './types'

export function isRendererInitialized(state: RootRendererState) {
  return state && state.renderer && state.renderer.initialized
}

export function isAvatarSceneInitialized(state: RootRendererState) {
  return state && state.renderer && state.renderer.avatarSceneInitialized
}

export function getParcelLoadingStarted(state: RootRendererState) {
  return state && state.renderer && state.renderer.parcelLoadingStarted
}

export function getClientPort(state: RootRendererState) {
  return state && state.renderer && state.renderer.clientPort
}

export function getClient(state: RootRendererState) {
  return state && state.renderer && state.renderer.rpcClient
}

export function getRendererModules(state: RootRendererState): RendererModules | undefined {
  return state && state.renderer && state.renderer.modules
}

export async function ensureRendererModules(): Promise<RendererModules> {
  return (await storeCondition(getRendererModules))!
}
