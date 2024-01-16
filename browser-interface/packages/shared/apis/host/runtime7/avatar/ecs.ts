import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations'
import { ReceiveUserExpressionMessage } from 'shared/comms/interface/types'
import { ProfileUserInfo } from 'shared/profiles/types'
import { PBAvatarBase } from '../../../../protocol/decentraland/sdk/components/avatar_base.gen'
import { PBAvatarEmoteCommand } from '../../../../protocol/decentraland/sdk/components/avatar_emote_command.gen'
import { PBAvatarEquippedData } from '../../../../protocol/decentraland/sdk/components/avatar_equipped_data.gen'
import { PBPlayerIdentityData } from '../../../../protocol/decentraland/sdk/components/player_identity_data.gen'
import { Entity } from '../engine/entity'
import { ReadWriteByteBuffer } from '../serialization/ByteBuffer'
import { AppendValueOperation } from '../serialization/crdt/appendValue'
import { DeleteEntity } from '../serialization/crdt/deleteEntity'
import { PutComponentOperation } from '../serialization/crdt/putComponent'

export { PBPointerEventsResult } from '../../../../protocol/decentraland/sdk/components/pointer_events_result.gen'

const MAX_ENTITY_VERSION = 0xffff
const AVATAR_RESERVED_ENTITY_NUMBER = { from: 10, to: 200 }

export const Sdk7ComponentIds = {
  TRANSFORM: 1,
  AVATAR_BASE: 1087,
  AVATAR_EMOTE_COMMAND: 1088,
  PLAYER_IDENTITY_DATA: 1089,
  AVATAR_EQUIPPED_DATA: 1091,
  POINTER_EVENTS_RESULT: 1063
}

export function createTinyEcs() {
  const avatarEntity = new Map<string, Entity>()
  const entities: Map<number, { version: number; live: boolean }> = new Map()
  const componentsTimestamp: Map<number, Map<number, { lastMessageData?: Uint8Array; ts: number }>> = new Map()
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

  function appendAvatarEmoteCommand(entity: Entity, data: ReceiveUserExpressionMessage): Uint8Array {
    const avatarEmoteCommandComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_EMOTE_COMMAND)

    const writer = PBAvatarEmoteCommand.encode({
      emoteCommand: {
        emoteUrn: data.expressionId,
        loop: false // TODO: how to know if is loopable
      }
    })

    const buffer = new Uint8Array(writer.finish(), 0, writer.len)

    const timestamp = (avatarEmoteCommandComponent.get(entity)?.ts || 0) + 1
    AppendValueOperation.write(entity, timestamp, Sdk7ComponentIds.AVATAR_EMOTE_COMMAND, buffer, crdtReusableBuffer)

    avatarEmoteCommandComponent.set(entity, { ts: timestamp })

    return crdtReusableBuffer.toCopiedBinary()
  }

  function handleNewProfile(entity: Entity, data: NewProfileForRenderer, bypassEarlyExit: boolean): Uint8Array[] {
    const msgs: Uint8Array[] = []
    const playerIdentityComponent = getComponentTimestamp(Sdk7ComponentIds.PLAYER_IDENTITY_DATA)

    if (playerIdentityComponent.get(entity) === undefined) {
      crdtReusableBuffer.resetBuffer()

      const writer = PBPlayerIdentityData.encode({
        address: data.userId,
        isGuest: data.hasConnectedWeb3
      })
      const buffer = new Uint8Array(writer.finish(), 0, writer.len)
      const timestamp = (playerIdentityComponent.get(entity)?.ts || 0) + 1
      PutComponentOperation.write(entity, timestamp, Sdk7ComponentIds.PLAYER_IDENTITY_DATA, buffer, crdtReusableBuffer)

      const messageData = crdtReusableBuffer.toCopiedBinary()
      playerIdentityComponent.set(entity, { ts: timestamp, lastMessageData: messageData })
      msgs.push(messageData)
    } else if (!bypassEarlyExit) {
      return []
    }

    const avatarBaseComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_BASE)
    const avatarEquippedComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_EQUIPPED_DATA)

    {
      crdtReusableBuffer.resetBuffer()

      const writer = PBAvatarBase.encode({
        skinColor: data.avatar.skinColor,
        eyesColor: data.avatar.eyeColor,
        hairColor: data.avatar.hairColor,
        bodyShapeUrn: data.avatar.bodyShape,
        name: data.name
      })
      const buffer = new Uint8Array(writer.finish(), 0, writer.len)
      const timestamp = (avatarBaseComponent.get(entity)?.ts || 0) + 1
      PutComponentOperation.write(entity, timestamp, Sdk7ComponentIds.AVATAR_BASE, buffer, crdtReusableBuffer)

      const messageData = crdtReusableBuffer.toCopiedBinary()
      avatarBaseComponent.set(entity, { ts: timestamp, lastMessageData: messageData })
      msgs.push(messageData)
    }

    {
      crdtReusableBuffer.resetBuffer()

      const writer = PBAvatarEquippedData.encode({
        wearableUrns: data.avatar.wearables,
        emotesUrns: (data.avatar.emotes || []).map(($) => $.urn)
      })
      const buffer = new Uint8Array(writer.finish(), 0, writer.len)
      const timestamp = (avatarEquippedComponent.get(entity)?.ts || 0) + 1
      PutComponentOperation.write(entity, timestamp, Sdk7ComponentIds.AVATAR_EQUIPPED_DATA, buffer, crdtReusableBuffer)

      const messageData = crdtReusableBuffer.toCopiedBinary()
      avatarEquippedComponent.set(entity, { ts: timestamp, lastMessageData: messageData })
      msgs.push(messageData)
    }
    return msgs
  }

  function updateProfile(entity: Entity, profile: ProfileUserInfo): Uint8Array[] {
    const msgs: Uint8Array[] = []
    const avatarBaseComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_BASE)
    const avatarEquippedComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_EQUIPPED_DATA)

    {
      crdtReusableBuffer.resetBuffer()

      const writer = PBAvatarBase.encode({
        skinColor: profile.data.avatar.skin.color,
        eyesColor: profile.data.avatar.eyes.color,
        hairColor: profile.data.avatar.hair.color,
        bodyShapeUrn: profile.data.avatar.bodyShape,
        name: profile.data.name
      })
      const buffer = new Uint8Array(writer.finish(), 0, writer.len)
      const timestamp = (avatarBaseComponent.get(entity)?.ts || 0) + 1
      PutComponentOperation.write(entity, timestamp, Sdk7ComponentIds.AVATAR_BASE, buffer, crdtReusableBuffer)

      const messageData = crdtReusableBuffer.toCopiedBinary()
      avatarBaseComponent.set(entity, { ts: timestamp, lastMessageData: messageData })
      msgs.push(messageData)
    }

    {
      crdtReusableBuffer.resetBuffer()

      const writer = PBAvatarEquippedData.encode({
        wearableUrns: profile.data.avatar.wearables,
        emotesUrns: (profile.data.avatar.emotes || []).map(($) => $.urn)
      })
      const buffer = new Uint8Array(writer.finish(), 0, writer.len)
      const timestamp = (avatarEquippedComponent.get(entity)?.ts || 0) + 1
      PutComponentOperation.write(entity, timestamp, Sdk7ComponentIds.AVATAR_EQUIPPED_DATA, buffer, crdtReusableBuffer)

      const messageData = crdtReusableBuffer.toCopiedBinary()
      avatarEquippedComponent.set(entity, { ts: timestamp, lastMessageData: messageData })
      msgs.push(messageData)
    }
    return msgs
  }
  function computeNextAvatarTransformTimestamp(entity: Entity) {
    const transformComponent = getComponentTimestamp(Sdk7ComponentIds.TRANSFORM)
    const timestamp = (transformComponent.get(entity)?.ts || 0) + 1
    transformComponent.set(entity, { ts: timestamp })
    return timestamp
  }

  function getState(): Uint8Array[] {
    const playerIdentityComponent = getComponentTimestamp(Sdk7ComponentIds.PLAYER_IDENTITY_DATA)
    const avatarBaseComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_BASE)
    const avatarEquippedComponent = getComponentTimestamp(Sdk7ComponentIds.AVATAR_EQUIPPED_DATA)

    const msgs: Uint8Array[] = []
    for (
      let entityNumber = AVATAR_RESERVED_ENTITY_NUMBER.from;
      entityNumber < AVATAR_RESERVED_ENTITY_NUMBER.to;
      entityNumber++
    ) {
      const currentEntity = entities.get(entityNumber)
      if (currentEntity && currentEntity.live) {
        const entityId = (((entityNumber & MAX_ENTITY_VERSION) |
          ((currentEntity.version & MAX_ENTITY_VERSION) << 16)) >>>
          0) as Entity

        const playerIdentityData = playerIdentityComponent.get(entityId)
        const avatarBaseData = avatarBaseComponent.get(entityId)
        const avatarEquippedData = avatarEquippedComponent.get(entityId)

        if (playerIdentityData?.lastMessageData) {
          msgs.push(playerIdentityData.lastMessageData)
        }

        if (avatarBaseData?.lastMessageData) {
          msgs.push(avatarBaseData.lastMessageData)
        }

        if (avatarEquippedData?.lastMessageData) {
          msgs.push(avatarEquippedData.lastMessageData)
        }
      }
    }

    return msgs
  }

  return {
    ensureAvatarEntityId,
    removeAvatarEntityId,

    computeNextAvatarTransformTimestamp,
    appendAvatarEmoteCommand,
    updateProfile,
    handleNewProfile,

    getState
  }
}
