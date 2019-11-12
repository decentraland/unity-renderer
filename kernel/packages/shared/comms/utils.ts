import { parcelLimits } from 'config'

export type Position = [number, number, number, number, number, number, number]

export class Parcel {
  constructor(public x: number, public z: number) {}
}

export function position2parcel(p: Position): Parcel {
  const parcelSize = parcelLimits.parcelSize
  return new Parcel(Math.trunc(p[0] / parcelSize), Math.trunc(p[2] / parcelSize))
}

export function sameParcel(p1: Parcel | null, p2: Parcel | null) {
  if (!p1 || !p2) {
    return false
  }
  return p1.x === p2.x && p1.z === p2.z
}

export class V2 {
  constructor(public x: number, public z: number) {}

  minus(a: V2) {
    return new V2(this.x - a.x, this.z - a.z)
  }

  innerProd(a: V2): number {
    return this.x * a.x + this.z * a.z
  }

  squareLenght(): number {
    return this.innerProd(this)
  }
}

export class CommunicationArea {
  public vMin: V2
  public vMax: V2

  constructor(center: Parcel, radius: number) {
    this.vMin = new V2(
      Math.max(parcelLimits.minParcelX, center.x - radius),
      Math.max(parcelLimits.minParcelZ, center.z - radius)
    )
    this.vMax = new V2(
      Math.min(parcelLimits.maxParcelX, center.x + radius),
      Math.min(parcelLimits.maxParcelZ, center.z + radius)
    )
  }

  public contains(p: Position) {
    const parcel = position2parcel(p)
    const vMin = this.vMin
    const vMax = this.vMax
    const contains = parcel.x >= vMin.x && parcel.x <= vMax.x && parcel.z >= vMin.z && parcel.z <= vMax.z
    return contains
  }
}

export function squareDistance(p1: Position, p2: Position): number {
  const v1 = new V2(p1[0], p1[2])
  const v2 = new V2(p2[0], p2[2])

  return v1.minus(v2).squareLenght()
}
