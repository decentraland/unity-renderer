import { LoadableScene } from 'shared/types'
import { action } from 'typesafe-actions'

export const SCENE_LOAD = '[SCENE MANAGER] Loading scene'
export const SCENE_START = '[SCENE MANAGER] Started scene'
export const SCENE_UNLOAD = '[SCENE MANAGER] Unload scene'
export const SCENE_CHANGED = '[SCENE MANAGER] Scenes changed'
export const PENDING_SCENES = '[SCENE MANAGER] Pending count'

export const scenesChanged = () => action(SCENE_CHANGED)
export const signalSceneLoad = (scene: LoadableScene) => action(SCENE_LOAD, scene)
export const signalSceneStart = (scene: LoadableScene) => action(SCENE_START, scene)
export const signalSceneUnload = (scene: LoadableScene) => action(SCENE_UNLOAD, scene)
export const informPendingScenes = (pendingScenes: number, totalScenes: number) =>
  action(PENDING_SCENES, { pendingScenes, totalScenes })

export type SceneLoad = ReturnType<typeof signalSceneLoad>
export type SceneStart = ReturnType<typeof signalSceneStart>
export type SceneUnload = ReturnType<typeof signalSceneUnload>
export type InformPendingScenes = ReturnType<typeof informPendingScenes>

export const UPDATE_STATUS_MESSAGE = '[RENDERER] Update loading message'
export const updateStatusMessage = (message: string, loadPercentage: number, lastUpdate: number) =>
  action(UPDATE_STATUS_MESSAGE, { message, loadPercentage, lastUpdate })
export type UpdateStatusMessage = ReturnType<typeof updateStatusMessage>
