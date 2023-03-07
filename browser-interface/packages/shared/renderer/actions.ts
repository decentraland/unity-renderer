import { action } from 'typesafe-actions'

import type { UnityGame } from '@dcl/unity-renderer/src/index'

import { RENDERER_INITIALIZED_CORRECTLY, PARCEL_LOADING_STARTED, RENDERER_INITIALIZE, RendererModules } from './types'
import { RpcClient, RpcClientPort, Transport } from '@dcl/rpc'

export const initializeRenderer = (
  delegate: (container: HTMLElement) => Promise<{ renderer: UnityGame; transport: Transport }>,
  container: HTMLElement
) => action(RENDERER_INITIALIZE, { delegate, container })
export type InitializeRenderer = ReturnType<typeof initializeRenderer>

export const REGISTER_RPC_PORT = 'REGISTER_RPC_PORT'
export const registerRendererPort = (rpcClient: RpcClient, rendererInterfacePort: RpcClientPort) =>
  action(REGISTER_RPC_PORT, { rpcClient, rendererInterfacePort })
export type RegisterRendererPort = ReturnType<typeof registerRendererPort>

export const signalRendererInitializedCorrectly = () => action(RENDERER_INITIALIZED_CORRECTLY)

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)

export const REGISTER_RPC_MODULES = 'REGISTER_RPC_MODULES'
export const registerRendererModules = (modules: RendererModules) => action(REGISTER_RPC_MODULES, { modules })
export type RegisterRendererModules = ReturnType<typeof registerRendererModules>

export const BEFORE_UNLOAD = 'BEFORE_UNLOAD'
export const beforeUnloadAction = () => action(BEFORE_UNLOAD)
