import { AnyAction } from 'redux'
import { REPORTED_SCENES_FOR_MINIMAP } from './actions'
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
import { PayloadAction } from 'typesafe-actions'

const ATLAS_INITIAL_STATE: AtlasState = {
  marketName: {},
  sceneNames: {},
  requestStatus: {},
  districtName: {},
  alreadyReported: {},
  lastReportPosition: undefined
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
      const marketAction = action as PayloadAction<typeof MARKET_DATA, MarketData>
      return {
        ...state,
        marketName: {
          ...state.marketName,
          ...marketAction.payload.data
        }
      }
    case REPORTED_SCENES_FOR_MINIMAP:
      return {
        ...state,
        lastReportPosition: action.payload.reportPosition ? action.payload.reportPosition : state.lastReportPosition,
        atlasReducer: {
          ...state.alreadyReported,
          ...action.payload.parcels.reduce((prev: Record<string, boolean>, next: string) => {
            prev[next] = true
            return prev
          }, {})
        }
      }
    case DISTRICT_DATA:
      return {
        ...state,
        districtName: {
          ...state.districtName,
          ...zip(action.payload.data, (t: District) => [t.id, t.name])
        }
      }
  }
  return state
}
