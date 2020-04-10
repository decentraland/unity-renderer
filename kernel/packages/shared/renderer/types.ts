export const RENDERER_INITIALIZED = 'Renderer initialized'
export const PARCEL_LOADING_STARTED = 'Parcel loading started'

export type RendererState = {
  initialized: boolean
}

export type RootRendererState = {
  renderer: RendererState
}
