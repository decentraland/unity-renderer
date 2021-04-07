export const RENDERER_INITIALIZED = 'Renderer initialized'
export const PARCEL_LOADING_STARTED = 'Parcel loading started'
export const ENGINE_STARTED = '[Success] Engine started'

export type RendererState = {
  initialized: boolean
  engineStarted: boolean
}

export type RootRendererState = {
  renderer: RendererState
}
