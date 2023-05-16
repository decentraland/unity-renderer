import { Candidate, Parcel } from '../types'

export enum AlgorithmLinkTypes {
  LARGE_LATENCY = 'LARGE_LATENCY',
  CLOSE_PEERS_SCORE = 'CLOSE_PEERS_SCORE',
  ALL_PEERS_SCORE = 'ALL_PEERS_SCORE',
  LOAD_BALANCING = 'LOAD_BALANCING',
  VERSION_CATALYST = 'VERSION_CATALYST',
  FORCE_CATALYST = 'FORCE_CATALYST',
  OVERLOADED_CATALYST = 'OVERLOADED_CATALYST'
}

export type LargeLatencyConfig = {
  type: AlgorithmLinkTypes.LARGE_LATENCY
  config?: LargeLatencyParameters
}

export type LargeLatencyParameters = { largeLatencyThreshold: number }

export type ClosePeersScoreConfig = {
  type: AlgorithmLinkTypes.CLOSE_PEERS_SCORE
  config?: {
    /**
     * Distance in parcels to which a peer is considered close, so it can count for the score.
     */
    closePeersDistance?: number
    baseScore?: number
    /**
     * If the score difference between two candidates is greater than this value, we can make a definitive decision. Otherwise, we delegate to the next link
     */
    definitiveDecisionThreshold?: number
    latencyDeductionsParameters?: LatencyDeductionsConfig
  }
}

/**
 * Score deduced by latency, equivalent to users. This responds to the following formula: m * (e ^ (x / d) - 1)
 * Where m is the multiplier, e is Euler's number, x is the latency and d is the exponencialDivisor.
 * See here for a visualization of the formula: https://www.desmos.com/calculator/zflj2ik6pl
 * By default, these values are 60 for the multiplier, and 700 for the divisor, resulting, for example, in the following values:
 *
 * | latency | deduction |
 * | ------- | --------- |
 * | 500     | 62        |
 * | 750     | 115       |
 * | 1000    | 190       |
 * | 1250    | 300       |
 * | 1500    | 451       |
 * | 1750    | 670       |
 * | 2000    | 984       |
 *
 * If a maxDeduction is provided, then no more than that number of users will be deduced from the score.
 */
export type LatencyDeductionsParameters = {
  multiplier: number
  exponentialDivisor: number
  maxDeduction: number
}

export type LatencyDeductionsConfig = Partial<LatencyDeductionsParameters>

export type ClosePeersScoreParameters = Required<ClosePeersScoreConfig['config']> & {
  latencyDeductionsParameters: LatencyDeductionsParameters
}

export type LoadBalancingConfig = {
  type: AlgorithmLinkTypes.LOAD_BALANCING
}

export type AllPeersScoreConfig = {
  type: AlgorithmLinkTypes.ALL_PEERS_SCORE
  config?: {
    /** Base score for any realm that has at least 1 user. Default: 40 */
    baseScore?: number
    /** If the realm has maxUsers, the score will rise only until the target percentage of fullness represented by this value is reached */
    fillTargetPercentage?: number
    /** If the realm has maxUsers, the score will become baseScore when this percentage is reached */
    discourageFillTargetPercentage?: number
    /**
     * If the score difference between two candidates is greater than this value, we can make a definitive decision. Otherwise, we delegate to the next link
     */
    definitiveDecisionThreshold?: number
    latencyDeductionsParameters?: LatencyDeductionsConfig
  }
}

export type OverloadedCatalystConfig = {
  type: AlgorithmLinkTypes.OVERLOADED_CATALYST
}

export type AllPeersScoreParameters = Required<AllPeersScoreConfig['config']> & {
  latencyDeductionsParameters: Required<LatencyDeductionsConfig>
}

export type VersionCatalystConfig = {
  type: AlgorithmLinkTypes.VERSION_CATALYST
  config?: VersionCatalystParameters
}

/**
 * An object containing the minimum versions required
 * to be selected.
 *
 * @property {string} [content] - Minimum content version required
 * @property {string} [lambdas] - Minimum lambdas version required
 * @property {string} [comms] - Minimum comms protocol version required
 * @property {string} [bff] - Minimum bff version required
 */
export type VersionCatalystParameters = {
  content?: string
  lambdas?: string
  comms?: string
  bff?: string
}

export type ForceCatalystConfig = {
  type: AlgorithmLinkTypes.FORCE_CATALYST
  config: ForceCatalystParameters
}

export type ForceCatalystParameters = {
  sortedOptions?: string[]
}

export type AlgorithmLinkConfig = (
  | LargeLatencyConfig
  | AllPeersScoreConfig
  | ClosePeersScoreConfig
  | LoadBalancingConfig
  | VersionCatalystConfig
  | ForceCatalystConfig
  | OverloadedCatalystConfig
) & { name?: string }

export type AlgorithmChainConfig = AlgorithmLinkConfig[]

/**
 * The allCandidates attribute lists all candidates. The "picked" candidates is a sorted list of those candidates picked by all the previous links
 */
export type AlgorithmContext = {
  allCandidates: Candidate[]
  picked: Candidate[]
  userParcel: Parcel
  selected?: Candidate
}

export type AlgorithmLink = {
  name: string
  pick: (context: AlgorithmContext) => AlgorithmContext
}
