import mitt from 'mitt'
import { getProfileFromStore } from 'shared/profiles/selectors'
import { store } from 'shared/store/isolatedStore'
import { avatarMessageObservable } from '../../../../comms/peers'
import * as rfc4 from '../../../../protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { Entity } from '../engine/entity'
import { createTinyEcs } from './ecs'

export type AvatarSdk7Message = {
  ChangePosition: {
    parcel: { x: number; z: number }
    entity: Entity
    ts: number
    data: rfc4.Position
  }

  BinaryMessage: Uint8Array

  RemoveAvatar: {
    entity: Entity
    data: Uint8Array
  }
}

export const avatarSdk7Ecs = createTinyEcs()
export const avatarSdk7MessageObservable = mitt<AvatarSdk7Message>()

avatarMessageObservable.add((evt) => {
  const avatarEntityId = avatarSdk7Ecs.ensureAvatarEntityId(evt.userId)
  if (evt.type === 'USER_DATA') {
    if (evt.data.position) {
      const timestamp = avatarSdk7Ecs.computeNextAvatarTransformTimestamp(avatarEntityId)
      avatarSdk7MessageObservable.emit('ChangePosition', {
        parcel: {
          x: Math.floor(evt.data.position.positionX / 16.0),
          z: Math.floor(evt.data.position.positionZ / 16.0)
        },
        entity: avatarEntityId,
        data: evt.data.position,
        ts: timestamp
      })

      if (evt.profile) {
        const avatarBase = avatarSdk7Ecs.handleNewProfile(avatarEntityId, evt.profile, false)
        for (const msg of avatarBase) {
          avatarSdk7MessageObservable.emit('BinaryMessage', msg)
        }
      }
    } else {
      if (evt.profile) {
        const avatarBase = avatarSdk7Ecs.handleNewProfile(avatarEntityId, evt.profile, true)
        for (const msg of avatarBase) {
          avatarSdk7MessageObservable.emit('BinaryMessage', msg)
        }
      }
    }
  } else if (evt.type === 'USER_EXPRESSION') {
    const avatarEmoteCommand = avatarSdk7Ecs.appendAvatarEmoteCommand(avatarEntityId, evt)
    avatarSdk7MessageObservable.emit('BinaryMessage', avatarEmoteCommand)
  } else if (evt.type === 'USER_REMOVED') {
    const avatarRemoveEntity = avatarSdk7Ecs.removeAvatarEntityId(evt.userId)
    avatarSdk7MessageObservable.emit('RemoveAvatar', { entity: avatarEntityId, data: avatarRemoveEntity })
  }
})

export function* avatarSdk7ProfileChanged(userId: string) {
  const profile = getProfileFromStore(store.getState(), userId)

  if (!profile?.data) {
    return {}
  }

  const avatarEntityId = avatarSdk7Ecs.ensureAvatarEntityId(userId)
  avatarSdk7Ecs.updateProfile(avatarEntityId, profile)
}
