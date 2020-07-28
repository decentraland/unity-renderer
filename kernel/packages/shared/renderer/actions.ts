import { action } from 'typesafe-actions'

import { UnityInterfaceContainer } from 'unity-interface/dcl'

import { RENDERER_INITIALIZED, PARCEL_LOADING_STARTED } from './types'

export const INITIALIZE_RENDERER = '[Request] Initialize renderer'
export const initializeRenderer = (container: HTMLElement, buildConfigPath: string) =>
  action(INITIALIZE_RENDERER, { container, buildConfigPath })
export type InitializeRenderer = ReturnType<typeof initializeRenderer>

export const ENGINE_STARTED = '[Success] Engine started'
export const engineStarted = () => action(ENGINE_STARTED)
export type EngineStarted = ReturnType<typeof engineStarted>

export const RENDERER_ENABLED = '[Succes] Renderer enabled'
export const rendererEnabled = (instancedJS: Promise<UnityInterfaceContainer>) =>
  action(RENDERER_ENABLED, { instancedJS })
export type RendererEnabled = ReturnType<typeof rendererEnabled>

export const MESSAGE_FROM_ENGINE = '[Request] Message from Engine'
export const messageFromEngine = (type: string, jsonEncodedMessage: string) =>
  action(MESSAGE_FROM_ENGINE, { type, jsonEncodedMessage })
export type MessageFromEngineAction = ReturnType<typeof messageFromEngine>

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)
export type SignalParcelLoadingStarted = ReturnType<typeof signalParcelLoadingStarted>
