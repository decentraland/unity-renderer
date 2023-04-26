import { RpcClient, RpcClientPort } from '@dcl/rpc'

export const RENDERER_INITIALIZED_CORRECTLY = '[RENDERER] Renderer initialized correctly'
export const AVATAR_SCENE_INITIALIZED = '[RENDERER] Avatar initialized correctly'
export const PARCEL_LOADING_STARTED = '[RENDERER] Parcel loading started'
export const RENDERER_INITIALIZE = '[RENDERER] Initializing'
import * as codegen from '@dcl/rpc/dist/codegen'
import { RestrictedActionsServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/restricted_actions.gen'
import { EmotesRendererServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/emotes_renderer.gen'
import { FriendRequestRendererServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/friend_request_renderer.gen'

export type RendererState = {
  initialized: boolean
  parcelLoadingStarted: boolean
  clientPort: RpcClientPort | undefined
  rpcClient: RpcClient | undefined
  modules: RendererModules | undefined
  avatarSceneInitialized: boolean
}

export type RendererModules = {
  emotes: codegen.RpcClientModule<EmotesRendererServiceDefinition, any> | undefined
  friendRequest: codegen.RpcClientModule<FriendRequestRendererServiceDefinition, any> | undefined
  restrictedActions: codegen.RpcClientModule<RestrictedActionsServiceDefinition, any> | undefined
}

export type RootRendererState = {
  renderer: RendererState
}
