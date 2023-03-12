import type { RendererModules, RootRendererState } from './types'

export function isRendererInitialized(state: RootRendererState) {
  return state && state.renderer && state.renderer.initialized
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
