import { Candidate } from '../types'
import { AlgorithmContext, LatencyDeductionsParameters } from './types'

export function usersCount(candidate: Candidate) {
  return candidate.usersCount || 0
}

export function maxUsers(candidate: Candidate) {
  return candidate.maxUsers
}

export function usersParcels(candidate: Candidate) {
  return candidate.usersParcels
}

export function memoizedScores(scoreFunction: (c: Candidate) => number) {
  const scores = new Map<Candidate, number>()
  return (candidate: Candidate) => {
    if (!scores.has(candidate)) {
      scores.set(candidate, scoreFunction(candidate))
    }

    return scores.get(candidate)!
  }
}

export function latencyDeductions(
  candidate: Candidate,
  { multiplier, exponentialDivisor, maxDeduction }: LatencyDeductionsParameters
) {
  const expResult = multiplier * (Math.exp(candidate.elapsed / exponentialDivisor) - 1)
  return Math.min(expResult, maxDeduction)
}

export function scoreUsingLatencyDeductions(
  parameters: LatencyDeductionsParameters,
  baseScoreFunction: (c: Candidate) => number
) {
  return (candidate: Candidate) => {
    const scoreByUsers = baseScoreFunction(candidate)

    return scoreByUsers - latencyDeductions(candidate, parameters)
  }
}

export function defaultScoreAddons(
  latencyDeductionsParameters: LatencyDeductionsParameters,
  baseScore: number,
  baseScoreFunction: (c: Candidate) => number
) {
  return memoizedScores(
    penalizeFull(baseScore, scoreUsingLatencyDeductions(latencyDeductionsParameters, baseScoreFunction))
  )
}

export function penalizeFull(baseScore: number, baseScoreFunction: (c: Candidate) => number) {
  return (candidate: Candidate) => {
    const max = maxUsers(candidate)
    const count = usersCount(candidate)

    return max && count >= max ? -baseScore : baseScoreFunction(candidate)
  }
}

export function selectFirstByScore(
  context: AlgorithmContext,
  score: (c: Candidate) => number,
  almostEqualThreshold: number = 0,
  pickOnlyTheBest: boolean = false
) {
  const compareFn = (a: Candidate, b: Candidate) => score(b) - score(a)

  return selectFirstBy(
    context,
    compareFn,
    (a, b) => Math.abs(score(b) - score(a)) <= almostEqualThreshold,
    pickOnlyTheBest
  )
}

export function selectFirstBy(
  context: AlgorithmContext,
  compareFn: (a: Candidate, b: Candidate) => number,
  almostEqual: (a: Candidate, b: Candidate) => boolean = () => false,
  pickOnlyTheBest: boolean = false
) {
  const sorted = context.picked.sort(compareFn)

  // We pick those that are equivalent to the first one
  if (pickOnlyTheBest) {
    context.picked = sorted.filter((it) => compareFn(context.picked[0], it) === 0 || almostEqual(context.picked[0], it))
  }

  if (
    context.picked.length === 1 ||
    (compareFn(context.picked[0], context.picked[1]) < 0 && !almostEqual(context.picked[0], context.picked[1]))
  ) {
    context.selected = context.picked[0]
  }

  return context
}

export function isValidSemver(semver: string): boolean {
  const semverPattern = /^\d+\.\d+\.\d+$/
  return !!semver && semverPattern.test(semver)
}

export function isValidCommsProtocol(protocol: string): boolean {
  const commsProtocolPattern = /^v\d+$/
  return !!protocol && commsProtocolPattern.test(protocol)
}
