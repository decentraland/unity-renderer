import { store } from 'shared/store/isolatedStore'
import { RendererModules, RootRendererState } from './types'

export function isRendererInitialized(state: RootRendererState) {
  return state && state.renderer && state.renderer.initialized
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
  const modules = getRendererModules(store.getState())
  if (modules) {
    return modules
  }

  return new Promise<RendererModules>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const modules = getRendererModules(store.getState())
      if (modules) {
        unsubscribe()
        return resolve(modules)
      }
    })
  })
}
