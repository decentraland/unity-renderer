import { combineReducers } from 'redux'
import { passportsReducer } from '../passports/reducer'
import { authReducer } from '../auth/reducer'
import { rendererReducer } from '../renderer/reducer'
import { protocolReducer } from '../protocol/reducer'
import { loadingReducer } from '../loading/reducer'

export const reducers = combineReducers({
  auth: authReducer,
  loading: loadingReducer,
  passports: passportsReducer,
  renderer: rendererReducer,
  protocol: protocolReducer
})
