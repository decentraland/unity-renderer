import { Auth } from './auth'

import './apis/index'
import './events'

import { ETHEREUM_NETWORK, setNetwork, getTLD, PREVIEW, DEBUG, ENABLE_WEB3 } from '../config'

import { initializeUrlPositionObserver } from './world/positionThings'
import { connect } from './comms'
import { initialize, queueTrackingEvent } from './analytics'
import { defaultLogger } from './logger'
import { initWeb3, getNetworkFromTLD, getAppNetwork } from './web3'
import { fetchProfile, generateRandomAvatarSpec, createProfile } from './world/profiles'

// TODO fill with segment keys and integrate identity server
export async function initializeAnalytics(userId: string) {
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

export async function initShared(container: HTMLElement): Promise<ETHEREUM_NETWORK> {
  const auth = new Auth()

  let userId: string

  console['group']('connect#login')

  if (PREVIEW) {
    defaultLogger.log(`Using test user.`)
    userId = 'email|5cdd68572d5f842a16d6cc17'
  } else {
    await auth.login(container)
    try {
      const payload: any = await auth.getAccessTokenData()
      userId = payload.user_id
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      throw new Error('Authentication error. Please reload the page to try again. (' + e.toString() + ')')
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

  console['group']('connect#comms')
  await connect(
    userId,
    net,
    auth
  )
  console['groupEnd']()

  initializeUrlPositionObserver()

  // Warn in case wallet is set in mainnet
  if (net === ETHEREUM_NETWORK.MAINNET && DEBUG && ENABLE_WEB3) {
    const style = document.createElement('style')
    style.appendChild(
      document.createTextNode(
        `body:before{content:'You are using Mainnet Ethereum Network, real transactions are going to be made.';background:#ff0044;color:#fff;text-align:center;text-transform:uppercase;height:24px;width:100%;position:fixed;padding-top:2px}#main-canvas{padding-top:24px};`
      )
    )
    document.head.appendChild(style)
  }

  // initialize profile
  console['group']('connect#profile')
  if (!PREVIEW) {
    let response
    try {
      response = await fetchProfile()
    } catch (e) {
      defaultLogger.error(`Not able to fetch profile for current user`)
    }

    if (!response || !response.ok) {
      defaultLogger.info(`Non existing profile, creating a random one`)
      const avatar = await generateRandomAvatarSpec()
      try {
        const creationResponse = await createProfile(avatar)
        defaultLogger.info(`New profile created with response ${creationResponse.status}`)
      } catch (e) {
        defaultLogger.error(`Error while creating profile`)
        defaultLogger.error(e)
      }
    }
  }
  console['groupEnd']()

  return net
}
