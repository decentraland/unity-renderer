import { action } from 'typesafe-actions'

export const SET_CURRENT_SCENE = 'SET_CURRENT_SCENE'
export const setCurrentScene = (currentScene: string | undefined, previousScene: string | undefined) =>
  action(SET_CURRENT_SCENE, { currentScene, previousScene })
export type SetCurrentScene = ReturnType<typeof setCurrentScene>

export const RENDERER_SIGNAL_SCENE_READY = 'RENDERER_SIGNAL_SCENE_READY'
/**
 * This action marks a scene "Ready". It is used to start the internal game loop
 * of each scene and to remove the loading screen.
 */
export const rendererSignalSceneReady = (sceneId: string, sceneNumber: number | undefined = undefined) =>
  action(RENDERER_SIGNAL_SCENE_READY, { sceneId, sceneNumber })
export type RendererSignalSceneReady = ReturnType<typeof rendererSignalSceneReady>
