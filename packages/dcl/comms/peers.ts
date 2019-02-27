import { AvatarEntity } from '../entities/utils/AvatarEntity'
import { Quaternion } from 'babylonjs'
import {
  UUID,
  ReceiveUserDataMessage,
  ReceiveUserPoseMessage,
  ReceiveUserVisibleMessage,
  UserRemovedMessage,
  AvatarMessageType,
  UserMessage
} from '../../shared/comms/types'
import { localProfileUUID, getBlockedUsers, getMutedUsers, avatarMessageObservable } from '../../shared/comms/peers'

const avatarMap = new Map<string, AvatarEntity>()

/**
 * for every UUID, ensures that exist an avatar and a user in the local state.
 * Returns the AvatarEntity instance
 * @param uuid
 */
function ensureAvatar(uuid: UUID): AvatarEntity | null {
  let avatar = avatarMap.get(uuid)

  if (avatar) {
    return avatar
  }

  // we wont create an avatar for the local user
  if (localProfileUUID !== uuid) {
    avatar = new AvatarEntity(uuid)

    avatar.setAttributes({
      avatarType: 'square-robot',
      headRotation: { x: 0, y: 0, z: 0 }
    })

    avatarMap.set(uuid, avatar)

    hideBlockedUsers()
  }

  return avatar
}

/**
 * Unblocks the users that are not in that list.
 */
function hideBlockedUsers(): void {
  const blockedUsers = getBlockedUsers()
  const mutedUsers = getMutedUsers()

  avatarMap.forEach((avatar, uuid) => {
    const visible = !blockedUsers.has(uuid)
    const muted = !visible || mutedUsers.has(uuid)
    avatar.setAttributes({ visible, muted })
  })
}

function handleUserData(message: ReceiveUserDataMessage) {
  const avatar: AvatarEntity = ensureAvatar(message.uuid)

  const userData = message.data

  if (avatar) {
    avatar.setAttributes({
      displayName: userData.displayName,
      publicKey: userData.publicKey,
      avatarType: userData.avatarType
    })
  }
}

const tmpEuler = new BABYLON.Vector3()

function handleUserPose({ uuid, pose }: ReceiveUserPoseMessage) {
  const avatar: AvatarEntity = ensureAvatar(uuid)

  if (!avatar) {
    return false
  }

  const [x, y, z, Qx, Qy, Qz, Qw] = pose

  avatar.position.set(x, y, z)
  if (!avatar.rotationQuaternion) {
    avatar.rotationQuaternion = Quaternion.Identity()
  }
  avatar.rotationQuaternion.set(Qx, Qy, Qz, Qw)
  avatar.rotationQuaternion.toEulerAnglesToRef(tmpEuler)

  tmpEuler.y = 0

  avatar.setAttributes({
    headRotation: tmpEuler
  })

  return true
}

/**
 * In some cases, like minimizing the window, the user will be invisible to the rest of the world.
 * This function handles those visible changes.
 */
function handleUserVisible({ uuid, visible }: ReceiveUserVisibleMessage) {
  const avatar: AvatarEntity = ensureAvatar(uuid)
  if (avatar) {
    avatar.setAttributes({ visible })
  }
}

function handleUserRemoved({ uuid }: UserRemovedMessage) {
  const avatar: AvatarEntity | null = avatarMap.get(uuid)
  if (avatar) {
    avatar.dispose()
  }
  avatarMap.delete(uuid)
}

function handleMutedBlockedMessages({ uuid }: UserMessage) {
  hideBlockedUsers()
}

avatarMessageObservable.add(evt => {
  if (evt.type === AvatarMessageType.USER_DATA) {
    handleUserData(evt)
  } else if (evt.type === AvatarMessageType.USER_POSE) {
    handleUserPose(evt)
  } else if (evt.type === AvatarMessageType.USER_VISIBLE) {
    handleUserVisible(evt)
  } else if (evt.type === AvatarMessageType.USER_REMOVED) {
    handleUserRemoved(evt)
  } else if (evt.type === AvatarMessageType.USER_MUTED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_BLOCKED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_UNMUTED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_UNBLOCKED) {
    handleMutedBlockedMessages(evt)
  }
})
