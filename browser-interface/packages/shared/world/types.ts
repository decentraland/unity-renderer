import { LoadableScene } from 'shared/types'

export type ParcelSceneLoadingState = {
  isWorldLoadingEnabled: boolean
  desiredParcelScenes: Map<string, LoadableScene>
}

export type WorldState = {
  currentScene: string | undefined
}

export type RootWorldState = {
  world: WorldState
}
