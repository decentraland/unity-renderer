import { CATALYSTS_FROM_DAO_CONTRACT, ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { bytesToHex, ContractFactory } from 'eth-connect'
import { retry } from 'lib/javascript/retry'
import { defaultLogger } from 'lib/logger'
import { catalystABI } from './web3/catalystABI'
import { requestManager } from './ethereum/provider'
import { CatalystNode, GraphResponse } from './types'
import { getEthereumNetworkFromProvider } from './getEthereumNetworkFromProvider'

declare let window: Window & {
  ethereum: any
}

/**
 * The HARDCODED_CATALYST_LIST exists because this function was taking ~8s fetching all the data from the contract (at
 * least through metamask)
 */
export async function fetchCatalystNodesFromDAO(): Promise<CatalystNode[]> {
  if (!requestManager.provider) {
    throw new Error('requestManager.provider not set')
  }

  const net = await getEthereumNetworkFromProvider()

  if (!CATALYSTS_FROM_DAO_CONTRACT) {
    if (net === ETHEREUM_NETWORK.MAINNET) {
      return [
        { domain: 'https://peer-wc1.decentraland.org' },
        { domain: 'https://interconnected.online' },
        { domain: 'https://peer-ec2.decentraland.org' },
        { domain: 'https://peer-ec1.decentraland.org' },
        { domain: 'https://peer.dclnodes.io' },
        { domain: 'https://peer-eu1.decentraland.org' },
        { domain: 'https://peer-ap1.decentraland.org' },
        { domain: 'https://peer.decentral.io' },
        { domain: 'https://peer.uadevops.com' },
        { domain: 'https://peer.kyllian.me' },
        { domain: 'https://peer.melonwave.com' }
      ]
    } else if (net === ETHEREUM_NETWORK.GOERLI) {
      return [
        { domain: 'https://peer.decentraland.zone' },
        { domain: 'https://peer-ap1.decentraland.zone' },
        { domain: 'https://peer-ue-2.decentraland.zone' },
        { domain: 'https://peer-ue-2.decentraland.zone' }
      ]
    }
  }

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
