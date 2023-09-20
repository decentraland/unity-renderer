import { avatarMessageObservable } from '../../../../comms/peers'
import mitt from 'mitt'
import { createTinyEcs } from './ecs'
import { Entity } from '../engine/entity'

export type AvatarSdk7Message = {
  ChangePosition: {
    parcel: { x: number; z: number }
    entity: Entity
    ts: number
    data: Uint8Array
  }

  BinaryMessage: Uint8Array

  RemoveAvatar: {
    entity: Entity
    data: Uint8Array
  }
}

const avatarEcs = createTinyEcs()
export const avatarSdk7MessageObservable = mitt<AvatarSdk7Message>()

avatarMessageObservable.add((evt) => {
  const avatarEntityId = avatarEcs.ensureAvatarEntityId(evt.userId)
  if (evt.type === 'USER_DATA') {
    if (evt.data.position) {
      const message = avatarEcs.updateAvatarTransform(avatarEntityId, evt.data.position)
      avatarSdk7MessageObservable.emit('ChangePosition', {
        parcel: { x: Math.floor(evt.data.position.positionX), z: Math.floor(evt.data.position.positionZ) },
        entity: avatarEntityId,
        data: message.data,
        ts: message.ts
      })
    }

    // if (evt.profile) {
    //   const avatarBase = avatarEcs.updateAvatarBase(avatarEntityId, evt.data)
    //   avatarSdk7MessageObservable.emit('BinaryMessage', avatarBase)

    //   if (evt.profile.avatar) {
    //     const avatarEquippedData = avatarEcs.updateAvatarEquippedData(avatarEntityId, evt.data)
    //     avatarSdk7MessageObservable.emit('BinaryMessage', avatarEquippedData)
    //   }
    // }
  } else if (evt.type === 'USER_EXPRESSION') {
    const avatarEmoteCommand = avatarEcs.updateAvatarEmoteCommand(avatarEntityId, evt)
    avatarSdk7MessageObservable.emit('BinaryMessage', avatarEmoteCommand)
  } else if (evt.type === 'USER_REMOVED') {
    const avatarRemoveEntity = avatarEcs.removeAvatarEntityId(evt.userId)
    avatarSdk7MessageObservable.emit('RemoveAvatar', { entity: avatarEntityId, data: avatarRemoveEntity })
  }
})
