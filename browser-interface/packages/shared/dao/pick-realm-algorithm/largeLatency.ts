import { LargeLatencyParameters, AlgorithmLink, AlgorithmLinkTypes, AlgorithmContext } from './types'

export function largeLatencyLink({ largeLatencyThreshold }: LargeLatencyParameters): AlgorithmLink {
  return {
    name: AlgorithmLinkTypes.LARGE_LATENCY,
    pick: (context: AlgorithmContext) => {
      const sorted = context.picked.sort((a, b) => a.elapsed - b.elapsed)

      const minElapsed = sorted[0].elapsed

      context.picked = sorted.filter((it) => it.elapsed - minElapsed < largeLatencyThreshold)

      if (context.picked.length === 1) {
        context.selected = context.picked[0]
      }

      return context
    }
  }
}
