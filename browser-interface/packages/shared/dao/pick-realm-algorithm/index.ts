import { Candidate, Parcel } from '../types'
import { defaultAllPeersScoreConfig, defaultClosePeersScoreConfig, defaultLargeLatencyConfig } from './defaults'
import { AlgorithmChainConfig, AlgorithmContext, AlgorithmLink, AlgorithmLinkConfig, AlgorithmLinkTypes } from './types'
import { defaultLogger } from 'lib/logger'
import { largeLatencyLink } from './largeLatency'
import { closePeersScoreLink } from './closePeers'
import { allPeersScoreLink } from './allPeers'
import { loadBalancingLink } from './loadBalancing'
import { versionCatalystLink } from './versionCatalyst'
import { forceCatalystLink } from './forceCatalyst'
import { overloadedCatalystLink } from './overloadedCatalyst'
import { trackEvent } from 'shared/analytics/trackEvent'

function buildLink(linkConfig: AlgorithmLinkConfig) {
  switch (linkConfig.type) {
    case AlgorithmLinkTypes.FORCE_CATALYST: {
      return forceCatalystLink({ ...linkConfig.config })
    }
    case AlgorithmLinkTypes.VERSION_CATALYST: {
      return versionCatalystLink({ ...linkConfig.config })
    }
    case AlgorithmLinkTypes.OVERLOADED_CATALYST: {
      return overloadedCatalystLink()
    }
    case AlgorithmLinkTypes.LARGE_LATENCY: {
      return largeLatencyLink({ ...defaultLargeLatencyConfig, ...linkConfig.config })
    }
    case AlgorithmLinkTypes.CLOSE_PEERS_SCORE: {
      return closePeersScoreLink({
        ...defaultClosePeersScoreConfig,
        ...linkConfig.config,
        latencyDeductionsParameters: {
          ...defaultClosePeersScoreConfig.latencyDeductionsParameters,
          ...linkConfig.config?.latencyDeductionsParameters
        }
      })
    }
    case AlgorithmLinkTypes.ALL_PEERS_SCORE: {
      return allPeersScoreLink({
        ...defaultAllPeersScoreConfig,
        ...linkConfig.config,
        latencyDeductionsParameters: {
          ...defaultAllPeersScoreConfig.latencyDeductionsParameters,
          ...linkConfig.config?.latencyDeductionsParameters
        }
      })
    }
    case AlgorithmLinkTypes.LOAD_BALANCING: {
      return loadBalancingLink()
    }
  }
}

function buildChain(config: AlgorithmChainConfig) {
  return config.map((linkConfig) => {
    const link = buildLink(linkConfig)

    if (linkConfig.name) {
      link.name = linkConfig.name
    }

    return link
  })
}

export function createAlgorithm(config: AlgorithmChainConfig) {
  const chain: AlgorithmLink[] = buildChain(config)

  return {
    pickCandidate(candidates: Candidate[], userParcel: Parcel) {
      if (candidates.length === 0) throw new Error('Cannot pick candidates from an empty list')

      let context: AlgorithmContext = { allCandidates: candidates, picked: candidates, userParcel }

      for (const link of chain) {
        context = link.pick(context)

        // If a link picks a particular candidate, we return that
        if (context.selected) {
          defaultLogger.log(`Picked candidate using algorithm link: ${link.name}`, context.selected)

          trackEvent('pickedRealm', {
            algorithm: link.name,
            domain: context.selected.domain
          })

          return context.selected
        }

        if (context.picked.length === 0) {
          defaultLogger.warn(
            'Trying to pick realms, a link in the chain filtered all the candidates. The first candidate will be picked.',
            candidates[0],
            link,
            context
          )
          break
        }
      }

      // If all the links have gone through, and we don't have a clear candidate, we pick the first
      if (context.picked[0]) {
        defaultLogger.log(
          `Picked candidate by most valued. Last link: ${chain[chain.length - 1]?.name}`,
          context.picked[0]
        )
        return context.picked[0]
      }

      return candidates[0]
    }
  }
}
