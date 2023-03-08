import { defaultLogger } from 'lib/logger'
import { queryGraph } from './queryGraph'

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
