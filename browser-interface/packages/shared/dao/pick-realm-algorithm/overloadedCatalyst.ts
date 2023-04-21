import { AlgorithmLink, AlgorithmLinkTypes, AlgorithmContext } from './types'

export function overloadedCatalystLink(): AlgorithmLink {
  return {
    name: AlgorithmLinkTypes.OVERLOADED_CATALYST,
    pick: (context: AlgorithmContext) => {
      // acceptingUsers is set by the BFF and it is false when:
      // memory/cpu is overloaded OR peer is at max capacity
      context.picked = context.picked.filter((node) => !!node.acceptingUsers)

      if (context.picked.length === 1) {
        context.selected = context.picked[0]
      }

      return context
    }
  }
}
