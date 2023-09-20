import { Vector3 } from 'lib/math/Vector3'
import * as rfc4 from '../../../../protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { Entity } from '../engine/entity'
import { Sdk7ComponentIds } from './../avatar/ecs'
import { PutComponentOperation } from './../serialization/crdt/putComponent'
import { ReadWriteByteBuffer } from './ByteBuffer'

const crdtReusableMessage = new ReadWriteByteBuffer()
const transformReusableMessage = new ReadWriteByteBuffer()

export function buildAvatarTransformMessage(entity: Entity, timestamp: number, data: rfc4.Position, offset: Vector3) {
  transformReusableMessage.resetBuffer()
  transformReusableMessage.incrementWriteOffset(44)

  transformReusableMessage.setFloat32(0, data.positionX - offset.x)
  transformReusableMessage.setFloat32(4, data.positionY)
  transformReusableMessage.setFloat32(8, data.positionZ - offset.z)

  transformReusableMessage.setFloat32(12, data.rotationX)
  transformReusableMessage.setFloat32(16, data.rotationY)
  transformReusableMessage.setFloat32(20, data.rotationZ)
  transformReusableMessage.setFloat32(24, data.rotationW)

  transformReusableMessage.setFloat32(28, 1.0)
  transformReusableMessage.setFloat32(32, 1.0)
  transformReusableMessage.setFloat32(36, 1.0)
  transformReusableMessage.setUint32(40, 0)

  crdtReusableMessage.resetBuffer()
  PutComponentOperation.write(
    entity,
    timestamp,
    Sdk7ComponentIds.TRANSFORM,
    transformReusableMessage.toBinary(),
    crdtReusableMessage
  )

  return crdtReusableMessage.toCopiedBinary()
}
