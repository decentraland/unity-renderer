import { fork } from 'redux-saga/effects'
import { atlasSaga } from 'shared/atlas/sagas'
import { loadingSaga } from 'shared/loading/sagas'
import { profileSaga } from 'shared/profiles/sagas'
import { rendererSaga } from 'shared/renderer/sagas'
import { metricSaga } from './metricSaga'
import { daoSaga } from 'shared/dao/sagas'
import { metaSaga } from 'shared/meta/sagas'
import { chatSaga } from 'shared/chat/sagas'
import { sessionSaga } from 'shared/session/sagas'
import { friendsSaga } from 'shared/friends/sagas'
import { commsSaga } from 'shared/comms/sagas'
import { voiceChatSaga } from 'shared/voiceChat/sagas'
import { socialSaga } from 'shared/social/sagas'
import { catalogsSaga } from 'shared/catalogs/sagas'
import { questsSaga } from 'shared/quests/sagas'
import { portableExperienceSaga } from 'shared/portableExperiences/sagas'
import { wearablesPortableExperienceSaga } from 'shared/wearablesPortableExperience/sagas'
import { sceneEventsSaga } from 'shared/sceneEvents/sagas'
import { sceneLoaderSaga } from 'shared/scene-loader/sagas'
import { worldSagas } from 'shared/world/sagas'
import { bffSaga } from 'shared/realm/sagas'

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
