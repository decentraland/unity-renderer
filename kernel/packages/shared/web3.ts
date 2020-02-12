import { ETHEREUM_NETWORK, getTLD } from '../config'

import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { queueTrackingEvent } from './analytics'
import { defaultLogger } from './logger'

import { Address } from 'web3x/address'
import { Eth } from 'web3x/eth'
import { Catalyst } from './dao/contracts/Catalyst'
import { decentralandConfigurations } from '../config/index'
import { WebsocketProvider } from 'web3x/providers'
import { ethereumConfigurations } from 'config'

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

  // if localhost
  return null
}

export async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await getNetwork()
  const web3net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  defaultLogger.info('Using ETH network: ', web3net)
  return web3net
}

export async function initWeb3(): Promise<void> {
  const address = await getAddress()

  if (address) {
    defaultLogger.log(`Identifying address ${address}`)
    queueTrackingEvent('Use web3 address', { address })
  }
}

export async function fetchCatalystNodes() {
  const contractAddress = Address.fromString(decentralandConfigurations.dao)
  let eth = Eth.fromCurrentProvider()

  if (!eth) {
    const net = await getAppNetwork()
    const provider = new WebsocketProvider(ethereumConfigurations[net].wss)

    eth = new Eth(provider)
  }

  const contract = new Catalyst(eth, contractAddress)

  const count = Number.parseInt(await contract.methods.catalystCount().call(), 10)

  const nodes = []
  for (let i = 0; i < count; ++i) {
    const ids = await contract.methods.catalystIds(i).call()
    const node = await contract.methods.catalystById(ids).call()

    if (node.domain.startsWith('http://')) {
      defaultLogger.warn(`Catalyst node domain using http protocol, skipping ${node.domain}`)
      continue
    }

    if (!node.domain.startsWith('https://')) {
      node.domain = 'https://' + node.domain
    }

    nodes.push(node)
  }

  return nodes
}
