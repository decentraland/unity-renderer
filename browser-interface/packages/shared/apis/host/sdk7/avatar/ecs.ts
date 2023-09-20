import { Entity } from '../engine/entity'
import { ReadWriteByteBuffer } from '../serialization/ByteBuffer'
import { PutComponentOperation } from '../serialization/crdt/putComponent'
import { AppendValueOperation } from '../serialization/crdt/appendValue'
import { DeleteEntity } from '../serialization/crdt/deleteEntity'
// import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
// import { PBAvatarEmoteCommand } from 'shared/protocol/out-ts/decentraland/sdk/components/avatar_emote_command.gen.ts'

import * as rfc4 from '../../../../protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { PBAvatarEmoteCommand } from '../../../../protocol/decentraland/sdk/components/avatar_emote_command.gen'
import { PBAvatarBase } from '../../../../protocol/decentraland/sdk/components/avatar_base.gen'
import { PBPlayerIdentityData } from '../../../../protocol/decentraland/sdk/components/player_identity_data.gen'
import { PBAvatarEquippedData } from '../../../../protocol/decentraland/sdk/components/avatar_equipped_data.gen'
import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations'

const MAX_ENTITY_VERSION = 0xffff
const AVATAR_RESERVED_ENTITY_NUMBER = { from: 10, to: 200 }

const ComponentIds = {
  TRANSFORM: 1,
  AVATAR_BASE: 1087,
  AVATAR_EMOTE_COMMAND: 1088,
  PLAYER_IDENTITY_DATA: 1089,
  AVATAR_EQUIPPED_DATA: 1091
}

export function createTinyEcs() {
  const avatarEntity = new Map<string, Entity>()
  const entities: Map<number, { version: number; live: boolean }> = new Map()
  const componentsTimestamp: Map<number, Map<number, { ts: number }>> = new Map()
  const crdtReusableBuffer = new ReadWriteByteBuffer()
  const transformReusableBuffer = new ReadWriteByteBuffer()

  function createNewEntity(): Entity {
    for (
      let entityNumber = AVATAR_RESERVED_ENTITY_NUMBER.from;
      entityNumber < AVATAR_RESERVED_ENTITY_NUMBER.to;
      entityNumber++
    ) {
      const currentEntity = entities.get(entityNumber)
      if (!currentEntity) {
        entities.set(entityNumber, { version: 0, live: true })
        return entityNumber as Entity
      } else if (!currentEntity.live && currentEntity.version < MAX_ENTITY_VERSION) {
        currentEntity.live = true
        currentEntity.version++
        return (((entityNumber & MAX_ENTITY_VERSION) | ((currentEntity.version & MAX_ENTITY_VERSION) << 16)) >>>
          0) as Entity
      }
    }

    throw new Error("Can't create more entities")
  }

  function ensureAvatarEntityId(userId: string) {
    const entity = avatarEntity.get(userId)
    if (entity) {
      return entity
    }

    const newEntity = createNewEntity()
    avatarEntity.set(userId, newEntity)
    return newEntity
  }

  function removeAvatarEntityId(userId: string): Uint8Array {
    const entity = avatarEntity.get(userId)
    if (entity) {
      const entityNumber = entity & MAX_ENTITY_VERSION
      const entityVersion = ((entity & 0xffff0000) >> 16) & MAX_ENTITY_VERSION

      if (entities.get(entityNumber)?.version === entityVersion) {
        entities.set(entityNumber, { version: entityVersion, live: false })
      }

      avatarEntity.delete(userId)
      for (const [_componentId, data] of componentsTimestamp) {
        data.delete(entity)
      }

      transformReusableBuffer.resetBuffer()
      DeleteEntity.write(entity, transformReusableBuffer)
      return transformReusableBuffer.toCopiedBinary()
    }
    return new Uint8Array()
  }

  function getComponentTimestamp(componentId: number) {
    const component = componentsTimestamp.get(componentId)
    if (component) {
      return component
    }

    componentsTimestamp.set(componentId, new Map())
    return componentsTimestamp.get(componentId)!
  }

  function updateAvatarEmoteCommand(entity: Entity, data): Uint8Array {
    const component = getComponentTimestamp(ComponentIds.AVATAR_EMOTE_COMMAND)

    // TODO: convert the data
    const componentValue = data
    const writer = PBAvatarEmoteCommand.encode(componentValue)
    const buffer = new Uint8Array(writer.finish(), 0, writer.len)

    const timestamp = (component.get(entity)?.ts || -1) + 1
    AppendValueOperation.write(entity, timestamp, ComponentIds.AVATAR_EMOTE_COMMAND, buffer, crdtReusableBuffer)

    // update timestamp
    component.set(entity, { ts: timestamp })

    return crdtReusableBuffer.toCopiedBinary()
  }

  function updateAvatarBase(entity: Entity, data: NewProfileForRenderer): Uint8Array {
    const component = getComponentTimestamp(ComponentIds.AVATAR_BASE)

    // TODO: convert the data
    const componentValue = data as any
    const writer = PBAvatarBase.encode(componentValue)
    const buffer = new Uint8Array(writer.finish(), 0, writer.len)

    const timestamp = (component.get(entity)?.ts || -1) + 1
    PutComponentOperation.write(entity, timestamp, ComponentIds.AVATAR_BASE, buffer, crdtReusableBuffer)

    // update timestamp
    component.set(entity, { ts: timestamp })

    return crdtReusableBuffer.toCopiedBinary()
  }

  function updatePlayerIdentityData(entity: Entity, data): Uint8Array {
    const component = getComponentTimestamp(ComponentIds.PLAYER_IDENTITY_DATA)

    // TODO: convert the data
    const componentValue = data
    const writer = PBPlayerIdentityData.encode(componentValue)
    const buffer = new Uint8Array(writer.finish(), 0, writer.len)

    const timestamp = (component.get(entity)?.ts || -1) + 1
    PutComponentOperation.write(entity, timestamp, ComponentIds.PLAYER_IDENTITY_DATA, buffer, crdtReusableBuffer)

    // update timestamp
    component.set(entity, { ts: timestamp })

    return crdtReusableBuffer.toCopiedBinary()
  }

  function updateAvatarEquippedData(entity: Entity, data): Uint8Array {
    const component = getComponentTimestamp(ComponentIds.AVATAR_EQUIPPED_DATA)

    // TODO: convert the data
    const componentValue = data
    const writer = PBAvatarEquippedData.encode(componentValue)
    const buffer = new Uint8Array(writer.finish(), 0, writer.len)

    const timestamp = (component.get(entity)?.ts || -1) + 1
    PutComponentOperation.write(entity, timestamp, ComponentIds.AVATAR_EQUIPPED_DATA, buffer, crdtReusableBuffer)

    // update timestamp
    component.set(entity, { ts: timestamp })

    return crdtReusableBuffer.toCopiedBinary()
  }

  function updateAvatarTransform(entity: Entity, data: rfc4.Position): { data: Uint8Array; ts: number } {
    const component = getComponentTimestamp(ComponentIds.TRANSFORM)
    transformReusableBuffer.resetBuffer()

    transformReusableBuffer.setFloat32(0, data.positionX)
    transformReusableBuffer.setFloat32(4, data.positionY)
    transformReusableBuffer.setFloat32(8, data.positionZ)

    // TODO: See convert EULER rotation to quaternion ?
    transformReusableBuffer.setFloat32(12, data.rotationX)
    transformReusableBuffer.setFloat32(16, data.rotationY)
    transformReusableBuffer.setFloat32(20, data.rotationZ)
    transformReusableBuffer.setFloat32(24, 1.0)

    transformReusableBuffer.setFloat32(28, 1.0)
    transformReusableBuffer.setFloat32(32, 1.0)
    transformReusableBuffer.setFloat32(36, 1.0)
    transformReusableBuffer.setUint32(40, 0)

    const timestamp = (component.get(entity)?.ts || -1) + 1
    PutComponentOperation.write(
      entity,
      timestamp,
      ComponentIds.TRANSFORM,
      transformReusableBuffer.toBinary(),
      crdtReusableBuffer
    )
    return { ts: timestamp, data: crdtReusableBuffer.toCopiedBinary() }
  }

  return {
    ensureAvatarEntityId,
    removeAvatarEntityId,

    updateAvatarTransform,
    updateAvatarBase,
    updateAvatarEquippedData,
    updateAvatarEmoteCommand,
    updatePlayerIdentityData
  }
}
