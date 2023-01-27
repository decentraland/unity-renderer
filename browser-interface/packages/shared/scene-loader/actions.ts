import { InstancedSpawnPoint } from 'shared/types'
import { action } from 'typesafe-actions'
import { ISceneLoader } from './types'

export const SET_SCENE_LOADER = 'SET_SCENE_LOADER'
export const setSceneLoader = (loader: ISceneLoader | undefined) => action(SET_SCENE_LOADER, { loader })
export type SetSceneLoader = ReturnType<typeof setSceneLoader>

export const SET_WORLD_LOADING_RADIUS = 'SET WORLD LOADING RADIUS'
export const setWorldLoadingRadius = (radius: number) => action(SET_WORLD_LOADING_RADIUS, { radius })
export type SetWorldLoadingRadius = ReturnType<typeof setWorldLoadingRadius>

/**
 * Used to set the parcel position and to react to changes in the x,y
 */
export const SET_PARCEL_POSITION = 'SET_PARCEL_POSITION'
export const setParcelPosition = (position: ReadOnlyVector2) => action(SET_PARCEL_POSITION, { position })
export type SetParcelPosition = ReturnType<typeof setParcelPosition>

export const TELEPORT_TO = 'TELEPORT_TO'
export const teleportToAction = (spawnPoint: InstancedSpawnPoint) => action(TELEPORT_TO, spawnPoint)
export type TeleportToAction = ReturnType<typeof teleportToAction>

/**
 * Enables the renderer when the scene in which we are teleporting finishes
 * loading. The .position indicates the spawn point of the scene.
 */
export const POSITION_SETTLED = 'POSITION_SETTLED'
export const positionSettled = (spawnPoint: InstancedSpawnPoint) => action(POSITION_SETTLED, { spawnPoint })
export type PositionSettled = ReturnType<typeof positionSettled>

/**
 * Disables the renderer when we teleport to a specific place and the
 * scenes are not yet loaded.
 *
 * The sceneId is the ID of the scene that will settle the position once loaded.
 */
export const POSITION_UNSETTLED = 'POSITION_UNSETTLED'
export const positionUnsettled = (sceneId: string, spawnPoint: InstancedSpawnPoint) =>
  action(POSITION_UNSETTLED, { sceneId, spawnPoint })
export type PositionUnsettled = ReturnType<typeof positionUnsettled>
