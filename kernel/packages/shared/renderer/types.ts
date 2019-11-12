export const RENDERER_INITIALIZED = 'Renderer initialized'

export type RendererState = {
  initialized: boolean
}

export type RootRendererState = {
  renderer: RendererState
}
