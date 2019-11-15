import { AnyAction } from 'redux'
import {
  AtlasState,
  District,
  DISTRICT_DATA,
  FAILURE_NAME_FROM_SCENE_JSON,
  FETCH_NAME_FROM_SCENE_JSON,
  MarketData,
  MARKET_DATA,
  SUCCESS_NAME_FROM_SCENE_JSON
} from './types'
import { zip } from './zip'

const ATLAS_INITIAL_STATE: AtlasState = {
  marketName: {},
  sceneNames: {},
  requestStatus: {},
  districtName: {}
}

export function atlasReducer(state?: AtlasState, action?: AnyAction) {
  if (!state) {
    return ATLAS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case FETCH_NAME_FROM_SCENE_JSON:
      return {
        ...state,
        requestStatus: {
          ...state.requestStatus,
          [action.payload]: 'loading'
        }
      }
    case SUCCESS_NAME_FROM_SCENE_JSON:
      if (action.payload.name === 'interactive-text') {
        return state
      }
      return {
        ...state,
        requestStatus: {
          ...state.requestStatus,
          [action.payload.sceneId]: 'ok'
        },
        sceneNames: {
          ...state.sceneNames,
          ...(action.payload as { parcels: string[]; name: string }).parcels.reduce((prev, val) => {
            return { ...prev, [val]: action.payload.name }
          }, {})
        }
      }
    case FAILURE_NAME_FROM_SCENE_JSON:
      return {
        ...state,
        requestStatus: {
          ...state.requestStatus,
          [action.payload.sceneId]: 'failure'
        }
      }
    case MARKET_DATA:
      return {
        ...state,
        marketName: {
          ...state.marketName,
          ...((action as any) as { payload: MarketData }).payload.data
        }
      }
    case DISTRICT_DATA:
      return {
        ...state,
        districtName: {
          ...state.districtName,
          ...zip((action as any).payload.data, (t: District) => [t.id, t.name])
        }
      }
  }
  return state
}
