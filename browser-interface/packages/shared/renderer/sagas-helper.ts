import { waitFor } from 'lib/redux'
import { REGISTER_RPC_PORT } from './actions'
import { getClient, isAvatarSceneInitialized, isRendererInitialized } from './selectors'
import { AVATAR_SCENE_INITIALIZED, RENDERER_INITIALIZED_CORRECTLY } from './types'

export const waitForRendererInstance = waitFor(isRendererInitialized, RENDERER_INITIALIZED_CORRECTLY)

export const waitForAvatarSceneInitialized = waitFor(isAvatarSceneInitialized, AVATAR_SCENE_INITIALIZED)

export const waitForRendererRpcConnection = waitFor(getClient, REGISTER_RPC_PORT)
