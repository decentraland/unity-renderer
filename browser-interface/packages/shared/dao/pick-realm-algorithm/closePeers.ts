import { countParcelsCloseTo } from '../../comms/interface/utils'
import { Parcel, Candidate } from '../types'
import { ClosePeersScoreParameters, AlgorithmLink, AlgorithmLinkTypes, AlgorithmContext } from './types'
import { usersParcels, selectFirstByScore, defaultScoreAddons } from './utils'

export function closePeersScoreLink({
  closePeersDistance,
  baseScore,
  latencyDeductionsParameters,
  definitiveDecisionThreshold
}: ClosePeersScoreParameters): AlgorithmLink {
  function closeUsersScore(currentParcel: Parcel) {
    return (candidate: Candidate) => {
      const parcels = usersParcels(candidate)
      if (parcels && parcels.length > 0) {
        return baseScore + countParcelsCloseTo(currentParcel, parcels, closePeersDistance)
      } else return 0
    }
  }

  return {
    name: AlgorithmLinkTypes.CLOSE_PEERS_SCORE,
    pick: (context: AlgorithmContext) => {
      const score = defaultScoreAddons(latencyDeductionsParameters, baseScore, closeUsersScore(context.userParcel))
      return selectFirstByScore(context, score, definitiveDecisionThreshold, true)
    }
  }
}
