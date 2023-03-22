import { VoiceSpatialParams } from './VoiceCommunicator'
import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { Quaternion, Vector3 } from '@dcl/ecs-math'

export function getSpatialParamsFor(position: rfc4.Position): VoiceSpatialParams {
  const orientation = Vector3.Backward().rotate(
    Quaternion.FromArray([position.rotationX, position.rotationY, position.rotationZ, position.rotationW])
  )

  return {
    position: [position.positionX, position.positionY, position.positionZ],
    orientation: [orientation.x, orientation.y, orientation.z]
  }
}
