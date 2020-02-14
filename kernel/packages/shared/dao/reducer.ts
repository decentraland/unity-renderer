import { AnyAction } from 'redux'
import {
  SET_CATALYST_REALM,
  INIT_CATALYST_REALM,
  SET_CATALYST_CANDIDATES,
  SET_CATALYST_REALM_COMMS_STATUS
} from './actions'
import { DaoState, Candidate, Realm } from './types'
import {
  FETCH_PROFILE_SERVICE,
  FETCH_CONTENT_SERVICE,
  UPDATE_CONTENT_SERVICE,
  COMMS_SERVICE,
  REALM as REALM_QUERY
} from '../../config/index'
import { getRealmFromString } from '.'

function getConfiguredRealm(candidates: Candidate[]) {
  if (REALM_QUERY) {
    return getRealmFromString(REALM_QUERY, candidates)
  }
}

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      initialized: false,
      profileServer: '',
      fetchContentServer: '',
      updateContentServer: '',
      commsServer: '',
      realm: undefined,
      candidates: [],
      commsStatus: { status: 'initial', connectedPeers: 0 }
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CATALYST_CANDIDATES:
      return {
        ...state,
        candidates: action.payload
      }
    case INIT_CATALYST_REALM: {
      const configuredRealm = getConfiguredRealm(state.candidates)
      const realm = configuredRealm ? configuredRealm : action.payload
      return {
        ...state,
        initialized: true,
        ...realmProperties(realm)
      }
    }
    case SET_CATALYST_REALM:
      return {
        ...state,
        ...realmProperties(action.payload, !!action.payload.configOverride)
      }
    case SET_CATALYST_REALM_COMMS_STATUS:
      return {
        ...state,
        commsStatus: action.payload ? action.payload : { status: 'initial', connectedPeers: 0 }
      }
    default:
      return state
  }
}
function realmProperties(realm: Realm, configOverride: boolean = true) {
  const domain = realm.domain
  return {
    profileServer: FETCH_PROFILE_SERVICE && configOverride ? FETCH_PROFILE_SERVICE : domain + '/lambdas/profile',
    fetchContentServer: FETCH_CONTENT_SERVICE && configOverride ? FETCH_CONTENT_SERVICE : domain + '/lambdas/contentv2',
    updateContentServer: UPDATE_CONTENT_SERVICE && configOverride ? UPDATE_CONTENT_SERVICE : domain + '/content',
    commsServer: COMMS_SERVICE && configOverride ? COMMS_SERVICE : domain + '/comms',
    realm
  }
}
