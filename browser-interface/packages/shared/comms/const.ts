import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { parcelSize } from 'lib/decentraland/parcels/limits'

export const MORDOR_POSITION_RFC4: rfc4.Position = {
  positionX: 1000 * parcelSize,
  positionY: 1000,
  positionZ: 1000 * parcelSize,
  rotationX: 0,
  rotationY: 0,
  rotationZ: 0,
  rotationW: 1,
  index: 0
}
