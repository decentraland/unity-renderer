import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { RootSceneLoaderState } from './types'

export const getSceneLoader = (state: RootSceneLoaderState) => state.sceneLoader.loader
export const getParcelPosition = (state: RootSceneLoaderState) => state.sceneLoader.parcelPosition
export const getPositionSettled = (state: RootSceneLoaderState) => state.sceneLoader.positionSettled
export const getParcelPositionAsString = (state: RootSceneLoaderState) =>
  encodeParcelPosition(state.sceneLoader.parcelPosition)
export const getLoadingRadius = (state: RootSceneLoaderState) => state.sceneLoader.loadingRadius
export const isPositionSettled = (state: RootSceneLoaderState) => state.sceneLoader.positionSettled
export const getPositionSpawnPointAndScene = (state: RootSceneLoaderState) => ({
  sceneId: state.sceneLoader.positionSettlerSceneId,
  spawnPoint: state.sceneLoader.spawnPoint
})
