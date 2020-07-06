import { action } from 'typesafe-actions'

export const SCENE_LOAD = 'Loading scene'
export const SCENE_START = 'Started scene'
export const SCENE_FAIL = 'Failed scene'

export const signalSceneLoad = (sceneId: string) => action(SCENE_LOAD, sceneId)
export const signalSceneStart = (sceneId: string) => action(SCENE_START, sceneId)
export const signalSceneFail = (sceneId: string) => action(SCENE_FAIL, sceneId)

export type SceneLoad = ReturnType<typeof signalSceneLoad>
export type SceneStart = ReturnType<typeof signalSceneStart>
export type SceneFail = ReturnType<typeof signalSceneFail>

declare const global: any

export function globalSignalSceneLoad(sceneId: string) {
  global['globalStore'].dispatch(signalSceneLoad(sceneId))
}

export function globalSignalSceneStart(sceneId: string) {
  global['globalStore'].dispatch(signalSceneStart(sceneId))
}

export function globalSignalSceneFail(sceneId: string) {
  global['globalStore'].dispatch(signalSceneFail(sceneId))
}

export const UPDATE_STATUS_MESSAGE = 'Update status message'
export const updateStatusMessage = (message: string) => action(UPDATE_STATUS_MESSAGE, message)
export type UpdateStatusMessage = ReturnType<typeof updateStatusMessage>
