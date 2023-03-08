import { defaultLogger } from 'lib/logger'
import { queryGraph } from './queryGraph'

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
