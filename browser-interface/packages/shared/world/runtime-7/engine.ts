import { Engine, IEngine, Transport } from '@dcl/ecs/dist-cjs'
import {
  Transform as defineTransform,
  PlayerIdentityData as definePlayerIdentityData,
  AvatarBase as defineAvatarBase,
  AvatarEquippedData as defineAvatarEquippedData,
  AvatarEmoteCommand as defineAvatarEmoteCommand
} from '@dcl/ecs/dist-cjs/components'
import { Entity, EntityUtils, createEntityContainer } from '@dcl/ecs/dist-cjs/engine/entity'
import { avatarMessageObservable, getAllPeers } from '../../comms/peers'
import { encodeParcelPosition } from '../../../lib/decentraland'
import { AvatarMessageType } from '../../comms/interface/types'
import { Position } from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { store } from '../../store/isolatedStore'
import { getCurrentUserId } from '../../session/selectors'
import { getProfileFromStore } from '../../profiles/selectors'
import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations'
import mitt from 'mitt'
import { Avatar } from '@dcl/schemas'
import { prepareAvatar } from '../../../lib/decentraland/profiles/transformations/profileToRendererFormat'
import { deepEqual } from '../../../lib/javascript/deepEqual'
import { positionObservable } from '../positionThings'

export type IInternalEngine = {
  engine: IEngine
  update: () => Promise<Uint8Array[]>
  destroy: () => void
}

type EmoteData = {
  emoteUrn: string
  timestamp: number
}

type LocalProfileChange = {
  changeAvatar: Avatar
  triggerEmote: EmoteData
}

export const localProfileChanged = mitt<LocalProfileChange>()

/**
 * We used this engine as an internal engine to add information to the worker.
 * It handles the Avatar information for each player
 */
export function createInternalEngine(id: string, parcels: string[], isGlobalScene: boolean): IInternalEngine {
  // LOCAL USERID -> engine.PlayerEntity
  const userId = getCurrentUserId(store.getState())!
  const userIdPositionObserver = positionObservable.add((data) => {
    const isInsideScene = isUserInScene({ positionX: data.position.x, positionZ: data.position.z })
    if (isInsideScene && !AvatarBase.has(engine.PlayerEntity)) {
      const profile = getUserData(userId)
      if (profile) {
        updateUser(userId, profile)
      }
    } else if (!isInsideScene && AvatarBase.has(engine.PlayerEntity)) {
      removeUser(userId)
    }
  })
  localProfileChanged.on('changeAvatar', (data) => {
    const avatar = prepareAvatar(data.avatar)
    updateUser(data.userId, { name: data.name, avatar })
  })
  localProfileChanged.on('triggerEmote', (emote) => {
    addEmote(userId, emote)
  })

  // End of LOCAL USER ID handlers

  const observerInstance = avatarMessageObservable.add((message) => {
    if (message.type === AvatarMessageType.USER_EXPRESSION) {
      addEmote(message.userId, { emoteUrn: message.expressionId, timestamp: message.timestamp })
    }

    if (message.type === AvatarMessageType.USER_DATA) {
      if (message.data.position && isUserInScene(message.data.position)) {
        updateUser(message.userId, message.profile, message.data.position)
      } else if (avatarMap.has(message.userId)) {
        removeUser(message.userId)
      }
    }

    if (message.type === AvatarMessageType.USER_REMOVED) {
      removeUser(message.userId)
    }

    if (message.type === AvatarMessageType.USER_VISIBLE) {
      if (!message.visible) {
        removeUser(message.userId)
      } else {
        addUser(message.userId)
      }
    }
  })

  const AVATAR_RESERVED_ENTITIES = { from: 10, to: 200 }
  // From 0 to 10 engine reserved entities.
  const entityContainer = createEntityContainer({ reservedStaticEntities: AVATAR_RESERVED_ENTITIES.from })
  const engine = Engine({ entityContainer, onChangeFunction: () => {} })
  const Transform = defineTransform(engine)
  const AvatarBase = defineAvatarBase(engine)
  const AvatarEquippedData = defineAvatarEquippedData(engine)
  const PlayerIdentityData = definePlayerIdentityData(engine)
  const AvatarEmoteCommand = defineAvatarEmoteCommand(engine)
  const avatarMap = new Map<string, Entity>()

  function addEmote(userId: string, data: EmoteData) {
    const entity = avatarMap.get(userId)
    if (!entity) return
    AvatarEmoteCommand.addValue(entity, { emoteUrn: data.emoteUrn, timestamp: data.timestamp, loop: false })
  }

  function updateUser(
    userId: string,
    data: { avatar: NewProfileForRenderer['avatar']; name: string },
    avatarPosition?: Position
  ) {
    const isIdentity = userId === getCurrentUserId(store.getState())
    const entity = avatarMap.get(userId)

    if (!entity) {
      console.error(`[BOEDO] user not found !`, { userId })
      return addUser(userId)
    }

    if (avatarPosition) {
      // [Transform Component] The renderer handles the PlayerEntity transform
      if (!isIdentity) {
        const transform = {
          position: { x: avatarPosition.positionX, y: avatarPosition.positionY, z: avatarPosition.positionZ },
          rotation: {
            x: avatarPosition.rotationX,
            y: avatarPosition.rotationY,
            z: avatarPosition.rotationZ,
            w: avatarPosition.rotationW
          }
        }
        const oldTransform = Transform.getOrNull(entity)
        if (!deepEqual(oldTransform?.position, transform.position)) {
          Transform.createOrReplace(entity, transform)
        }
      }
    }
    const oldAvatarBase = AvatarBase.getOrNull(entity)
    const avatarBase = {
      skinColor: data.avatar.skinColor,
      eyesColor: data.avatar.eyeColor,
      hairColor: data.avatar.hairColor,
      bodyShapeUrn: data.avatar.bodyShape,
      name: data.name
    }

    // [PBAvatarBase Component]
    if (deepEqual(oldAvatarBase, avatarBase)) {
    } else {
      AvatarBase.createOrReplace(entity, avatarBase)
    }
    const oldAvatarData = AvatarEquippedData.getOrNull(entity)
    const avatarData = {
      emotesUrns: data.avatar.emotes.map(($) => $.urn),
      wearableUrns: data.avatar.wearables
    }
    // [AvatarEquippedData Component]
    if (deepEqual(oldAvatarData, avatarData)) {
    } else {
      AvatarEquippedData.createOrReplace(entity, avatarData)
    }
  }

  function getUserData(userId: string) {
    const dataFromStore = getProfileFromStore(store.getState(), userId)
    if (!dataFromStore) return undefined
    return {
      avatar: prepareAvatar(dataFromStore.data.avatar),
      name: dataFromStore.data.name,
      isGuest: !dataFromStore.data.hasConnectedWeb3
    }
  }

  function addUser(userId: string) {
    const isIdentity = getCurrentUserId(store.getState()) === userId
    const profile = getUserData(userId)
    if (avatarMap.get(userId) || !profile) {
      return
    }
    const entity = isIdentity ? engine.PlayerEntity : engine.addEntity()
    const [entityId] = EntityUtils.fromEntityId(entity)

    if (entityId >= AVATAR_RESERVED_ENTITIES.to) {
      engine.removeEntity(entity)
      console.error(`[BOEDO] Max amount of users reached`, entityId)
      return
    }
    PlayerIdentityData.create(entity, { address: userId, isGuest: profile.isGuest })
    avatarMap.set(userId, entity)
    updateUser(userId, { avatar: profile.avatar, name: profile.name })
  }

  function removeUser(userId: string) {
    const entity = avatarMap.get(userId)
    if (!entity) return

    const isIdentity = getCurrentUserId(store.getState()) === userId
    if (isIdentity) {
      AvatarBase.deleteFrom(engine.PlayerEntity)
      AvatarEquippedData.deleteFrom(engine.PlayerEntity)
    } else {
      avatarMap.delete(userId)
      engine.removeEntity(entity)
    }
  }

  for (const [userId, data] of getAllPeers()) {
    if (!data.position || !isUserInScene(data.position)) continue

    addUser(userId)
  }

  function isUserInScene(position: Pick<Position, 'positionX' | 'positionZ'>) {
    if (isGlobalScene) return true

    const parcel = encodeParcelPosition({
      x: Math.floor(position.positionX / 16.0),
      y: Math.floor(position.positionZ / 16.0)
    })

    return parcels.includes(parcel)
  }

  /**
   * We used this transport to send only the avatar updates to the client instead of the full state
   * Every time there is an update on a profile, this would add those CRDT messages to avatarMessages
   * and they'd be append to the crdtSendToRenderer call
   */
  const transport: Transport = {
    filter: (message) => !!message,
    send: async (message: Uint8Array) => {
      if (message.byteLength) avatarMessages.push(message)
    }
  }
  engine.addTransport(transport)
  const avatarMessages: Uint8Array[] = []

  // Add local user
  addUser(userId)

  return {
    engine,
    update: async () => {
      await engine.update(1)
      const messages = [...avatarMessages]
      avatarMessages.length = 0
      return messages
    },
    destroy: () => {
      avatarMessageObservable.remove(observerInstance)
      positionObservable.remove(userIdPositionObserver)
      localProfileChanged.off('triggerEmote')
      localProfileChanged.off('changeAvatar')
    }
  }
}
