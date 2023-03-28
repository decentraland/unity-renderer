import { PORTABLE_EXPERIENCES_DEBOUNCE_DELAY } from 'config'
import { call, debounce, delay, fork, put, select, takeEvery } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'
import { getFeatureFlagVariantValue } from 'shared/meta/selectors'
import { waitForAvatarSceneInitialized, waitForRendererRpcConnection } from 'shared/renderer/sagas-helper'
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

  // Many actions can trigger a rebuild of the portable experience "desired list", for example, by adding wearables one
  // by one in some async `for` loop that awaits for each experience.
  // Thus, we reactions to UPDATE_ENGINE_PX, so we only alert the renderer once, and avoid unnecessary processing costs.
  yield debounce(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY(), UPDATE_ENGINE_PX, handlePortableExperienceChangesEffect)

  // Finally, initialize the portable experiences
  yield fork(fetchInitialPortableExperiences)
}

export function* fetchInitialPortableExperiences() {
  yield waitForMetaConfigurationInitialization()

  yield waitForAvatarSceneInitialized()

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

  // Ensure we have a connection to the renderer
  yield call(waitForRendererRpcConnection)

  // unload the filtered PX
  yield call(declareWantedPortableExperiences, filteredDesiredPortableExperiences)
  // TODO: this is a horrible hack to give enough time to the renderer to kill all the PX
  // because scene unitialization is not an atomic operation to prevent dropped frames
  yield delay(250)
  // reload all PX
  yield call(declareWantedPortableExperiences, allDesiredPortableExperiences)
}

// tell the controller which PXs we do want running, this operation can be safely debounced
function* handlePortableExperienceChangesEffect(action: UpdateEnginePortableExperiencesAction) {
  // Ensure we have a connection to the renderer
  yield call(waitForRendererRpcConnection)

  yield call(declareWantedPortableExperiences, action.payload.desiredPortableExperiences)
}
