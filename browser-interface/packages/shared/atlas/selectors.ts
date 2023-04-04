import { RootAtlasState } from './types'

export const EMPTY_PARCEL_NAME = 'Empty parcel'

export function getPoiTiles(state: RootAtlasState) {
  return state.atlas.pois
}

export function postProcessSceneName(name: string | undefined | null): string {
  if (name === undefined || name === 'interactive-text' || name === null) {
    return EMPTY_PARCEL_NAME
  }

  if (name.startsWith('Road at')) {
    return 'Road'
  }

  return name
}
