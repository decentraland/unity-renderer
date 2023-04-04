import { AnyAction } from 'redux'
import { LAST_REPORTED_POSITION, ReportLastPosition, INITIALIZE_POI_TILES, InitializePoiTiles } from './actions'
import { AtlasState } from './types'

const ATLAS_INITIAL_STATE: AtlasState = {
  hasPois: false,
  lastReportPosition: undefined,
  pois: []
}

export function atlasReducer(state?: AtlasState, action?: AnyAction) {
  if (!state) {
    return ATLAS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case LAST_REPORTED_POSITION:
      return reduceReportedLastPositionForMinimap(state, (action as ReportLastPosition).payload)
    case INITIALIZE_POI_TILES:
      return reduceInitializePoiTiles(state, action as InitializePoiTiles)
  }
  return state
}

function reduceInitializePoiTiles(state: AtlasState, action: InitializePoiTiles): AtlasState {
  return { ...state, hasPois: true, pois: action.payload.tiles }
}

function reduceReportedLastPositionForMinimap(state: AtlasState, payload: ReportLastPosition['payload']) {
  return {
    ...state,
    lastReportPosition: payload.position
  }
}
