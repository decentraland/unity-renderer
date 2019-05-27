import { Auth } from 'decentraland-auth'

import './apis/index'
import './events'

import { ETHEREUM_NETWORK, setNetwork, getTLD, DISABLE_AUTH, PREVIEW } from 'config'
import { info, error } from 'engine/logger'

import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { initializeUrlPositionObserver } from './world/positionThings'
import { connect } from './comms'
import { initialize } from './analytics'

// TODO fill with segment keys and integrate identity server
export async function initializeAnalytics(userId: string) {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc', userId)
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
    info(e)
    return
  }
}

async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await getNetwork()
  const web3net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  // TLD environment have priority
  const net = getNetworkFromTLD() || web3net

  if (web3net && net !== web3net) {
    // TODO @fmiras show an HTML error if web3 networks differs from domain network and do not load client at all
    error(`Switch to network ${net}`)
  }

  info('Using ETH network: ', net)
  return net
}

async function authenticate(): Promise<any> {
  if (DISABLE_AUTH || PREVIEW) {
    return { user_id: 'email|5cdd68572d5f842a16d6cc17' }
  }

  const auth = new Auth()
  await auth.login()
  return auth.getPayload()
}

export async function initShared(): Promise<ETHEREUM_NETWORK> {
  const { user_id } = await authenticate()
  console['log'](`User ${user_id} logged in`)
  const address = await getAddress()
  const net = await getAppNetwork()

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  await initializeAnalytics(user_id)
  await connect(
    user_id,
    net,
    address
  )
  initializeUrlPositionObserver()

  return net
}
