import { CatalystNode } from '../../types'
import { PIN_CATALYST } from 'config'
import { fetchCatalystNodes } from './nodes'

export async function listAvailableCatalysts(nodesEndpoint: string | undefined): Promise<CatalystNode[]> {
  const nodes: CatalystNode[] = PIN_CATALYST ? [{ domain: PIN_CATALYST }] : await fetchCatalystNodes(nodesEndpoint)
  if (nodes.length === 0) {
    throw new Error('no nodes are available in the DAO for the current network')
  }
  return nodes
}
