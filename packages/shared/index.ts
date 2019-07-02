import { Auth } from 'decentraland-auth'

import './apis/index'
import './events'

import { ETHEREUM_NETWORK, setNetwork, getTLD, PREVIEW, DEBUG, AVOID_WEB3 } from '../config'

import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { initializeUrlPositionObserver } from './world/positionThings'
import { connect } from './comms'
import { initialize, queueTrackingEvent } from './analytics'
import { defaultLogger } from './logger'

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

function getNetworkFromTLD(): ETHEREUM_NETWORK | null {
  const tld = getTLD()
  if (tld === 'zone') {
    return ETHEREUM_NETWORK.ROPSTEN
  }

  if (tld === 'today' || tld === 'org') {
    return ETHEREUM_NETWORK.MAINNET
  }

  return null
}

async function getAddress(): Promise<string | undefined> {
  try {
    await awaitWeb3Approval()
    return await getUserAccount()
  } catch (e) {
    defaultLogger.info(e)
  }
}

async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await getNetwork()
  const web3net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  // TLD environment have priority
  const net = getNetworkFromTLD() || web3net

  if (web3net && net !== web3net) {
    // TODO @fmiras show an HTML error if web3 networks differs from domain network and do not load client at all
    defaultLogger.error(`Switch to network ${net}`)
  }

  defaultLogger.info('Using ETH network: ', net)
  return net
}

export async function initShared(container: HTMLElement): Promise<ETHEREUM_NETWORK> {
  const auth = new Auth()

  let user_id: string

  console['group']('connect#login')

  if (PREVIEW) {
    defaultLogger.log(`Using test user.`)
    user_id = 'email|5cdd68572d5f842a16d6cc17'
  } else {
    await auth.login(container)
    try {
      const payload: any = await auth.getAccessTokenData()
      user_id = payload.user_id
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      throw new Error('Authentication error. Please reload the page to try again. (' + e.toString() + ')')
    }

    await initializeAnalytics(user_id)
  }

  defaultLogger.log(`User ${user_id} logged in`)

  console['groupEnd']()

  console['group']('connect#ethereum')
  const address = await getAddress()

  if (address) {
    defaultLogger.log(`Identifying address ${address}`)
    queueTrackingEvent('Use web3 address', { address })
  }

  const net = await getAppNetwork()
  queueTrackingEvent('Use network', { net })

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  console['groupEnd']()

  console['group']('connect#comms')
  await connect(
    user_id,
    net,
    auth,
    address
  )
  console['groupEnd']()

  initializeUrlPositionObserver()

  // Warn in case wallet is set in mainnet
  if (net === ETHEREUM_NETWORK.MAINNET && DEBUG && !AVOID_WEB3) {
    const style = document.createElement('style')
    style.appendChild(
      document.createTextNode(
        `body:before{content:'You are using Mainnet Ethereum Network, real transactions are going to be made.';background:#ff0044;color:#fff;text-align:center;text-transform:uppercase;height:24px;width:100%;position:fixed;padding-top:2px}#main-canvas{padding-top:24px};`
      )
    )
    document.head.appendChild(style)
  }

  return net
}
