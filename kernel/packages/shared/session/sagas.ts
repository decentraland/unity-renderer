import { call, delay, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ENABLE_WEB3, ETHEREUM_NETWORK, PREVIEW, setNetwork, WORLD_EXPLORER } from 'config'

import { createLogger } from 'shared/logger'
import { initializeReferral, referUser } from 'shared/referral'
import {
  createEth,
  createWeb3Connector,
  getProviderType,
  getUserEthAccountIfAvailable,
  isGuest,
  isSessionExpired,
  loginCompleted,
  requestWeb3Provider
} from 'shared/ethereum/provider'
import { setLocalInformationForComms } from 'shared/comms/peers'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  AWAITING_USER_SIGNATURE,
  setLoadingScreen,
  setLoadingWaitTutorial
} from 'shared/loading/types'
import { identifyEmail, identifyUser, queueTrackingEvent } from 'shared/analytics'
import { checkTldVsWeb3Network, getAppNetwork } from 'shared/web3'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import {
  getLastSessionByProvider,
  getLastSessionWithoutWallet,
  getStoredSession,
  removeStoredSession,
  Session,
  setStoredSession
} from './index'
import { ExplorerIdentity, LoginStage, SignUpStage, StoredSession } from './types'
import {
  AUTHENTICATE,
  AuthenticateAction,
  changeLoginStage,
  changeSignUpStage,
  INIT_SESSION,
  loginCompleted as loginCompletedAction,
  LOGOUT,
  REDIRECT_TO_SIGN_UP,
  signInSetCurrentProvider,
  signInSigning,
  SIGNUP,
  SIGNUP_CANCEL,
  SIGNUP_COME_BACK_TO_AVATAR_EDITOR,
  signUpClearData,
  signUpSetIdentity,
  signUpSetIsSignUp,
  signUpSetProfile,
  toggleWalletPrompt,
  UPDATE_TOS,
  updateTOS,
  userAuthentified
} from './actions'
import { ProviderType } from '../ethereum/ProviderType'
import Html from '../Html'
import { fetchProfileLocally, doesProfileExist } from '../profiles/sagas'
import { generateRandomUserProfile } from '../profiles/generateRandomUserProfile'
import { unityInterface } from '../../unity-interface/UnityInterface'
import { getSignUpIdentity, getSignUpProfile } from './selectors'
import { ensureRealmInitialized } from '../dao/sagas'
import { ensureBaseCatalogs } from '../catalogs/sagas'
import { saveProfileRequest } from '../profiles/actions'
import { Profile } from '../profiles/types'

const TOS_KEY = 'tos'
const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initialize)
  yield call(initializeReferral)

  yield takeEvery(UPDATE_TOS, updateTermOfService)
  yield takeLatest(INIT_SESSION, initSession)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(REDIRECT_TO_SIGN_UP, redirectToSignUp)
  yield takeLatest(SIGNUP, signUp)
  yield takeLatest(SIGNUP_CANCEL, cancelSignUp)
  yield takeLatest(AUTHENTICATE, authenticate)
  yield takeLatest(AWAITING_USER_SIGNATURE, scheduleAwaitingSignaturePrompt)
  yield takeLatest(SIGNUP_COME_BACK_TO_AVATAR_EDITOR, showAvatarEditor)
}

function* initialize() {
  const tosAgreed: boolean = !!getFromLocalStorage(TOS_KEY)
  yield put(updateTOS(tosAgreed))
  Html.initializeTos(tosAgreed)
}

function* updateTermOfService(action: any) {
  return saveToLocalStorage(TOS_KEY, action.payload)
}

function* scheduleAwaitingSignaturePrompt() {
  yield delay(10000)
  const isStillWaiting = yield select((state) => !state.session?.initialized)

  if (isStillWaiting) {
    yield put(toggleWalletPrompt(true))
    Html.showAwaitingSignaturePrompt(true)
  }
}

function* initSession() {
  yield ensureRealmInitialized()
  yield createWeb3Connector()
  if (ENABLE_WEB3) {
    Html.showEthLogin()
    yield checkPreviousSession()
  } else {
    yield previewAutoSignIn()
  }
  yield put(changeLoginStage(LoginStage.SIGN_IN))
  Html.bindLoginEvent()
}

function* checkPreviousSession() {
  const session = getLastSessionWithoutWallet()
  if (!isSessionExpired(session) && session) {
    const identity = session.identity
    if (identity?.provider && identity.provider !== ProviderType.GUEST) {
      yield put(signInSetCurrentProvider(identity.provider))
    }
  } else {
    removeStoredSession(session?.userId)
  }
}

function* previewAutoSignIn() {
  yield requestProvider(ProviderType.GUEST)
  const session = yield authorize()
  yield signIn(session.userId, session.identity)
}

function* authenticate(action: AuthenticateAction) {
  yield put(signInSigning(true))
  const provider = yield requestProvider(action.payload.provider as ProviderType)
  if (!provider) {
    yield put(signInSigning(false))
    return
  }
  const session = yield authorize()
  let profileExists = yield doesProfileExist(session.userId)

  if (profileExists || isGuestWithProfile(session) || PREVIEW) {
    return yield signIn(session.userId, session.identity)
  }
  return yield startSignUp(session.userId, session.identity)
}

function isGuestWithProfile(session: StoredSession) {
  const profile = fetchProfileLocally(session.userId)
  return isGuest() && profile
}

function* startSignUp(userId: string, identity: ExplorerIdentity) {
  yield put(signUpSetIsSignUp(true))
  let prevGuest = fetchProfileLocally(userId)
  let profile: Profile = prevGuest ? prevGuest : yield generateRandomUserProfile(userId)
  profile.userId = identity.address
  profile.ethAddress = identity.rawAddress
  profile.unclaimedName = '' // clean here to allow user complete in passport step
  profile.hasClaimedName = false
  profile.version = 0

  yield put(signUpSetIdentity(userId, identity))
  yield put(signUpSetProfile(profile))

  if (prevGuest) {
    return yield signUp()
  }
  yield showAvatarEditor()
}

function* showAvatarEditor() {
  yield put(setLoadingScreen(true))
  yield put(changeLoginStage(LoginStage.SIGN_UP))
  yield put(changeSignUpStage(SignUpStage.AVATAR))

  const profile = yield select(getSignUpProfile)

  yield ensureBaseCatalogs()
  unityInterface.LoadProfile(profile)
  unityInterface.ShowAvatarEditorInSignIn()
  yield put(setLoadingScreen(false))
  Html.switchGameContainer(true)
}

function* requestProvider(providerType: ProviderType) {
  const provider = yield requestWeb3Provider(providerType)
  if (provider) {
    if (WORLD_EXPLORER && (yield checkTldVsWeb3Network())) {
      throw new Error('Network mismatch')
    }

    if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (yield getAppNetwork())) {
      Html.showNetworkWarning()
    }
  }
  return provider
}

function* authorize() {
  if (ENABLE_WEB3) {
    try {
      let userData

      if (isGuest()) {
        userData = getLastSessionByProvider(ProviderType.GUEST)
      } else {
        const ethAddress = yield getUserEthAccountIfAvailable(true)
        const address = ethAddress.toLocaleLowerCase()

        userData = getStoredSession(address)

        if (userData) {
          // We save the raw ethereum address of the current user to avoid having to convert-back later after lowercasing it for the userId
          userData.identity.rawAddress = ethAddress
        }
      }

      // check that user data is stored & key is not expired
      if (!userData || isSessionExpired(userData)) {
        const identity = yield createAuthIdentity()
        return {
          userId: identity.address,
          identity
        }
      }

      return {
        userId: userData.identity.address,
        identity: userData.identity
      }
    } catch (e) {
      logger.error(e)
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    logger.log(`Using test user.`)
    const identity = yield createAuthIdentity()
    const session = { userId: identity.address, identity }
    saveSession(session.userId, session.identity)
    return session
  }
}

function* signIn(userId: string, identity: ExplorerIdentity) {
  logger.log(`User ${userId} logged in`)
  yield put(changeLoginStage(LoginStage.COMPLETED))

  saveSession(userId, identity)
  if (identity.hasConnectedWeb3) {
    identifyUser(userId)
    referUser(identity)
  }

  yield put(signInSigning(false))
  yield setUserAuthentified(userId, identity)

  loginCompleted.resolve()
  yield put(loginCompletedAction())
}

function* setUserAuthentified(userId: string, identity: ExplorerIdentity) {
  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    queueTrackingEvent('Use network', { net })
  }

  yield put(userAuthentified(userId, identity, net))
}

function* signUp() {
  yield put(setLoadingScreen(true))
  yield put(changeLoginStage(LoginStage.COMPLETED))
  const session = yield select(getSignUpIdentity)

  logger.log(`User ${session.userId} signed up`)

  const profile = yield select(getSignUpProfile)
  profile.userId = session.userId
  profile.ethAddress = session.identity.rawAddress
  profile.version = 0
  profile.hasClaimedName = false
  if (profile.email) {
    identifyEmail(profile.email, isGuest() ? undefined : profile.userId)
    profile.tutorialStep |= 128 // We use binary 256 for tutorial and 128 for email promp
  }
  delete profile.email // We don't deploy the email because it is public

  yield put(setLoadingWaitTutorial(true))
  yield signIn(session.userId, session.identity)
  yield put(saveProfileRequest(profile, session.userId))
  yield put(signUpClearData())
}

function* cancelSignUp() {
  yield put(signUpClearData())
  yield put(signInSigning(false))
  yield put(signUpSetIsSignUp(false))
  yield put(changeLoginStage(LoginStage.SIGN_IN))
}

function saveSession(userId: string, identity: ExplorerIdentity) {
  setStoredSession({
    userId,
    identity
  })

  setLocalInformationForComms(userId, {
    userId,
    identity
  })
}

async function createAuthIdentity(): Promise<ExplorerIdentity> {
  const ephemeral = createIdentity()

  let ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  let provider: ProviderType | undefined
  let hasConnectedWeb3 = false

  if (ENABLE_WEB3 && !isGuest()) {
    const eth = createEth()!
    const account = (await eth.getAccounts())[0]

    address = account.toJSON()
    provider = getProviderType()
    signer = async (message: string) => {
      let result
      while (!result) {
        try {
          result = await new Personal(eth.provider).sign(message, account, '')
        } catch (e) {
          if (e.message && e.message.includes('User denied message signature')) {
            put(changeLoginStage(LoginStage.SIGN_ADVICE))
            Html.showEthSignAdvice(true)
          }
        }
      }
      put(changeLoginStage(LoginStage.COMPLETED))
      Html.showEthSignAdvice(false)
      return result
    }
    hasConnectedWeb3 = true
  } else {
    const account: Account = Account.create()

    provider = ProviderType.GUEST
    address = account.address.toJSON()
    signer = async (message: string) => account.sign(message).signature

    // If we are using a local profile, we don't want the identity to expire.
    // Eventually, if a wallet gets created, we can migrate the profile to the wallet.
    ephemeralLifespanMinutes = 365 * 24 * 60 * 99
  }

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)

  return { ...auth, rawAddress: address, address: address.toLocaleLowerCase(), hasConnectedWeb3, provider }
}

function* logout() {
  Session.current.logout().catch((e) => logger.error('error while logging out', e))
}

function* redirectToSignUp() {
  Session.current.redirectToSignUp().catch((e) => logger.error('error while redirecting to sign up', e))
}
