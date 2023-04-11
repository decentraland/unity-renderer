import { AlgorithmContext } from 'shared/dao/pick-realm-algorithm/types'
import { Candidate, Parcel, ServerConnectionStatus } from 'shared/dao/types'

// Some random parcels to use for fixture, but hardcoded to make tests repeatable
const someRandomParcels: Parcel[] = [
  [20, -26],
  [-25, -15],
  [25, 23],
  [-19, -25],
  [24, -22],
  [-13, -8],
  [10, -13],
  [19, 8],
  [-6, -25],
  [15, 21],
  [-9, -21],
  [-14, 16],
  [6, 15],
  [10, -10],
  [-22, 3],
  [10, -6],
  [7, -29],
  [11, 29],
  [14, -28],
  [-9, 25],
  [23, -22],
  [23, -2],
  [-28, 24],
  [-28, 18],
  [-29, 1],
  [10, -9],
  [27, -25],
  [-28, 10],
  [15, 27],
  [-24, 9],
  [18, 4],
  [7, 20],
  [-24, 5],
  [-16, 19],
  [-24, 13],
  [-11, -13],
  [-7, -25],
  [-12, 10],
  [17, 1],
  [-28, -29],
  [1, -10],
  [-2, -17],
  [5, 19],
  [-23, 16],
  [21, -18],
  [24, -23],
  [-7, 6],
  [4, -22],
  [-4, 9],
  [-22, 4],
  [-22, 2],
  [14, -11],
  [29, 1],
  [15, -4],
  [21, -11],
  [-11, 14],
  [-16, -9],
  [5, 24],
  [28, 21],
  [26, 27],
  [15, 14],
  [-1, -3],
  [20, 9],
  [20, -7],
  [-27, -5],
  [-15, 4],
  [-30, 14],
  [8, 22],
  [13, 17],
  [13, 1],
  [11, -29],
  [7, -12],
  [-22, 12],
  [25, -17],
  [-9, -12],
  [-18, -21],
  [6, 0],
  [23, 0],
  [-3, 28],
  [11, 20],
  [-3, 23],
  [6, 8],
  [8, -17],
  [-1, -17],
  [-15, 24],
  [26, -24],
  [23, -20],
  [15, -27],
  [-6, -9],
  [-3, -10],
  [22, 0],
  [-13, 16],
  [-6, -11],
  [-26, 23],
  [11, -5],
  [-4, -14],
  [12, -27],
  [-23, 28],
  [9, 4],
  [0, -4]
]

function circularSlice<T>(count: number, source: T[]): T[] {
  return [...new Array(count).keys()].map((i) => source[i % source.length])
}

function buildCandidate(params: Partial<Candidate>): Candidate {
  const usersCount = params.usersCount ? params.usersCount : 10
  return {
    protocol: 'v2',
    catalystName: 'test',
    domain: 'foo.bar',
    elapsed: 200,
    status: ServerConnectionStatus.OK,
    usersCount,
    version: { content: '1.0.0', lambdas: '1.0.0', bff: '1.0.0', comms: 'v3' },
    acceptingUsers: true,
    usersParcels: circularSlice(usersCount, someRandomParcels),
    ...params
  }
}

export function buildCandidates(...manyParams: Partial<Candidate>[]): Candidate[] {
  return manyParams.map(buildCandidate)
}

export function contextForCandidates(userParcel: Parcel, ...manyParams: Partial<Candidate>[]): AlgorithmContext {
  const candidates = buildCandidates(...manyParams)

  return { allCandidates: candidates, picked: candidates, userParcel }
}
