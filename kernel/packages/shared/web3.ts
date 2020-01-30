import { ethereumConfigurations, ETHEREUM_NETWORK, getTLD, getServerConfigurations } from '../config'

import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { queueTrackingEvent } from './analytics'
import { defaultLogger } from './logger'

import { WebsocketProvider } from 'web3x/providers'
import { Address } from 'web3x/address'
import { Eth } from 'web3x/eth'
import { Katalyst } from './dao/contracts/Katalyst'

async function getAddress(): Promise<string | undefined> {
  try {
    await awaitWeb3Approval()
    return await getUserAccount()
  } catch (e) {
    defaultLogger.info(e)
  }
}

export function getNetworkFromTLD(): ETHEREUM_NETWORK | null {
  const tld = getTLD()
  if (tld === 'zone') {
    return ETHEREUM_NETWORK.ROPSTEN
  }

  if (tld === 'today' || tld === 'org') {
    return ETHEREUM_NETWORK.MAINNET
  }

  return null
}

export async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
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

export async function initWeb3(): Promise<void> {
  const address = await getAddress()

  if (address) {
    defaultLogger.log(`Identifying address ${address}`)
    queueTrackingEvent('Use web3 address', { address })
  }
}

declare const ethereum: any

export async function fetchKatalystNodes() {
  const contractAddress = Address.fromString(getServerConfigurations().dao)
  let eth = Eth.fromCurrentProvider()

  if (!eth) {
    defaultLogger.info(`user denied account access to metamask, defaulting to infura mainnet node`)
    eth = new Eth(new WebsocketProvider(ethereumConfigurations[ETHEREUM_NETWORK.MAINNET].wss))
  } else {
    await ethereum.enable()
  }

  const accounts = await eth.getAccounts()
  defaultLogger.info(`accounts: `, accounts)

  defaultLogger.info(`eth: `, eth)
  const contract = new Katalyst(eth, contractAddress)

  // @ts-ignore
  const count = await contract.methods.katalystCount().call()
  // @ts-ignore
  const ids = await contract.methods.katalystIds(0).call()
  // @ts-ignore
  const url = await contract.methods.katalystById(ids).call()
}
