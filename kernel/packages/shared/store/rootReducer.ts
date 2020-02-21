import { combineReducers } from 'redux'
import { passportsReducer } from '../passports/reducer'
import { rendererReducer } from '../renderer/reducer'
import { protocolReducer } from '../protocol/reducer'
import { loadingReducer } from '../loading/reducer'
import { atlasReducer } from '../atlas/reducer'
import { daoReducer } from '../dao/reducer'
import { metaReducer } from '../meta/reducer'

export const reducers = combineReducers({
  atlas: atlasReducer,
  loading: loadingReducer,
  passports: passportsReducer,
  renderer: rendererReducer,
  protocol: protocolReducer,
  dao: daoReducer,
  meta: metaReducer
})
