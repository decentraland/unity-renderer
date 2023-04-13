import { Vector2 } from '@dcl/ecs-math'
import { PositionReport } from 'shared/world/positionThings'
import { parcelSize, maxParcelX, maxParcelZ } from 'lib/decentraland/parcels/limits'
import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'

export type ParcelArray = [number, number]

export function positionHashRfc4(p: rfc4.Position) {
  const parcel = position2parcelRfc4(p)
  const x = (parcel.x + maxParcelX) >> 2
  const z = (parcel.y + maxParcelZ) >> 2
  return `${x}:${z}`
}

export function position2parcelRfc4(p: Readonly<rfc4.Position>): Vector2 {
  return new Vector2(Math.trunc(p.positionX / parcelSize), Math.trunc(p.positionZ / parcelSize))
}

export function sameParcel(p1: Vector2 | null, p2: Vector2 | null) {
  if (!p1 || !p2) {
    return false
  }
  return p1.x === p2.x && p1.y === p2.y
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
  const v1 = new Vector2(p1.positionX, p1.positionZ)
  const v2 = new Vector2(p2.positionZ, p2.positionZ)

  return v1.subtract(v2).lengthSquared()
}

export function gridSquareDistance(parcel1: ParcelArray, parcel2: ParcelArray): number {
  const xDiff = parcel1[0] - parcel2[0]
  const zDiff = parcel1[1] - parcel2[1]
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

export function rotateUsingQuaternion(pos: rfc4.Position, x: number, y: number, z: number): [number, number, number] {
  const qx = pos.rotationX
  const qy = pos.rotationY
  const qz = pos.rotationZ
  const qw = pos.rotationW

  // calculate quat * vector

  const ix = qw * x + qy * z - qz * y
  const iy = qw * y + qz * x - qx * z
  const iz = qw * z + qx * y - qy * x
  const iw = -qx * x - qy * y - qz * z

  // calculate result * inverse quat

  const rx = ix * qw + iw * -qx + iy * -qz - iz * -qy
  const ry = iy * qw + iw * -qy + iz * -qx - ix * -qz
  const rz = iz * qw + iw * -qz + ix * -qy - iy * -qx

  return [rx, ry, rz]
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
