import { ETHEREUM_NETWORK, ethereumConfigurations } from 'config'
import { defaultLogger } from 'lib/logger'
import { CatalystNode, GraphResponse } from './types'
import { retry } from 'lib/javascript/retry'
import { requestManager } from './ethereum/provider'
import { ContractFactory, bytesToHex } from 'eth-connect'

declare let window: Window & {
  ethereum: any
}

export async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await requestManager.net_version()
  const chainId = parseInt(web3Network, 10)
  const web3net = chainId === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.GOERLI
  return web3net
}

const catalystABI = [
  {
    constant: true,
    inputs: [{ name: '', type: 'address' }],
    name: 'owners',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'hasInitialized',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'catalystCount',
    outputs: [{ name: '', type: 'uint256' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '_script', type: 'bytes' }],
    name: 'getEVMScriptExecutor',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'getRecoveryVault',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'bytes32' }],
    name: 'catalystIndexById',
    outputs: [{ name: '', type: 'uint256' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '_id', type: 'bytes32' }],
    name: 'catalystOwner',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '_id', type: 'bytes32' }],
    name: 'catalystDomain',
    outputs: [{ name: '', type: 'string' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'uint256' }],
    name: 'catalystIds',
    outputs: [{ name: '', type: 'bytes32' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: 'token', type: 'address' }],
    name: 'allowRecoverability',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'appId',
    outputs: [{ name: '', type: 'bytes32' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: false,
    inputs: [],
    name: 'initialize',
    outputs: [],
    payable: false,
    stateMutability: 'nonpayable',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'getInitializationBlock',
    outputs: [{ name: '', type: 'uint256' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: false,
    inputs: [{ name: '_token', type: 'address' }],
    name: 'transferToVault',
    outputs: [],
    payable: false,
    stateMutability: 'nonpayable',
    type: 'function'
  },
  {
    constant: true,
    inputs: [
      { name: '_sender', type: 'address' },
      { name: '_role', type: 'bytes32' },
      { name: '_params', type: 'uint256[]' }
    ],
    name: 'canPerform',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'getEVMScriptRegistry',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: false,
    inputs: [{ name: '_id', type: 'bytes32' }],
    name: 'removeCatalyst',
    outputs: [],
    payable: false,
    stateMutability: 'nonpayable',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'bytes32' }],
    name: 'domains',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'bytes32' }],
    name: 'catalystById',
    outputs: [
      { name: 'id', type: 'bytes32' },
      { name: 'owner', type: 'address' },
      { name: 'domain', type: 'string' },
      { name: 'startTime', type: 'uint256' },
      { name: 'endTime', type: 'uint256' }
    ],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: false,
    inputs: [
      { name: '_owner', type: 'address' },
      { name: '_domain', type: 'string' }
    ],
    name: 'addCatalyst',
    outputs: [],
    payable: false,
    stateMutability: 'nonpayable',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'kernel',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'MODIFY_ROLE',
    outputs: [{ name: '', type: 'bytes32' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [],
    name: 'isPetrified',
    outputs: [{ name: '', type: 'bool' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    anonymous: false,
    inputs: [
      { indexed: true, name: '_id', type: 'bytes32' },
      { indexed: true, name: '_owner', type: 'address' },
      { indexed: false, name: '_domain', type: 'string' }
    ],
    name: 'AddCatalyst',
    type: 'event'
  },
  {
    anonymous: false,
    inputs: [
      { indexed: true, name: '_id', type: 'bytes32' },
      { indexed: true, name: '_owner', type: 'address' },
      { indexed: false, name: '_domain', type: 'string' }
    ],
    name: 'RemoveCatalyst',
    type: 'event'
  },
  {
    anonymous: false,
    inputs: [
      { indexed: true, name: 'executor', type: 'address' },
      { indexed: false, name: 'script', type: 'bytes' },
      { indexed: false, name: 'input', type: 'bytes' },
      { indexed: false, name: 'returnData', type: 'bytes' }
    ],
    name: 'ScriptResult',
    type: 'event'
  },
  {
    anonymous: false,
    inputs: [
      { indexed: true, name: 'vault', type: 'address' },
      { indexed: true, name: 'token', type: 'address' },
      { indexed: false, name: 'amount', type: 'uint256' }
    ],
    name: 'RecoverToVault',
    type: 'event'
  }
]

export async function fetchCatalystNodesFromDAO(): Promise<CatalystNode[]> {
  if (!requestManager.provider) {
    throw new Error('requestManager.provider not set')
  }

  const net = await getAppNetwork()

  const contract2: {
    catalystCount(): Promise<string>
    catalystIds(input: string | number): Promise<Uint8Array>
    catalystById(id: string | number): Promise<string>
  } = (await new ContractFactory(requestManager, catalystABI).at(ethereumConfigurations[net].CatalystProxy)) as any

  const count = Number.parseInt(await retry(() => contract2.catalystCount()), 10)
  const nodes: { domain: string }[] = []
  for (let i = 0; i < count; ++i) {
    const ids = '0x' + bytesToHex(await retry(() => contract2.catalystIds(i)))
    const node: any = await retry(() => contract2.catalystById(ids))
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
    return jsonResponse.nfts.map((nft) => nft.ens.subdomain)
  } catch (e) {
    // do nothing
  }
  return []
}

export async function fetchENSOwner(url: string, name: string) {
  const query = `
    query GetOwner($name: String!) {
      nfts(first: 1, where: { searchText: $name, category: ens  }) {
        owner{
          address
        }
      }
    }`

  const variables = { name: name.toLowerCase() }

  try {
    const resp = await queryGraph(url, query, variables)
    return resp.nfts.length === 1 ? (resp.nfts[0].owner.address as string) : null
  } catch (error) {
    defaultLogger.error(`Error querying graph`, error)
    throw error
  }
}

/**
 * Fetch owners of ENS (names) that contains string "name"
 * @param url query url
 * @param name string to query
 * @param maxResults max results expected (The Graph support up to 1000)
 */
export async function fetchENSOwnersContains(url: string, name: string, maxResults: number): Promise<string[]> {
  const query = `
    query GetOwner($name: String!, $maxResults: Int!) {
      nfts(first: $maxResults, where: { searchText_contains: $name, category: ens }) {
        owner{
          address
        }
      }
    }`

  const variables = { name: name.toLowerCase(), maxResults }

  try {
    const response = await queryGraph(url, query, variables)
    return response.nfts.map((nft: any) => nft.owner.address as string)
  } catch (error) {
    defaultLogger.error(`Error querying graph`, error)
    throw error
  }
}

async function queryGraph(url: string, query: string, variables: any, _totalAttempts: number = 5) {
  const ret = await fetch(url, {
    method: 'POST',
    body: JSON.stringify({ query, variables }),
    headers: { 'Content-Type': 'application/json' }
  })
  const response = await ret.json()
  if (response.errors) {
    throw new Error(`Error querying graph. Reasons: ${JSON.stringify(response.errors)}`)
  }
  return response.data
}

/**
 * Register to any change in the configuration of the wallet to reload the app and avoid wallet changes in-game.
 * TODO: move to explorer-website
 */
export function registerProviderNetChanges() {
  if (window.ethereum && typeof window.ethereum.on === 'function') {
    window.ethereum.on('chainChanged', () => location.reload())
  }
}
