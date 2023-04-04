import { AlgorithmLink, AlgorithmLinkTypes, AlgorithmContext } from './types'
import { usersCount, selectFirstBy } from './utils'

export function loadBalancingLink(): AlgorithmLink {
  return {
    name: AlgorithmLinkTypes.LOAD_BALANCING,
    pick: (context: AlgorithmContext) => {
      const usersByDomain: Record<string, number> = {}

      context.picked.forEach((it) => {
        if (!usersByDomain[it.domain]) {
          usersByDomain[it.domain] = 0
        }

        usersByDomain[it.domain] += usersCount(it)
      })

      // We pick the realm whose domain has the least amount of users
      return selectFirstBy(context, (a, b) => usersByDomain[a.domain] - usersByDomain[b.domain])
    }
  }
}
