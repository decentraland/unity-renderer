import { action } from 'typesafe-actions'

import { UnityGame } from 'unity-interface/loader'

import { RENDERER_INITIALIZED, PARCEL_LOADING_STARTED, ENGINE_STARTED } from './types'

export const INITIALIZE_RENDERER = '[Request] Initialize renderer'
export const initializeRenderer = (
  delegate: (container: HTMLElement, onMessage: (type: string, payload: string) => void) => Promise<UnityGame>,
  container: HTMLElement
) => action(INITIALIZE_RENDERER, { delegate, container })
export type InitializeRenderer = ReturnType<typeof initializeRenderer>

export const engineStarted = () => action(ENGINE_STARTED)
export type EngineStarted = ReturnType<typeof engineStarted>

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)
export type SignalParcelLoadingStarted = ReturnType<typeof signalParcelLoadingStarted>
