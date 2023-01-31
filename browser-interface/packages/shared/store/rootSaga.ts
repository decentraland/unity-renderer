import { fork } from 'redux-saga/effects'
import { atlasSaga } from '../atlas/sagas'
import { loadingSaga } from '../loading/sagas'
import { profileSaga } from '../profiles/sagas'
import { rendererSaga } from '../renderer/sagas'
import { metricSaga } from './metricSaga'
import { daoSaga } from '../dao/sagas'
import { metaSaga } from '../meta/sagas'
import { chatSaga } from '../chat/sagas'
import { sessionSaga } from '../session/sagas'
import { friendsSaga } from '../friends/sagas'
import { commsSaga } from '../comms/sagas'
import { voiceChatSaga } from '../voiceChat/sagas'
import { socialSaga } from '../social/sagas'
import { catalogsSaga } from '../catalogs/sagas'
import { questsSaga } from '../quests/sagas'
import { portableExperienceSaga } from '../portableExperiences/sagas'
import { wearablesPortableExperienceSaga } from '../wearablesPortableExperience/sagas'
import { sceneEventsSaga } from '../sceneEvents/sagas'
import { sceneLoaderSaga } from '../scene-loader/sagas'
import { worldSagas } from '../world/sagas'
import { bffSaga } from 'shared/realm/sagas'
import { loadingScreenSaga } from '../loadingScreen/sagas'

export function createRootSaga() {
  return function* rootSaga() {
    yield fork(metaSaga)
    yield fork(friendsSaga)
    yield fork(sessionSaga)
    yield fork(commsSaga)
    yield fork(bffSaga)
    yield fork(voiceChatSaga)
    yield fork(catalogsSaga)
    yield fork(profileSaga)
    yield fork(chatSaga)
    yield fork(atlasSaga)
    yield fork(daoSaga)
    yield fork(metricSaga)
    yield fork(loadingSaga)
    yield fork(loadingScreenSaga)
    yield fork(socialSaga)
    yield fork(questsSaga)
    yield fork(rendererSaga)
    yield fork(sceneEventsSaga)
    yield fork(portableExperienceSaga)
    yield fork(wearablesPortableExperienceSaga)
    yield fork(sceneLoaderSaga)
    yield fork(worldSagas)
  }
}
