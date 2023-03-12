import type { Vector2 } from 'lib/math/Vector2'
import type { InstancedSpawnPoint, LoadableScene } from 'shared/types'

export type SetDesiredScenesCommand = {
  scenes: LoadableScene[]
}

export type SceneLoaderPositionReport = {
  position: Vector2
  loadingRadius: number
  teleported: boolean
}

export interface ISceneLoader {
  reportPosition(positionReport: SceneLoaderPositionReport): Promise<SetDesiredScenesCommand>
  fetchScenesByLocation(parcels: string[]): Promise<SetDesiredScenesCommand>
  stop(): Promise<void>
}

export type SceneLoaderState = {
  loader: ISceneLoader | undefined
  positionSettled: boolean
  loadingRadius: number
  parcelPosition: Vector2

  // if positionSettled==true, once this scene loads, the player will be spawned
  // to that position
  positionSettlerSceneId?: string
  spawnPoint: InstancedSpawnPoint
}

export type RootSceneLoaderState = {
  sceneLoader: SceneLoaderState
}
