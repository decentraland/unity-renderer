import { action } from 'typesafe-actions'
import { RENDERER_INITIALIZED } from './types'

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>
