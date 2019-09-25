import {
  ENABLE_WEB3,
  ETHEREUM_NETWORK,
  getLoginConfigurationForCurrentDomain,
  getTLD,
  PREVIEW,
  setNetwork,
  STATIC_WORLD
} from '../config'
import { initialize, queueTrackingEvent } from './analytics'
import './apis/index'
import { Auth } from './auth'
import { connect, disconnect } from './comms'
import { isMobile } from './comms/mobile'
import { persistCurrentUser } from './comms/index'
import { localProfileUUID } from './comms/peers'
import './events'
import { ReportFatalError } from './loading/ReportFatalError'
import { AUTH_ERROR_LOGGED_OUT, COMMS_COULD_NOT_BE_ESTABLISHED, MOBILE_NOT_SUPPORTED } from './loading/types'
import { defaultLogger } from './logger'
import { Session } from './session/index'
import { ProfileSpec } from './types'
import { getAppNetwork, getNetworkFromTLD, initWeb3 } from './web3'
import { initializeUrlPositionObserver } from './world/positionThings'
import {
  createProfile,
  createStubProfileSpec,
  fetchLegacy,
  fetchProfile,
  legacyToSpec,
  resolveProfileSpec
} from './world/profiles'

// TODO fill with segment keys and integrate identity server
async function initializeAnalytics(userId: string) {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      return initialize('1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU', userId)
    case 'today':
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc', userId)
    case 'zone':
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc', userId)
    default:
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc', userId)
  }
}

declare let window: any

export async function initShared(): Promise<Session | undefined> {
  if (isMobile()) {
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return undefined
  }
  const session = new Session()

  const auth = new Auth({
    ...getLoginConfigurationForCurrentDomain(),
    redirectUri: window.location.href,
    ephemeralKeyTTL: 24 * 60 * 60 * 1000
  })
  session.auth = auth

  let userId: string

  console['group']('connect#login')

  if (PREVIEW) {
    defaultLogger.log(`Using test user.`)
    userId = 'email|5cdd68572d5f842a16d6cc17'
  } else {
    auth.setup()
    try {
      userId = await auth.getUserId()
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
    await initializeAnalytics(userId)
  }

  defaultLogger.log(`User ${userId} logged in`)

  console['groupEnd']()

  console['group']('connect#ethereum')

  let net: ETHEREUM_NETWORK

  if (ENABLE_WEB3) {
    await initWeb3()
    net = await getAppNetwork()
  } else {
    net = getNetworkFromTLD() || ETHEREUM_NETWORK.MAINNET
  }

  queueTrackingEvent('Use network', { net })

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  console['groupEnd']()

  initializeUrlPositionObserver()

  // DCL Servers connections/requests after this
  if (STATIC_WORLD) {
    return session
  }

  console['group']('connect#comms')
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      defaultLogger.info(`Try number ${i}...`)
      await connect(
        userId,
        net,
        auth
      )
      break
    } catch (e) {
      if (!e.message.startsWith('Communications link') || i >= maxAttemps) {
        // max number of attemps reached, rethrow error
        defaultLogger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
        disconnect()
        ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
        throw e
      }
    }
  }
  console['groupEnd']()

  // initialize profile
  console['group']('connect#profile')
  if (!PREVIEW) {
    let response
    try {
      response = await fetchProfile(await auth.getAccessToken(), userId)
    } catch (e) {
      defaultLogger.error(`Not able to fetch profile for current user`)
    }

    let spec: ProfileSpec
    if (!response || !response.ok) {
      const legacy = await fetchLegacy(await auth.getAccessToken(), '')
      if (legacy.ok) {
        spec = legacyToSpec((await legacy.json()).data)
      } else {
        defaultLogger.info(`Non existing avatar, creating a random one`)
        spec = await createStubProfileSpec()
      }
    } else if (response && response.ok) {
      spec = await response.json()
    } else {
      defaultLogger.info(`Non existing profile, creating a random one`)
      spec = await createStubProfileSpec()

      const avatar = spec.avatar
      try {
        const creationResponse = await createProfile(await auth.getAccessToken(), avatar)
        defaultLogger.info(`New profile created with response ${creationResponse.status}`)
      } catch (e) {
        defaultLogger.error(`Error while creating profile`)
        defaultLogger.error(e)
      }
    }
    const profile = await resolveProfileSpec(localProfileUUID!, spec!, await auth.getEmail())
    persistCurrentUser({ userId: localProfileUUID!, version: profile.version, profile })
  }
  console['groupEnd']()

  return session
}
