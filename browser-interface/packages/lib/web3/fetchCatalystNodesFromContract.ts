import { CATALYSTS_FROM_DAO_CONTRACT, ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { bytesToHex, ContractFactory } from 'eth-connect'
import { retry } from 'lib/javascript/retry'
import { defaultLogger } from 'lib/logger'
import { isMainRealmEnabled } from 'shared/meta/selectors'
import { store } from 'shared/store/isolatedStore'
import { catalystABI } from './catalystABI'
import { getEthereumNetworkFromProvider } from './getEthereumNetworkFromProvider'
import { requestManager } from './provider'

export type CatalystNode = {
  domain: string
}

/**
 * The HARDCODED_CATALYST_LIST exists because this function was taking ~8s fetching all the data from the contract (at
 * least through metamask)
 */

export async function fetchCatalystNodesFromContract(): Promise<CatalystNode[]> {
  if (!requestManager.provider) {
    throw new Error('requestManager.provider not set')
  }

  const net = await getEthereumNetworkFromProvider()
  const catalysts = await fetchCatalysts(net)
  const state = store.getState()

  if (isMainRealmEnabled(state)) {
    if (net === ETHEREUM_NETWORK.MAINNET) {
      catalysts.push({ domain: 'https://realm-provider.decentraland.zone/main' }) //TODO use org instead
    } else if (net === ETHEREUM_NETWORK.SEPOLIA) {
      catalysts.push({ domain: 'https://realm-provider.decentraland.zone/main' })
    }
  }

  return catalysts
}

async function fetchCatalysts(net: ETHEREUM_NETWORK): Promise<CatalystNode[]> {
  if (!requestManager.provider) {
    throw new Error('requestManager.provider not set')
  }

  if (!CATALYSTS_FROM_DAO_CONTRACT) {
    let catalysts: CatalystNode[]
    if (net === ETHEREUM_NETWORK.MAINNET) {
      catalysts = [
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
    } else if (net === ETHEREUM_NETWORK.SEPOLIA) {
      catalysts = [
        { domain: 'https://peer.decentraland.zone' },
        { domain: 'https://peer-ap1.decentraland.zone' },
        { domain: 'https://peer-ue-2.decentraland.zone' }
      ]
    } else {
      catalysts = []
    }
    return catalysts
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
