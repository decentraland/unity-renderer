import { PORTABLE_EXPERIENCES_DEBOUNCE_DELAY } from 'config'
import { call, debounce, delay, fork, put, select, takeEvery } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'
import { getFeatureFlagVariantValue } from 'shared/meta/selectors'
import { waitForRendererRpcConnection } from 'shared/renderer/sagas-helper'
import { LoadableScene } from 'shared/types'
import {
  ADD_DESIRED_PORTABLE_EXPERIENCE,
  REMOVE_DESIRED_PORTABLE_EXPERIENCE
} from 'shared/wearablesPortableExperience/actions'
import {
  declareWantedPortableExperiences,
  getPortableExperienceFromUrn
} from 'unity-interface/portableExperiencesUtils'
import {
  ACTIVATE_ALL_PORTABLE_EXPERIENCES,
  addKernelPortableExperience,
  ADD_KERNEL_PX,
  ADD_SCENE_PX,
  DENY_PORTABLE_EXPERIENCES,
  ReloadScenePortableExperienceAction,
  RELOAD_SCENE_PX,
  REMOVE_SCENE_PX,
  SHUTDOWN_ALL_PORTABLE_EXPERIENCES,
  updateEnginePortableExperiences,
  UpdateEnginePortableExperiencesAction,
  UPDATE_ENGINE_PX
} from './actions'
import { getDesiredPortableExperiences } from './selectors'

export function* portableExperienceSaga(): any {
  // Ensure we have a connection to the renderer
  yield call(waitForRendererRpcConnection)

  // List the actions that might trigger a portable experience change
  yield takeEvery(
    [
      REMOVE_DESIRED_PORTABLE_EXPERIENCE,
      ADD_DESIRED_PORTABLE_EXPERIENCE,
      SHUTDOWN_ALL_PORTABLE_EXPERIENCES,
      ACTIVATE_ALL_PORTABLE_EXPERIENCES,
      DENY_PORTABLE_EXPERIENCES,
      ADD_SCENE_PX,
      ADD_KERNEL_PX,
      REMOVE_SCENE_PX
    ],
    handlePortableExperienceChanges
  )
  yield takeEvery(RELOAD_SCENE_PX, reloadPortableExperienceChanges)

  // Debounce it -- this is likely due to some internal logical bug
  // TODO: Figure out if we still need this
  yield debounce(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY(), UPDATE_ENGINE_PX, handlePortableExperienceChangesEffect)

  // Finally, initialize the portable experiences
  yield fork(fetchInitialPortableExperiences)
}

export function* fetchInitialPortableExperiences() {
  yield waitForMetaConfigurationInitialization()

  const qs = new URLSearchParams(globalThis.location.search)

  const globalPortableExperiences: string[] = qs.has('GLOBAL_PX')
    ? qs.getAll('GLOBAL_PX')
    : yield select(getFeatureFlagVariantValue, 'initial_portable_experiences')

  if (Array.isArray(globalPortableExperiences)) {
    for (const id of globalPortableExperiences) {
      try {
        const px: LoadableScene = yield call(getPortableExperienceFromUrn, id)
        yield put(addKernelPortableExperience(px))
      } catch (err: any) {
        console.error(err)
        trackEvent('error', {
          context: 'fetchInitialPortableExperiences',
          message: err.message,
          stack: err.stack
        })
      }
    }
  }
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
