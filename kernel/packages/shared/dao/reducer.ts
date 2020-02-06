import { AnyAction } from 'redux'
import { SET_CATALYST_REALM } from './actions'
import { DaoState } from './types'
import {
  FETCH_PROFILE_SERVICE,
  FETCH_CONTENT_SERVICE,
  UPDATE_CONTENT_SERVICE,
  COMMS_SERVICE,
  LAYER
} from '../../config/index'

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      initialized: false,
      profileServer: '',
      fetchContentServer: '',
      updateContentServer: '',
      commsServer: '',
      layer: ''
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CATALYST_REALM:
      return {
        ...state,
        initialized: true,
        profileServer: FETCH_PROFILE_SERVICE ? FETCH_PROFILE_SERVICE : action.payload.domain + '/lambdas/profile',
        fetchContentServer: FETCH_CONTENT_SERVICE
          ? FETCH_CONTENT_SERVICE
          : action.payload.domain + '/lambdas/contentv2',
        updateContentServer: UPDATE_CONTENT_SERVICE ? UPDATE_CONTENT_SERVICE : action.payload.domain + '/content',
        commsServer: COMMS_SERVICE ? COMMS_SERVICE : action.payload.domain + '/comms',
        layer: LAYER ? LAYER : action.payload.layer
      }
    default:
      return state
  }
}
