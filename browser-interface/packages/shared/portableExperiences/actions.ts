import { LoadableScene } from 'shared/types'
import { action } from 'typesafe-actions'

export const UPDATE_ENGINE_PX = '[PX] UpdateEngine'
export const updateEnginePortableExperiences = (desiredPortableExperiences: LoadableScene[]) =>
  action(UPDATE_ENGINE_PX, { desiredPortableExperiences })
export type UpdateEnginePortableExperiencesAction = ReturnType<typeof updateEnginePortableExperiences>

export const DENY_PORTABLE_EXPERIENCES = '[PX] SetDenyList'
export const denyPortableExperiences = (urnList: string[]) => action(DENY_PORTABLE_EXPERIENCES, { urnList })
export type DenyPortableExperiencesAction = ReturnType<typeof denyPortableExperiences>

export const SHUTDOWN_ALL_PORTABLE_EXPERIENCES = '[PX] ShutDownAll'
export const killAllPortableExperiences = () => action(SHUTDOWN_ALL_PORTABLE_EXPERIENCES, {})
export type KillAllPortableExperiencesAction = ReturnType<typeof killAllPortableExperiences>

export const ACTIVATE_ALL_PORTABLE_EXPERIENCES = '[PX] ActivateAll'
export const activateAllPortableExperiences = () => action(ACTIVATE_ALL_PORTABLE_EXPERIENCES, {})
export type ActivateAllPortableExperiencesAction = ReturnType<typeof activateAllPortableExperiences>

export const ADD_KERNEL_PX = '[PX] AddKernelPx'
export const addKernelPortableExperience = (data: LoadableScene) => action(ADD_KERNEL_PX, { data })
export type AddKernelPortableExperience = ReturnType<typeof addKernelPortableExperience>

export const ADD_SCENE_PX = '[PX] AddScenePx'
export const addScenePortableExperience = (data: LoadableScene) => action(ADD_SCENE_PX, { data })
export type AddScenePortableExperienceAction = ReturnType<typeof addScenePortableExperience>

export const RELOAD_SCENE_PX = '[PX] ReloadScenePx'
export const reloadScenePortableExperience = (data: LoadableScene) => action(RELOAD_SCENE_PX, { data })
export type ReloadScenePortableExperienceAction = ReturnType<typeof reloadScenePortableExperience>

export const REMOVE_SCENE_PX = '[PX] RemoveScenePx'
export const removeScenePortableExperience = (urn: string) => action(REMOVE_SCENE_PX, { urn })
export type RemoveScenePortableExperienceAction = ReturnType<typeof removeScenePortableExperience>

export type PortableExperienceActions =
  | UpdateEnginePortableExperiencesAction
  | DenyPortableExperiencesAction
  | AddScenePortableExperienceAction
  | ReloadScenePortableExperienceAction
  | RemoveScenePortableExperienceAction
  | KillAllPortableExperiencesAction
  | ActivateAllPortableExperiencesAction
  | AddKernelPortableExperience
