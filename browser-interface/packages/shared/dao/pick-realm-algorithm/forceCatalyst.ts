import { AlgorithmContext, AlgorithmLink, AlgorithmLinkTypes, ForceCatalystParameters } from './types'

export function forceCatalystLink(candidatesOptions: ForceCatalystParameters): AlgorithmLink {
  return {
    name: AlgorithmLinkTypes.FORCE_CATALYST,
    pick: (context: AlgorithmContext) => {
      const picked = context.picked

      const candidates = picked.map((candidate) => candidate.catalystName)
      const selected = candidatesOptions.sortedOptions?.find((candidate) => candidates.includes(candidate))

      context.selected = !!selected ? picked.find((candidate) => candidate.catalystName === selected) : undefined

      return context
    }
  }
}
