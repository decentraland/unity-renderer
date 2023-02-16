import { call, put, select, takeLatest } from 'redux-saga/effects'
import { UPDATE_LOADING_SCREEN, updateLoadingScreen } from './actions'
import { waitForRendererInstance } from '../renderer/sagas-helper'
import { getParcelLoadingStarted, isLoadingScreenVisible } from './selectors'
import { LoadingState } from '../loading/reducer'
import { getLoadingState } from '../loading/selectors'
import { RootState } from '../store/rootTypes'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { AUTHENTICATE, CHANGE_LOGIN_STAGE, SIGNUP_SET_IS_SIGNUP } from '../session/actions'
import { PARCEL_LOADING_STARTED, RENDERER_INITIALIZED_CORRECTLY } from '../renderer/types'
import { PENDING_SCENES, SCENE_FAIL, SCENE_LOAD, SCENE_UNLOAD, UPDATE_STATUS_MESSAGE } from '../loading/actions'
import { TELEPORT_TRIGGERED } from '../loading/types'
import { SET_REALM_ADAPTER } from '../realm/actions'
import { POSITION_SETTLED, POSITION_UNSETTLED, SET_SCENE_LOADER } from '../scene-loader/actions'
import { RENDERING_ACTIVATED, RENDERING_BACKGROUND, RENDERING_DEACTIVATED, RENDERING_FOREGROUND } from './types'
import { getFeatureFlagEnabled } from 'shared/meta/selectors'

// The following actions may change the status of the loginVisible
// Reaction on them will be ported to Renderer
export const ACTIONS_FOR_LOADING = [
  AUTHENTICATE,
  CHANGE_LOGIN_STAGE,
  PARCEL_LOADING_STARTED,
  PENDING_SCENES,
  RENDERER_INITIALIZED_CORRECTLY,
  RENDERING_ACTIVATED,
  RENDERING_BACKGROUND,
  RENDERING_DEACTIVATED,
  RENDERING_FOREGROUND,
  SCENE_FAIL,
  SCENE_LOAD,
  SIGNUP_SET_IS_SIGNUP,
  TELEPORT_TRIGGERED,
  UPDATE_STATUS_MESSAGE,
  SET_REALM_ADAPTER,
  SET_SCENE_LOADER,
  POSITION_SETTLED,
  POSITION_UNSETTLED,
  SCENE_UNLOAD
]

export function* loadingScreenSaga() {
}
