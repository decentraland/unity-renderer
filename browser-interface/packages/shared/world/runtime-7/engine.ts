import { Engine, IEngine, Transport } from '@dcl/ecs/dist-cjs'
import {
  Transform as defineTransform,
  PlayerIdentityData as definePlayerIdentityData
} from '@dcl/ecs/dist-cjs/components'
import { Entity, EntityContainer, EntityUtils } from '@dcl/ecs/dist-cjs/engine/entity'
import { avatarMessageObservable, getAllPeers, getPeer, getPeerData } from '../../comms/peers'
import { encodeParcelPosition } from '../../../lib/decentraland'
import { AvatarMessageType } from '../../comms/interface/types'
import { Position } from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { NewProfileForRenderer } from '../../../lib/decentraland/profiles/transformations'

export type IInternalEngine = {
  engine: IEngine
  update: () => Promise<Uint8Array[]>
}

/**
 * We used this engine as an internal engine to add information to the worker.
 * It handles the Avatar information for each player
 */
export function createInternalEngine(id: string, parcels: string[]): IInternalEngine {
  avatarMessageObservable.add((message) => {
    console.log(`[BOEDO ${id}] avatarMessageObservable`, message)
    if (message.type === AvatarMessageType.USER_DATA) {
      updateUser(message.userId, message.data.position, message.profile)
    }
    if (message.type === AvatarMessageType.USER_REMOVED) {
      removeUser(message.userId)
    }

    if (message.type === AvatarMessageType.USER_VISIBLE) {
      if (!message.visible) {
        removeUser(message.userId)
      }
    }
    // TODO: user visible ?
  })

  const AVATAR_RESERVED_ENTITIES = { from: 10, to: 200 }
  // From 0 to 10 engine reserved entities.
  const entityContainer = EntityContainer({ reservedStaticEntities: AVATAR_RESERVED_ENTITIES.from })
  const engine = Engine({ entityContainer, onChangeFunction: () => {} })
  const Transform = defineTransform(engine)
  const PlayerIdentityData = definePlayerIdentityData(engine)
  const avatarMap = new Map<string, Entity>()

  function updateUser(userId: string, avatarPosition: Position | undefined, _profile: NewProfileForRenderer) {
    const entity = avatarMap.get(userId)
    if (!entity) {
      console.error(`[BOEDO] user not found !`, { userId })
      return addUser(userId)
    }

    if (avatarPosition) {
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
      if (JSON.stringify(oldTransform?.position) === JSON.stringify(transform.position)) {
        console.log(`[BOEDO ${id}] same position, ignoring`)
        return
      }
      Transform.createOrReplace(entity, transform)
    }
  }

  function addUser(userId: string) {
    console.log(`[BOEDO ${id}] addUser`, { userId })
    const data = getPeerData(userId)
    const peer = getPeer(userId)
    if (!data || !peer) {
      console.error(`[BOEDO] peer info not found`, { userId })
      return
    }
    if (avatarMap.get(userId)) {
      return updateUser(userId, peer.position, data)
    }
    const entity = engine.addEntity()
    const [entityId] = EntityUtils.fromEntityId(entity)
    if (entityId >= AVATAR_RESERVED_ENTITIES.to) {
      engine.removeEntity(entity)
      console.error(`[BOEDO] Max amount of users reached`, entityId)
      return
    }
    console.log(`[BOEDO ${id}] PlayerIdentityData`, { userId, entity })
    PlayerIdentityData.create(entity, { address: data.userId, isGuest: data.hasConnectedWeb3 })
    avatarMap.set(userId, entity)
  }

  function removeUser(userId: string) {
    const entity = avatarMap.get(userId)
    console.log(`[BOEDO ${id}] removeUser`, { userId })

    if (!entity) return

    avatarMap.delete(userId)
    engine.removeEntity(entity)
  }

  for (const [userId, data] of getAllPeers()) {
    if (!data.position) continue
    const parcel = encodeParcelPosition({
      x: Math.floor(data.position.positionX / 16.0),
      y: Math.floor(data.position.positionZ / 16.0)
    })
    if (parcels.includes(parcel)) {
      console.log(`[BOEDO ${id}] adding user`, { userId })
      addUser(userId)
    }
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

  return {
    engine,
    update: async () => {
      await engine.update(1)
      const messages = [...avatarMessages]
      avatarMessages.length = 0
      return messages
    }
  }
}
