import { Candidate } from '../types'
import { ask } from './ping'
import { fetchCatalystStatus } from './status'

export async function fetchCatalystStatuses(
  nodes: { domain: string }[],
  denylistedCatalysts: string[],
  askFunction: typeof ask
): Promise<Candidate[]> {
  const results: Candidate[] = []

  await Promise.all(
    nodes.map(async (node) => {
      const result = await fetchCatalystStatus(node.domain, denylistedCatalysts, askFunction)
      if (result) {
        results.push(result)
      }
    })
  )

  return results
}
