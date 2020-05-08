import { combineReducers } from 'redux'
import { profileReducer } from '../profiles/reducer'
import { rendererReducer } from '../renderer/reducer'
import { protocolReducer } from '../protocol/reducer'
import { loadingReducer } from '../loading/reducer'
import { atlasReducer } from '../atlas/reducer'
import { daoReducer } from '../dao/reducer'
import { metaReducer } from '../meta/reducer'
import { chatReducer } from '../chat/reducer'

export const reducers = combineReducers({
  atlas: atlasReducer,
  chat: chatReducer,
  loading: loadingReducer,
  profiles: profileReducer,
  renderer: rendererReducer,
  protocol: protocolReducer,
  dao: daoReducer,
  meta: metaReducer
})
