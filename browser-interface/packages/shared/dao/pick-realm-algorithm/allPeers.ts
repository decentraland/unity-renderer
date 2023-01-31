import { Candidate } from '../types'
import { AlgorithmContext, AlgorithmLink, AlgorithmLinkTypes, AllPeersScoreParameters } from './types'
import { usersCount, maxUsers, selectFirstByScore, defaultScoreAddons } from './utils'

export function allPeersScoreLink({
  baseScore,
  discourageFillTargetPercentage,
  fillTargetPercentage,
  latencyDeductionsParameters,
  definitiveDecisionThreshold
}: AllPeersScoreParameters): AlgorithmLink {
  function usersScore(candidate: Candidate) {
    const count = usersCount(candidate)
    const max = maxUsers(candidate)

    // We prefer realms that have users. Those will have at least baseScore
    if (count === 0) return 0

    const linearUsersScore = (users: number) => baseScore + users

    if (max) {
      // We try to fill all realms until around the percentage provided
      if (count >= fillTargetPercentage * max) {
        // If this is the case, we are in the "downward" phase of the score
        // We calculate a segment joining the fillTargetPercentage% of users with baseScore at discourageFillTargetPercentage% maxUsers
        // In that way, when we reach discourageFillTargetPercentage% maxUsers, realms that have at least one user start to get prioritized
        const segment = {
          start: { x: fillTargetPercentage * max, y: linearUsersScore(fillTargetPercentage * max) },
          end: { x: discourageFillTargetPercentage * max, y: baseScore }
        }

        const slope = (segment.end.y - segment.start.y) / (segment.end.x - segment.start.x)

        // The score is the result of calculating the corresponding point of this segment at usersCount
        return segment.start.y + slope * (count - segment.start.x)
      }
    }

    return linearUsersScore(count)
  }

  return {
    name: AlgorithmLinkTypes.ALL_PEERS_SCORE,
    pick: (context: AlgorithmContext) => {
      const score = defaultScoreAddons(latencyDeductionsParameters, baseScore, usersScore)

      return selectFirstByScore(context, score, definitiveDecisionThreshold, true)
    }
  }
}
