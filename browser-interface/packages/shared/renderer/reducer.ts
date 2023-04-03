import { AnyAction } from 'redux'
import { RegisterRendererModules, RegisterRendererPort, REGISTER_RPC_MODULES, REGISTER_RPC_PORT } from './actions'
import {
  PARCEL_LOADING_STARTED,
  RendererState,
  RENDERER_INITIALIZED_CORRECTLY,
  AVATAR_SCENE_INITIALIZED
} from './types'

const INITIAL_STATE: RendererState = {
  initialized: false,
  parcelLoadingStarted: false,
  clientPort: undefined,
  rpcClient: undefined,
  modules: undefined,
  avatarSceneInitialized: false
}

export function rendererReducer(state?: RendererState, action?: AnyAction): RendererState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case RENDERER_INITIALIZED_CORRECTLY:
      return {
        ...state,
        initialized: true
      }
    case AVATAR_SCENE_INITIALIZED:
      return {
        ...state,
        avatarSceneInitialized: true
      }
    case REGISTER_RPC_PORT:
      const { payload } = action as RegisterRendererPort
      return {
        ...state,
        clientPort: payload.rendererInterfacePort,
        rpcClient: payload.rpcClient
      }
    case PARCEL_LOADING_STARTED:
      return {
        ...state,
        parcelLoadingStarted: true
      }
    case REGISTER_RPC_MODULES:
      return {
        ...state,
        modules: (action as RegisterRendererModules).payload.modules
      }
    default:
      return state
  }
}
