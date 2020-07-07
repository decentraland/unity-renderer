import { ethereumConfigurations } from 'config'
import { Address } from 'web3x/address'
import { Eth } from 'web3x/eth'
import { WebsocketProvider } from 'web3x/providers'
import { ETHEREUM_NETWORK, getTLD } from '../config'
import { decentralandConfigurations } from '../config/index'
import { queueTrackingEvent } from './analytics'
import { Catalyst } from './dao/contracts/Catalyst'
import { ERC721 } from './dao/contracts/ERC721'
import { getNetwork, getUserAccount } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { defaultLogger } from './logger'
import { CatalystNode, GraphResponse } from './types'

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

export async function hasClaimedName(address: string) {
  const dclNameContract = Address.fromString(decentralandConfigurations.DCLRegistrar)
  let eth = Eth.fromCurrentProvider()

  if (!eth) {
    const net = await getAppNetwork()
    const provider = new WebsocketProvider(ethereumConfigurations[net].wss)

    eth = new Eth(provider)
  }
  const contract = new ERC721(eth, dclNameContract)
  const balance = (await contract.methods.balanceOf(Address.fromString(address)).call()) as any
  if ((typeof balance === 'number' && balance > 0) || (typeof balance === 'string' && parseInt(balance, 10) > 0)) {
    return true
  } else {
    return false
  }
}

export async function fetchCatalystNodes(): Promise<CatalystNode[]> {
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

    // trim url in case it starts/ends with a blank
    node.domain = node.domain.trim()

    nodes.push(node)
  }

  return nodes
}

export async function fetchOwnedENS(theGraphBaseUrl: string, ethAddress: string): Promise<string[]> {
  const query = `
query GetNameByBeneficiary($beneficiary: String) {
  nfts(where: { owner: $beneficiary, category: ens }) {
    ens {
      labelHash
      beneficiary
      caller
      subdomain
      createdAt
    }
  }
}`

  const variables = { beneficiary: ethAddress.toLowerCase() }

  try {
    const jsonResponse: GraphResponse = await queryGraph(theGraphBaseUrl, query, variables)
    return jsonResponse.data.nfts.map((nft) => nft.ens.subdomain)
  } catch (e) {
    // do nothing
  }
  return []
}

export async function fetchOwner(url: string, name: string) {
  const query = `
    query GetOwner($name: String!) {
      nfts(first: 1, where: { searchText: $name }) {
        owner{
          address
        }
      }
    }`

  const variables = { name: name.toLowerCase() }

  try {
    const resp = await queryGraph(url, query, variables)
    return resp.data.nfts.length === 1 ? (resp.data.nfts[0].owner.address as string) : null
  } catch (error) {
    defaultLogger.error(`Error querying graph`, error)
    throw error
  }
}

async function queryGraph(url: string, query: string, variables: any, totalAttempts: number = 5) {
  const opts = {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query, variables })
  }

  for (let attempt = 0; attempt < totalAttempts; attempt++) {
    try {
      const res = await fetch(url, opts)
      return res.json()
    } catch (error) {
      defaultLogger.warn(`Could not query graph. Attempt ${attempt} of ${totalAttempts}.`, error)
    }
  }

  throw new Error(`Error while querying graph url=${url}, query=${query}, variables=${JSON.stringify(variables)}`)
}
