import { RpcClient, RpcClientPort, Transport } from '@dcl/rpc'
import type { UnityGame } from 'unity-interface/loader'
import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations/types'
import { deepEqual } from 'lib/javascript/deepEqual'
import defaultLogger from 'lib/logger'
import { call, fork, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { createRendererRpcClient } from 'renderer-protocol/rpcClient'
import { registerEmotesService } from 'renderer-protocol/services/emotesService'
import { registerFriendRequestRendererService } from 'renderer-protocol/services/friendRequestService'
import { registerRestrictedActionsService } from 'renderer-protocol/services/restrictedActionsService'
import { createRpcTransportService } from 'renderer-protocol/services/transportService'
import { trackEvent } from 'shared/analytics/trackEvent'
import { receivePeerUserData } from 'shared/comms/peers'
import { getExploreRealmsService } from 'shared/dao/selectors'
import { waitingForRenderer } from 'shared/loading/types'
import {getAllowedContentServer, getFeatureFlagVariantName} from 'shared/meta/selectors'
import {
  addProfileToLastSentProfileVersionAndCatalog,
  SendProfileToRenderer,
  sendProfileToRenderer,
  SEND_PROFILE_TO_RENDERER_REQUEST
} from 'shared/profiles/actions'
import { getLastSentProfileVersion, getProfileFromStore } from 'shared/profiles/selectors'
import { SET_REALM_ADAPTER } from 'shared/realm/actions'
import { getFetchContentServerFromRealmAdapter, getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { isSceneFeatureToggleEnabled } from 'lib/decentraland/sceneJson/isSceneFeatureToggleEnabled'
import { SignUpSetIsSignUp, SIGNUP_SET_IS_SIGNUP, signUp } from 'shared/session/actions'
import { getCurrentIdentity, getCurrentUserId } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'
import { CurrentRealmInfoForRenderer, NotificationType, VOICE_CHAT_FEATURE_TOGGLE } from 'shared/types'
import { store } from 'shared/store/isolatedStore'
import {
  SetVoiceChatErrorAction,
  SET_VOICE_CHAT_ERROR,
  SET_VOICE_CHAT_HANDLER,
  VoicePlayingUpdateAction,
  VoiceRecordingUpdateAction,
  VOICE_PLAYING_UPDATE,
  VOICE_RECORDING_UPDATE
} from 'shared/voiceChat/actions'
import { getVoiceHandler } from 'shared/voiceChat/selectors'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { SetCurrentScene, SET_CURRENT_SCENE } from 'shared/world/actions'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { initializeEngine } from 'unity-interface/dcl'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { InitializeRenderer, registerRendererModules, registerRendererPort, REGISTER_RPC_PORT } from './actions'
import { waitForRendererInstance } from './sagas-helper'
import { getClientPort } from './selectors'
import { RendererModules, RENDERER_INITIALIZE } from './types'
import { adjectives, animals, colors, Config, uniqueNamesGenerator } from 'unique-names-generator'

export function* rendererSaga() {
  yield takeEvery(SEND_PROFILE_TO_RENDERER_REQUEST, handleSubmitProfileToRenderer)
  yield takeLatest(SIGNUP_SET_IS_SIGNUP, sendSignUpToRenderer)
  yield takeEvery(VOICE_PLAYING_UPDATE, updateUserVoicePlayingRenderer)
  yield takeEvery(VOICE_RECORDING_UPDATE, updatePlayerVoiceRecordingRenderer)
  yield takeEvery(SET_VOICE_CHAT_ERROR, handleVoiceChatError)
  yield takeEvery(REGISTER_RPC_PORT, handleRegisterRpcPort)

  const action: InitializeRenderer = yield take(RENDERER_INITIALIZE)
  yield call(initializeRenderer, action)

  yield takeLatest(SET_CURRENT_SCENE, listenToWhetherSceneSupportsVoiceChat)

  yield fork(reportRealmChangeToRenderer)
  yield fork(updateChangeVoiceChatHandlerProcess)
}

/**
 * On every new port we register the services for it and starts the inverse RPC
 */
function* handleRegisterRpcPort() {
  const port: RpcClientPort | undefined = yield select(getClientPort)

  if (!port) {
    return
  }

  if (createRpcTransportService(port)) {
    const modules: RendererModules = {
      emotes: registerEmotesService(port),
      friendRequest: registerFriendRequestRendererService(port),
      restrictedActions: registerRestrictedActionsService(port)
    }

    yield put(registerRendererModules(modules))
  }
}

/**
 * This saga sends the BFF configuration changes to the renderer upon every change
 */
function* reportRealmChangeToRenderer() {
  yield call(waitForRendererInstance)

  while (true) {
    const realmAdapter: IRealmAdapter = yield call(waitForRealm)

    try {
      const configuredContentServer: string = getFetchContentServerFromRealmAdapter(realmAdapter)
      const contentServerUrl: string = yield select(getAllowedContentServer, configuredContentServer)
      const current = convertCurrentRealmType(realmAdapter, contentServerUrl)
      defaultLogger.info('UpdateRealmsInfo', current)
      getUnityInstance().UpdateRealmsInfo({ current })
      getUnityInstance().UpdateRealmAbout(realmAdapter.about)

      const realmsService = yield select(getExploreRealmsService)

      if (realmsService) {
        yield call(fetchAndReportRealmsInfo, realmsService)
      }

      // wait for the next context
    } catch (err: any) {
      defaultLogger.error(err)
    }

    yield take(SET_REALM_ADAPTER)
  }
}

async function fetchAndReportRealmsInfo(url: string) {
  try {
    const response = await fetch(url)
    if (response.ok) {
      const value = await response.json()
      getUnityInstance().UpdateRealmsInfo({ realms: value })
    }
  } catch (e) {
    defaultLogger.error(url, e)
  }
}

function convertCurrentRealmType(realmAdapter: IRealmAdapter, contentServerUrl: string): CurrentRealmInfoForRenderer {
  return {
    serverName: realmAdapter.about.configurations?.realmName || realmAdapter.baseUrl,
    layer: '',
    domain: realmAdapter.baseUrl,
    contentServerUrl: contentServerUrl
  }
}

function* updateUserVoicePlayingRenderer(action: VoicePlayingUpdateAction) {
  const { playing, userId } = action.payload
  yield call(waitForRendererInstance)
  getUnityInstance().SetUserTalking(userId, playing)
}

function* updatePlayerVoiceRecordingRenderer(action: VoiceRecordingUpdateAction) {
  yield call(waitForRendererInstance)
  getUnityInstance().SetPlayerTalking(action.payload.recording)
}

function* updateChangeVoiceChatHandlerProcess() {
  while (true) {
    // wait for a new VoiceHandler
    yield take(SET_VOICE_CHAT_HANDLER)

    const handler: VoiceHandler | null = yield select(getVoiceHandler)

    yield call(waitForRendererInstance)

    if (handler) {
      getUnityInstance().SetVoiceChatStatus({ isConnected: true })
    } else {
      getUnityInstance().SetVoiceChatStatus({ isConnected: false })
    }
  }
}

function* handleVoiceChatError(action: SetVoiceChatErrorAction) {
  const message = action.payload.message
  yield call(waitForRendererInstance)
  if (message) {
    getUnityInstance().ShowNotification({
      type: NotificationType.GENERIC,
      message,
      buttonMessage: 'OK',
      timer: 5
    })
  }
}

function* listenToWhetherSceneSupportsVoiceChat(data: SetCurrentScene) {
  const currentScene: SceneWorker | undefined = data.payload.currentScene
    ? yield call(getSceneWorkerBySceneID, data.payload.currentScene)
    : undefined

  const nowEnabled = currentScene
    ? isSceneFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE, currentScene?.metadata)
    : isSceneFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE)

  yield call(waitForRendererInstance)

  getUnityInstance().SetVoiceChatEnabledByScene(nowEnabled)
}

function* initializeRenderer(action: InitializeRenderer) {
  const { delegate, container } = action.payload

  // will start loading
  yield put(waitingForRenderer())

  // start loading the renderer
  try {
    const { renderer, transport }: { renderer: UnityGame; transport: Transport } = yield call(delegate, container)

    const startTime = performance.now()

    trackEvent('renderer_initializing_start', {})

    // register the RPC port
    const rpcHandles: {
      rpcClient: RpcClient
      rendererInterfacePort: RpcClientPort
    } = yield call(createRendererRpcClient, transport)
    yield put(registerRendererPort(rpcHandles.rpcClient, rpcHandles.rendererInterfacePort))

    // wire the kernel to the renderer, at some point, the `initializeEngine`
    // function _MUST_ send the `signalRendererInitializedCorrectly` action
    // to signal that the renderer successfuly loaded
    yield call(initializeEngine, renderer)

    // wait for renderer start
    yield call(waitForRendererInstance)

    trackEvent('renderer_initializing_end', {
      loading_time: performance.now() - startTime
    })
  } catch (e) {
    trackEvent('renderer_initialization_error', {
      message: e + ''
    })
    if (e instanceof Error) {
      throw e
    } else {
      throw new Error('Error initializing rendering')
    }
  }
}

function* sendSignUpToRenderer(action: SignUpSetIsSignUp) {
  if (action.payload.isSignUp) {
    if (getFeatureFlagVariantName(store.getState(), 'seamless_login_variant') === 'enabled') {
      const userId: string = yield select(getCurrentUserId)
      yield put(sendProfileToRenderer(userId))
      const config: Config = {
        dictionaries: [adjectives, colors, animals],
        separator: '-',
        style: 'capital'
      }
      const randomName = uniqueNamesGenerator(config)
      trackEvent('seamless_login tos accepted', {})
      store.dispatch(signUp('', randomName))
      getUnityInstance().ShowAvatarEditorInSignIn()
    } else {
      getUnityInstance().ShowAvatarEditorInSignIn()
      const userId: string = yield select(getCurrentUserId)
      yield put(sendProfileToRenderer(userId))
    }
  }
}

let lastSentProfile: NewProfileForRenderer | null = null
export function* handleSubmitProfileToRenderer(action: SendProfileToRenderer): any {
  const { userId } = action.payload

  yield call(waitForRendererInstance)
  const bff: IRealmAdapter = yield call(waitForRealm)
  const { profile, identity, isCurrentUser, lastSentProfileVersion } = (yield select(
    getInformationToSubmitProfileFromStore,
    userId
  )) as ReturnType<typeof getInformationToSubmitProfileFromStore>
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(bff)

  if (!profile || !profile.data) {
    return
  }

  if (isCurrentUser) {
    const forRenderer = profileToRendererFormat(profile.data, {
      address: identity?.address,
      baseUrl: fetchContentServerWithPrefix
    })
    forRenderer.hasConnectedWeb3 = identity?.hasConnectedWeb3 || false

    // TODO: this condition shouldn't be necessary. Unity fails with setThrew
    //       if LoadProfile is called rapidly because it cancels ongoing
    //       requests and those cancellations throw exceptions
    if (lastSentProfile && lastSentProfile?.version > forRenderer.version) {
      const event = 'Invalid user version' as const
      trackEvent(event, { address: userId, version: forRenderer.version })
    } else if (!deepEqual(lastSentProfile, forRenderer)) {
      lastSentProfile = forRenderer
      getUnityInstance().LoadProfile(forRenderer)
    }
  } else {
    // Add version check before submitting profile to renderer
    // Technically profile version might be `0` and make `!lastSentProfileVersion` always true
    if (typeof lastSentProfileVersion !== 'number' || lastSentProfileVersion < profile.data.version) {
      const forRenderer = profileToRendererFormat(profile.data, {
        baseUrl: fetchContentServerWithPrefix
      })

      getUnityInstance().AddUserProfileToCatalog(forRenderer)
      // Update catalog and last sent profile version
      yield put(addProfileToLastSentProfileVersionAndCatalog(userId, forRenderer.version))
    }

    // Send to Avatars scene
    // TODO: Consider refactor this so that it's distinguishable from a message received over the network
    // (`receivePeerUserData` is a handler for a comms message!!!)
    receivePeerUserData(profile.data, fetchContentServerWithPrefix)
  }
}

export function getInformationToSubmitProfileFromStore(state: RootState, userId: string) {
  const identity = getCurrentIdentity(state)
  const isCurrentUser = identity?.address.toLowerCase() === userId.toLowerCase()
  return {
    profile: getProfileFromStore(state, userId),
    identity,
    isCurrentUser,
    lastSentProfileVersion: getLastSentProfileVersion(state, userId)
  }
}
