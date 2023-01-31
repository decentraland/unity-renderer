import { call, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { Authenticator } from '@dcl/crypto'

import { DEBUG_KERNEL_LOG, ETHEREUM_NETWORK, PREVIEW } from 'config'

import { createDummyLogger, createLogger } from 'shared/logger'
import { initializeReferral, referUser } from 'shared/referral'
import { getUserAccount, isSessionExpired, requestManager } from 'shared/ethereum/provider'
import { awaitingUserSignature, AWAITING_USER_SIGNATURE } from 'shared/loading/types'
import { getAppNetwork, registerProviderNetChanges } from 'shared/web3'

import { getFromPersistentStorage, saveToPersistentStorage } from 'atomicHelpers/persistentStorage'

import { getLastGuestSession, getStoredSession, removeStoredSession, setStoredSession } from './index'
import { ExplorerIdentity, RootSessionState, SessionState, StoredSession } from './types'
import {
  AUTHENTICATE,
  changeLoginState,
  INIT_SESSION,
  LOGOUT,
  REDIRECT_TO_SIGN_UP,
  SIGNUP,
  SIGNUP_CANCEL,
  signUpClearData,
  signUpSetIsSignUp,
  UPDATE_TOS,
  updateTOS,
  userAuthentified,
  AuthenticateAction,
  signUpCancel,
  USER_AUTHENTIFIED,
  UserAuthentified,
  SignUpAction
} from './actions'
import { localProfilesRepo } from '../profiles/sagas'
import { getCurrentIdentity, getIsGuestLogin, isLoginCompleted } from './selectors'
import { waitForRoomConnection } from '../dao/sagas'
import { saveProfileDelta } from '../profiles/actions'
import { DecentralandIdentity, LoginState } from '@dcl/kernel-interface'
import { RequestManager } from 'eth-connect'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { Store } from 'redux'
import { store } from 'shared/store/isolatedStore'
import { globalObservable } from 'shared/observables'
import { selectNetwork } from 'shared/dao/actions'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { setRoomConnection } from 'shared/comms/actions'
import { Avatar } from '@dcl/schemas'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { createUnsafeIdentity } from '@dcl/crypto/dist/crypto'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { ProfileType } from 'shared/profiles/types'

const TOS_KEY = 'tos'
const logger = DEBUG_KERNEL_LOG ? createLogger('session: ') : createDummyLogger()

export function* sessionSaga(): any {
  yield takeLatest(AUTHENTICATE, authenticate)

  yield takeEvery(UPDATE_TOS, updateTermOfService)
  yield takeLatest(INIT_SESSION, initSession)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(REDIRECT_TO_SIGN_UP, redirectToSignUp)
  yield takeLatest(SIGNUP, signUpHandler)
  yield takeLatest(SIGNUP_CANCEL, cancelSignUp)
  yield takeLatest(AWAITING_USER_SIGNATURE, signaturePrompt)
  yield takeEvery(USER_AUTHENTIFIED, function* (action: UserAuthentified) {
    yield call(saveSession, action.payload.identity, action.payload.isGuest)
    logger.log(`User ${action.payload.identity.address} logged in isGuest=` + action.payload.isGuest)
  })

  yield call(initialize)
  yield call(initializeReferral)
}

function* initialize() {
  const tosAgreed: boolean = !!((yield call(getFromPersistentStorage, TOS_KEY)) as boolean)
  yield put(updateTOS(tosAgreed))
}

function* updateTermOfService(action: any) {
  yield call(saveToPersistentStorage, TOS_KEY, action.payload)
}

function* signaturePrompt() {
  yield put(changeLoginState(LoginState.SIGNATURE_PENDING))
}

function* initSession() {
  yield put(changeLoginState(LoginState.WAITING_PROVIDER))
}

function* authenticate(action: AuthenticateAction) {
  const { isGuest, provider } = action.payload
  // setup provider
  requestManager.setProvider(provider)

  yield put(changeLoginState(LoginState.SIGNATURE_PENDING))

  let identity: ExplorerIdentity

  try {
    identity = yield authorize(requestManager)
  } catch (e: any) {
    if (('' + (e.message || e.toString())).includes('User denied message signature')) {
      yield put(signUpCancel())
      return
    } else {
      throw e
    }
  }

  yield put(changeLoginState(LoginState.WAITING_RENDERER))

  yield call(waitForRendererInstance)

  yield put(changeLoginState(LoginState.WAITING_PROFILE))

  // set the etherum network to start loading profiles
  const net: ETHEREUM_NETWORK = yield call(getAppNetwork)
  yield put(selectNetwork(net))
  registerProviderNetChanges()

  // 1. authenticate our user
  yield put(userAuthentified(identity, net, isGuest))
  // 2. wait for comms to connect, it only requires the Identity authentication
  yield call(waitForRoomConnection)
  // 3. then ask for our profile
  const avatar: Avatar = yield call(
    ProfileAsPromise,
    identity.address,
    0,
    isGuest ? ProfileType.LOCAL : ProfileType.DEPLOYED
  )

  // 4. continue with signin/signup (only not in preview)
  const isSignUp = avatar.version <= 0 && !PREVIEW
  if (isSignUp) {
    yield put(signUpSetIsSignUp(isSignUp))
    yield take(SIGNUP)
  }

  // 5. finish sign in
  yield call(ensureMetaConfigurationInitialized)
  yield put(changeLoginState(LoginState.COMPLETED))

  if (!isGuest) {
    yield call(referUser, identity)
  }

  if (isSignUp) {
    // HACK to fix onboarding flow, remove in RFC-1 impl
    getUnityInstance().FadeInLoadingHUD({} as any)
  }
}

function* authorize(requestManager: RequestManager) {
  let userData: StoredSession | null = null

  const isGuest: boolean = yield select(getIsGuestLogin)

  if (isGuest) {
    userData = yield call(getLastGuestSession)
  } else {
    try {
      const address: string = yield call(getUserAccount, requestManager, false)
      if (address) {
        userData = yield call(getStoredSession, address)

        if (userData) {
          // We save the raw ethereum address of the current user to avoid having to convert-back later after lowercasing it for the userId
          userData.identity.rawAddress = address
        }
      }
    } catch (e: any) {
      logger.error(e)
      // do nothing
    }
  }

  // check that user data is stored & key is not expired
  if (!userData || isSessionExpired(userData)) {
    yield put(awaitingUserSignature())
    const identity: ExplorerIdentity = yield createAuthIdentity(requestManager, isGuest)
    return identity
  }

  return userData.identity
}

function* signUpHandler(action: SignUpAction) {
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)

  if (!identity) {
    throw new Error('missing identity in signup session')
  }

  yield put(
    saveProfileDelta({
      name: action.payload.name,
      userId: identity.address,
      ethAddress: identity.rawAddress,
      version: 0,
      hasClaimedName: false,
      email: ''
    })
  )

  globalObservable.emit('signUp', { email: action.payload.email })
  logger.log(`User ${identity.address} signed up`)
}

function* cancelSignUp() {
  yield put(signUpClearData())
  yield put(signUpSetIsSignUp(false))
  yield put(changeLoginState(LoginState.WAITING_PROVIDER))
}

async function saveSession(identity: ExplorerIdentity, isGuest: boolean) {
  await setStoredSession({
    identity,
    isGuest
  })
}

async function getSigner(
  requestManager: RequestManager,
  isGuest: boolean
): Promise<{
  hasConnectedWeb3: boolean
  address: string
  signer: (message: string) => Promise<string>
  ephemeralLifespanMinutes: number
}> {
  if (!isGuest) {
    const address = await getUserAccount(requestManager, false)

    if (!address) throw new Error("Couldn't get an address from the Ethereum provider")

    return {
      address,
      async signer(message: string) {
        while (true) {
          const result = await requestManager.personal_sign(message, address, '')
          if (!result) continue
          return result
        }
      },
      hasConnectedWeb3: true,
      ephemeralLifespanMinutes: 7 * 24 * 60 // 1 week
    }
  } else {
    const account = createUnsafeIdentity()

    return {
      address: account.address.toLowerCase(),
      async signer(message) {
        return Authenticator.createSignature(account, message)
      },
      hasConnectedWeb3: false,
      // If we are using a local profile, we don't want the identity to expire.
      // Eventually, if a wallet gets created, we can migrate the profile to the wallet.
      ephemeralLifespanMinutes: 365 * 24 * 60 * 99
    }
  }
}

async function createAuthIdentity(requestManager: RequestManager, isGuest: boolean): Promise<ExplorerIdentity> {
  const ephemeral = createUnsafeIdentity()

  const { address, signer, hasConnectedWeb3, ephemeralLifespanMinutes } = await getSigner(requestManager, isGuest)

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)

  return { ...auth, rawAddress: address, address: address.toLowerCase(), hasConnectedWeb3 }
}

function* logout() {
  const identity: ExplorerIdentity | undefined = yield select(getCurrentIdentity)
  const network: ETHEREUM_NETWORK = yield select(getSelectedNetwork)
  if (identity && identity.address && network) {
    yield localProfilesRepo.remove(identity.address, network)
    globalObservable.emit('logout', { address: identity.address, network })
  }

  yield put(setRoomConnection(undefined))

  if (identity?.address) {
    yield call(removeStoredSession, identity.address)
  }
  window.location.reload()
}

function* redirectToSignUp() {
  window.location.search += '&show_wallet=1'
  window.location.reload()
}

export function observeAccountStateChange(
  store: Store<RootSessionState>,
  accountStateChange: (previous: SessionState, current: SessionState) => any
) {
  let previousState = store.getState().session

  store.subscribe(() => {
    const currentState = store.getState().session
    if (previousState !== currentState) {
      previousState = currentState
      accountStateChange(previousState, currentState)
    }
  })
}

export async function onLoginCompleted(): Promise<SessionState> {
  const state = store.getState()

  if (isLoginCompleted(state)) return state.session

  return new Promise<SessionState>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const state = store.getState()
      if (isLoginCompleted(state)) {
        unsubscribe()
        return resolve(state.session)
      }
    })
  })
}

export function initializeSessionObserver() {
  observeAccountStateChange(store, (_, session) => {
    globalObservable.emit('accountState', {
      hasProvider: !!session.provider,
      loginStatus: session.loginState as LoginState,
      // TODO: the authChain may have an undefined field. DecentralandIdentity got outdated
      identity: session.identity as any as DecentralandIdentity | undefined,
      network: session.network,
      isGuest: !!session.isGuestLogin
    })
  })
}
