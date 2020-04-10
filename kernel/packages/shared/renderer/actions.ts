import { action } from 'typesafe-actions'
import { RENDERER_INITIALIZED, PARCEL_LOADING_STARTED } from './types'

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)
export type SignalParcelLoadingStarted = ReturnType<typeof signalParcelLoadingStarted>
