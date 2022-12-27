import { Position3D } from 'shared/comms/v3/types'

export const DISCRETIZE_POSITION_INTERVALS = [32, 64, 80, 128, 160]
const MAX_UINT32 = 4294967295

/**
 * Calculates the discretized distance between position a and position b, using the provided intervals (DISCRETIZE_POSITION_INTERVALS as default)
 *
 * For instance, given the intervals [32, 64, 80], then we get the following values:
 * - distance(a, b) = 30 => 0
 * - distance(a, b) = 50 => 1
 * - distance(a, b) = 64 => 1
 * - distance(a, b) = 77 => 2
 * - distance(a, b) = 90 => 3
 * - distance(a, b) = 99999 => 3
 *
 * The @param intervals provided should be ordered from lower to greater
 */
export function discretizedPositionDistanceXZ(intervals: number[] = DISCRETIZE_POSITION_INTERVALS) {
  return (a: Position3D, b: Position3D) => {
    let dx = 0
    let dz = 0

    dx = a[0] - b[0]

    dz = a[2] - b[2]

    const squaredDistance = dx * dx + dz * dz

    const intervalIndex = intervals.findIndex((it) => squaredDistance <= it * it)

    return intervalIndex !== -1 ? intervalIndex : intervals.length
  }
}

export function randomUint32() {
  return Math.floor(Math.random() * MAX_UINT32)
}

/**
 * Picks the top `count` elements according to `criteria` from the array and returns them and the remaining elements. If the array
 * has less or equal elements than the amount required, then it returns a copy of the array sorted by `criteria`.
 */
export function pickBy<T>(array: T[], count: number, criteria: (t1: T, t2: T) => number): T[] {
  const sorted = array.sort(criteria)

  const selected = sorted.splice(0, count)

  return selected
}
