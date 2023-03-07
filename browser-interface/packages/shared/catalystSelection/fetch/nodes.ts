import defaultLogger from 'lib/logger'
import { CatalystNode } from 'shared/types'
import { fetchCatalystNodesFromContract } from 'shared/web3/lib/fetchCatalystNodesFromDAO'

export async function fetchCatalystNodes(endpoint: string | undefined): Promise<CatalystNode[]> {
  if (endpoint) {
    try {
      const response = await fetch(endpoint)
      if (response.ok) {
        const nodes = await response.json()
        if (nodes.length) {
          return nodes.map((node: any) => ({ ...node, domain: node.address }))
        }
      } else {
        throw new Error('Response was not OK. Status was: ' + response.statusText)
      }
    } catch (e) {
      defaultLogger.warn(`Tried to fetch catalysts from ${endpoint} but failed. Falling back to DAO contract`, e)
    }
  }

  return fetchCatalystNodesFromContract()
}
