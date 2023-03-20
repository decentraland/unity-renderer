import { waitFor } from 'lib/redux'
import { REGISTER_RPC_PORT } from './actions'
import { getClient, isRendererInitialized } from './selectors'
import { RENDERER_INITIALIZED_CORRECTLY } from './types'

export const waitForRendererInstance = waitFor(isRendererInitialized, RENDERER_INITIALIZED_CORRECTLY)

export const waitForRendererRpcConnection = waitFor(getClient, REGISTER_RPC_PORT)
