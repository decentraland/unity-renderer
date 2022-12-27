import { RpcClientPort } from '@dcl/rpc'

export const RENDERER_INITIALIZED_CORRECTLY = '[RENDERER] Renderer initialized correctly'
export const PARCEL_LOADING_STARTED = '[RENDERER] Parcel loading started'
export const RENDERER_INITIALIZE = '[RENDERER] Initializing'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesRendererServiceDefinition } from '@dcl/protocol/out-ts/decentraland/renderer/renderer_services/emotes_renderer.gen'

export type RendererState = {
  initialized: boolean
  parcelLoadingStarted: boolean
  clientPort: RpcClientPort | undefined
  modules: RendererModules | undefined
}

export type RendererModules = {
  emotes: codegen.RpcClientModule<EmotesRendererServiceDefinition, any> | undefined
}

export type RootRendererState = {
  renderer: RendererState
}
