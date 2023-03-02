import { call, takeEvery, debounce, select, put, delay } from 'redux-saga/effects'
import { LoadableScene } from 'shared/types'
import {
  ADD_DESIRED_PORTABLE_EXPERIENCE,
  REMOVE_DESIRED_PORTABLE_EXPERIENCE
} from 'shared/wearablesPortableExperience/actions'
import { declareWantedPortableExperiences } from 'unity-interface/portableExperiencesUtils'
import {
  ADD_SCENE_PX,
  DENY_PORTABLE_EXPERIENCES,
  ReloadScenePortableExperienceAction,
  RELOAD_SCENE_PX,
  REMOVE_SCENE_PX,
  updateEnginePortableExperiences,
  UpdateEnginePortableExperiencesAction,
  UPDATE_ENGINE_PX,
  SHUTDOWN_ALL_PORTABLE_EXPERIENCES,
  ACTIVATE_ALL_PORTABLE_EXPERIENCES,
  ADD_KERNEL_PX
} from './actions'
import { getDesiredPortableExperiences } from './selectors'

export function* portableExperienceSaga(): any {
  yield takeEvery(REMOVE_DESIRED_PORTABLE_EXPERIENCE, handlePortableExperienceChanges)
  yield takeEvery(ADD_DESIRED_PORTABLE_EXPERIENCE, handlePortableExperienceChanges)
  yield takeEvery(SHUTDOWN_ALL_PORTABLE_EXPERIENCES, handlePortableExperienceChanges)
  yield takeEvery(ACTIVATE_ALL_PORTABLE_EXPERIENCES, handlePortableExperienceChanges)
  yield takeEvery(DENY_PORTABLE_EXPERIENCES, handlePortableExperienceChanges)
  yield takeEvery(ADD_SCENE_PX, handlePortableExperienceChanges)
  yield takeEvery(ADD_KERNEL_PX, handlePortableExperienceChanges)
  yield takeEvery(REMOVE_SCENE_PX, handlePortableExperienceChanges)
  yield takeEvery(RELOAD_SCENE_PX, reloadPortableExperienceChanges)
  yield debounce(100 /* ms */, UPDATE_ENGINE_PX, handlePortableExperienceChangesEffect)
}

// every time the desired portable experiences change, the action `updateEnginePortableExperiences` should be dispatched
function* handlePortableExperienceChanges() {
  const desiredPortableExperiences = yield select(getDesiredPortableExperiences)
  yield put(updateEnginePortableExperiences(desiredPortableExperiences))
}

// reload portable experience
function* reloadPortableExperienceChanges(action: ReloadScenePortableExperienceAction) {
  const allDesiredPortableExperiences: LoadableScene[] = yield select(getDesiredPortableExperiences)

  const filteredDesiredPortableExperiences = allDesiredPortableExperiences.filter(
    ($) => $.id !== action.payload.data.id
  )

  // unload the filtered PX
  yield call(declareWantedPortableExperiences, filteredDesiredPortableExperiences)
  // TODO: this is a horrible hack to give enough time to the renderer to kill all the PX
  yield delay(250)
  // reload all PX
  yield call(declareWantedPortableExperiences, allDesiredPortableExperiences)
}

// tell the controller which PXs we do want running
function* handlePortableExperienceChangesEffect(action: UpdateEnginePortableExperiencesAction) {
  yield call(declareWantedPortableExperiences, action.payload.desiredPortableExperiences)
}
