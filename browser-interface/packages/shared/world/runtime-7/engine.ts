import { Engine, IEngine, Transport } from '@dcl/ecs/dist-cjs'
import {
  MediaState,
  AudioEvent as defineAudioEvent,
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
import { getSceneWorkerBySceneID, getSceneWorkerBySceneNumber } from '../parcelSceneManager'
import { SceneWorker } from '../SceneWorker'

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

type State = {
  sceneId: string | number
  entityId: Entity
  state: MediaState
}

type AudioStreamChange = {
  changeState: State
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

function getScene(id: string | number): SceneWorker | undefined {
  if (typeof id === 'string') return getSceneWorkerBySceneID(id)
  if (typeof id === 'number') return getSceneWorkerBySceneNumber(id)
  return undefined
}

function getEngineFromScene(scene?: SceneWorker): IEngine | undefined {
  return scene?.rpcContext.internalEngine?.engine
}

export const localProfileChanged = mitt<LocalProfileChange>()
export const audioStreamEmitter = mitt<AudioStreamChange>()

// AudioStream updates
audioStreamEmitter.on('changeState', ({ entityId, sceneId, state }) => {
  const scene = getScene(sceneId)
  const engine = getEngineFromScene(scene)

  if (!engine) return

  const AudioEvent = defineAudioEvent(engine)
  AudioEvent.addValue(entityId, { state, timestamp: Date.now() })
})
// end of AudioStream updates

/**
 * We used this engine as an internal engine to add information to the worker.
 * It handles the Avatar information for each player
 */
export function createInternalEngine(id: string, parcels: string[], isGlobalScene: boolean): IInternalEngine {
  const AVATAR_RESERVED_ENTITIES = { from: 10, to: 200 }
  const userId = getCurrentUserId(store.getState())!

  // From 0 to 10 engine reserved entities.
  const entityContainer = createEntityContainer({ reservedStaticEntities: AVATAR_RESERVED_ENTITIES.from })
  const engine = Engine({ entityContainer, onChangeFunction: () => {} })
  const Transform = defineTransform(engine)
  const AvatarBase = defineAvatarBase(engine)
  const AvatarEquippedData = defineAvatarEquippedData(engine)
  const PlayerIdentityData = definePlayerIdentityData(engine)
  const AvatarEmoteCommand = defineAvatarEmoteCommand(engine)
  const avatarMap = new Map<string, Entity>()

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
    if (!deepEqual(oldAvatarBase, avatarBase)) {
      AvatarBase.createOrReplace(entity, avatarBase)
    }
    const oldAvatarData = AvatarEquippedData.getOrNull(entity)
    const avatarData = {
      emoteUrns: data.avatar.emotes.map(($) => $.urn),
      wearableUrns: data.avatar.wearables
    }
    // [AvatarEquippedData Component]
    if (!deepEqual(oldAvatarData, avatarData)) {
      AvatarEquippedData.createOrReplace(entity, avatarData)
    }
  }

  function isUserInScene(position: Pick<Position, 'positionX' | 'positionZ'>) {
    if (isGlobalScene) return true

    const parcel = encodeParcelPosition({
      x: Math.floor(position.positionX / 16.0),
      y: Math.floor(position.positionZ / 16.0)
    })

    return parcels.includes(parcel)
  }

  //// CURRENT USER UPDATES -> engine.PlayerEntity
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
  // End of LOCAL USER

  // PlayersConnected observers
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

  /**
   * We used this transport to send only kernel-side updates (profile, emotes, audio stream...) to the client instead of the full state
   * For example: every time there is an update on a profile, this would add those CRDT messages to internalMessages
   * and they'd be append to the crdtSendToRenderer call
   */
  const transport: Transport = {
    filter: (message) => !!message,
    send: async (message: Uint8Array) => {
      if (message.byteLength) internalMessages.push(message)
    }
  }
  engine.addTransport(transport)
  const internalMessages: Uint8Array[] = []

  // Add current user
  addUser(userId)

  // Add Players connected to comms that are in the scene
  for (const [userId, data] of getAllPeers()) {
    if (!data.position || !isUserInScene(data.position)) continue

    addUser(userId)
  }

  return {
    engine,
    update: async () => {
      await engine.update(1)
      const messages = [...internalMessages]
      internalMessages.length = 0
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
