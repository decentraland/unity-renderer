import { combineReducers } from 'redux'

import { profileReducer } from 'shared/profiles/reducer'
import { catalogsReducer } from 'shared/catalogs/reducer'
import { rendererReducer } from 'shared/renderer/reducer'
import { loadingReducer } from 'shared/loading/reducer'
import { atlasReducer } from 'shared/atlas/reducer'
import { daoReducer } from 'shared/dao/reducer'
import { metaReducer } from 'shared/meta/reducer'
import { chatReducer } from 'shared/chat/reducer'
import { commsReducer } from 'shared/comms/reducer'
import { onboardingReducer, realmReducer } from 'shared/realm/reducer'
import { voiceChatReducer } from 'shared/voiceChat/reducer'
import { friendsReducer } from 'shared/friends/reducer'
import { sessionReducer } from 'shared/session/reducer'
import { questsReducer } from 'shared/quests/reducer'
import { portableExperienceReducer } from 'shared/portableExperiences/reducer'
import { wearablesPortableExperienceReducer } from 'shared/wearablesPortableExperience/reducer'
import { sceneLoaderReducer } from 'shared/scene-loader/reducer'
import { worldReducer } from 'shared/world/reducer'

export const reducers = combineReducers({
  atlas: atlasReducer,
  chat: chatReducer,
  friends: friendsReducer,
  session: sessionReducer,
  loading: loadingReducer,
  profiles: profileReducer,
  catalogs: catalogsReducer,
  renderer: rendererReducer,
  dao: daoReducer,
  comms: commsReducer,
  realm: realmReducer,
  voiceChat: voiceChatReducer,
  meta: metaReducer,
  quests: questsReducer,
  wearablesPortableExperiences: wearablesPortableExperienceReducer,
  portableExperiences: portableExperienceReducer,
  sceneLoader: sceneLoaderReducer,
  world: worldReducer,
  onboarding: onboardingReducer
})
