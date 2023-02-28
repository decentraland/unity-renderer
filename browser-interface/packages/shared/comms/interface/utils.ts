import { Vector2 } from '@dcl/ecs-math'
import { PositionReport } from 'shared/world/positionThings'
import { parcelSize } from 'lib/decentraland/parcels/limits'
import * as rfc4 from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'

export type ParcelArray = [number, number]

export function position2parcelRfc4(p: Readonly<rfc4.Position>): Vector2 {
  return new Vector2(Math.trunc(p.positionX / parcelSize), Math.trunc(p.positionZ / parcelSize))
}

export class CommunicationArea {
  public vMin: Vector2
  public vMax: Vector2

  constructor(center: Vector2, radius: number) {
    this.vMin = new Vector2(center.x - radius, center.y - radius)
    this.vMax = new Vector2(center.x + radius, center.y + radius)
  }

  public contains(p: rfc4.Position) {
    const parcel = position2parcelRfc4(p)
    const vMin = this.vMin
    const vMax = this.vMax
    const contains = parcel.x >= vMin.x && parcel.x <= vMax.x && parcel.y >= vMin.y && parcel.y <= vMax.y
    return contains
  }
}

export function squareDistanceRfc4(p1: rfc4.Position, p2: rfc4.Position): number {
  return squareDistance(p1.positionX, p1.positionZ, p2.positionX, p2.positionZ)
}
function gridSquareDistance(parcel1: ParcelArray, parcel2: ParcelArray): number {
  return squareDistance(parcel1[0], parcel1[1], parcel2[0], parcel2[1])
}
function squareDistance(x1: number, y1: number, x2: number, y2: number): number {
  const xDiff = x1 - x2
  const zDiff = y1 - y2
  return xDiff * xDiff + zDiff * zDiff
}

export function countParcelsCloseTo(origin: ParcelArray, parcels: ParcelArray[], distance: number = 3) {
  let close = 0

  const squaredDistance = distance * distance

  parcels.forEach((parcel) => {
    if (gridSquareDistance(origin, parcel) <= squaredDistance) {
      close += 1
    }
  })

  return close
}

export function positionReportToCommsPositionRfc4(report: Readonly<PositionReport>) {
  const p = {
    positionX: report.position.x,
    positionY: report.position.y - report.playerHeight,
    positionZ: report.position.z,
    rotationX: report.quaternion.x,
    rotationY: report.quaternion.y,
    rotationZ: report.quaternion.z,
    rotationW: report.quaternion.w,
    index: 0
  } as rfc4.Position

  return p
}
