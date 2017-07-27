import { parcelLimits } from 'config'
import { MessageType, GenericMessage } from '../../shared/comms/worldcomm_pb'

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

  constructor(center: V2, radius: number) {
    this.vMin = new V2(
      Math.max(parcelLimits.minParcelX, center.x - radius),
      Math.max(parcelLimits.minParcelZ, center.z - radius)
    )
    this.vMax = new V2(
      Math.min(parcelLimits.maxParcelX, center.x + radius),
      Math.min(parcelLimits.maxParcelZ, center.z + radius)
    )
  }

  public contains(p: V2) {
    const vMin = this.vMin
    const vMax = this.vMax
    const contains = p.x >= vMin.x && p.x <= vMax.x && p.z >= vMin.z && p.z <= vMax.z
    return contains
  }
}

export function decodeMessageHeader(data: Uint8Array): [MessageType, number] {
  try {
    const msg = GenericMessage.deserializeBinary(data)
    const msgType = msg.getType()
    const timestamp = msg.getTime()
    return [msgType, timestamp]
  } catch (e) {
    return [MessageType.UNKNOWN_MESSAGE_TYPE, 0]
  }
}
