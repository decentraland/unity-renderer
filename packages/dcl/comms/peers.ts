import { AvatarEntity } from '../entities/utils/AvatarEntity'
import { getUserProfile, getBlockedUsers, getMutedUsers } from '../../shared/comms/profile'
import { Quaternion } from 'babylonjs'

export type UUID = string

/**
 * This type contains information about the peers, the AvatarEntity must accept this whole object in setAttributes(obj).
 */
export type PeerInformation = {
  /**
   * Unique peer ID
   */
  uuid: UUID

  flags: {
    muted?: boolean
  }

  user?: UserInformation

  avatar?: AvatarEntity
}

export type UserInformation = {
  /**
   * User's display name
   */
  displayName?: string
  publicKey?: string
  avatarType?: string
  status?: string
}

// The order is [X,Y,Z,Qx,Qy,Qz,Qw]
export type Pose = [number, number, number, number, number, number, number]

export type PoseInformation = {
  v: Pose
}

export const peerMap = new Map<UUID, PeerInformation>()

export let localProfileUUID: UUID | null = null

export function findPeerByName(displayName: string): UserInformation {
  const peers = Array.from(peerMap).map(([key, value]) => value)
  const peer = peers.find((peer: PeerInformation) => peer.user.displayName === displayName)

  if (!peer || !peer.user) {
    return null
  }

  return peer.user
}

/**
 * @param uuid the UUID used by the communication engine
 */
export function setLocalProfile(uuid: UUID, user: UserInformation = {}) {
  if (typeof (uuid as any) !== 'string') throw new Error('Did not receive a valid UUID')

  if (localProfileUUID) {
    removeById(localProfileUUID)
  }

  const profile = {
    uuid,
    user,
    flags: {}
  }

  peerMap.set(uuid, profile)

  localProfileUUID = uuid
  return profile
}

/**
 * Removes both the peer information and the Avatar from the world.
 * @param uuid
 */
export function removeById(uuid: UUID) {
  if (localProfileUUID === uuid) {
    localProfileUUID = null
  }

  if (!peerMap.has(uuid)) return

  const avatar = peerMap.get(uuid).avatar
  peerMap.delete(uuid)

  if (avatar) {
    avatar.dispose()
  }
}

/**
 * If not exist, sets up a new avatar and profile object
 * @param uuid
 */
export function setUpID(uuid: UUID) {
  if (!uuid) return null
  if (typeof (uuid as any) !== 'string') throw new Error('Did not receive a valid UUID')

  let peer: PeerInformation

  if (!peerMap.has(uuid)) {
    peer = {
      uuid,
      flags: {}
    }

    peerMap.set(uuid, peer)
  } else {
    peer = peerMap.get(uuid)
  }

  // we wont create an avatar for the local user
  if (!peer.avatar && localProfileUUID !== uuid) {
    peer.avatar = new AvatarEntity(uuid)

    peer.avatar.setAttributes({
      avatarType: 'square-robot',
      headRotation: { x: 0, y: 0, z: 0 }
    })

    blockPeer(peer, getBlockedUsers())
    mutePeer(peer, getMutedUsers())
  }

  return peer
}

/**
 * for every UUID, ensures that exist an avatar and a user in the local state.
 * Returns the AvatarEntity instance
 * @param uuid
 */
export function ensureAvatar(uuid: UUID): AvatarEntity | null {
  return setUpID(uuid).avatar || null
}

function blockPeer(peer: PeerInformation, blockedUsers: Set<string>) {
  const avatar = peer.avatar
  if (avatar && peer.user) {
    const visible = !blockedUsers.has(peer.user.publicKey)
    avatar.setAttributes({ visible, muted: !visible })
  }
}

function mutePeer(peer: PeerInformation, mutedUsers: Set<string>) {
  const avatar = peer.avatar
  if (avatar && peer.user) {
    const muted = mutedUsers.has(peer.user.publicKey)
    avatar.setAttributes({ muted })
  }
}

/**
 * Receives a list of muted users and blocks the users in that list. Also unblocks the users that are not in that list.
 */
export function hideBlockedUsers(blockedUsers: Set<string>): void {
  if (blockedUsers) {
    peerMap.forEach((peer, key) => {
      blockPeer(peer, blockedUsers)
    })
  }
}

/**
 * Receives a list of muted users and mutes the users in that list. Also unmutes the users that are not in that list.
 */
export function muteUsers(mutedUsers: Set<string>): void {
  if (mutedUsers) {
    peerMap.forEach((peer, key) => {
      mutePeer(peer, mutedUsers)
    })
  }
}

export function receiveUserData(uuid: string, profile: Partial<UserInformation>) {
  const avatar: AvatarEntity = ensureAvatar(uuid)

  const peerData = peerMap.get(uuid)
  const userData = peerData.user || (peerData.user = peerData.user || {})

  const profileChanged =
    (profile.displayName && userData.displayName !== profile.displayName) ||
    (profile.publicKey && userData.publicKey !== profile.publicKey) ||
    (profile.avatarType && userData.avatarType !== profile.avatarType)

  if (profileChanged) {
    // TODO: Sanitize this values
    Object.assign(userData, profile)

    if (avatar) {
      avatar.setAttributes({
        displayName: userData.displayName,
        publicKey: userData.publicKey,
        avatarType: userData.avatarType
      })
    }
  }
}

const tmpEuler = new BABYLON.Vector3()

export function receiveUserPose(uuid: string, pose: PoseInformation) {
  const avatar: AvatarEntity = ensureAvatar(uuid)

  if (!avatar) {
    return false
  }

  const [x, y, z, Qx, Qy, Qz, Qw] = pose.v

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

export function receiveHandPose(hand: string, uuid: string, pose: PoseInformation) {
  const avatar: AvatarEntity = ensureAvatar(uuid)
  if (!avatar) return

  handleHandChange(hand, pose, avatar)
}

function handleHandChange(hand: string, pose: PoseInformation, avatar: AvatarEntity) {
  if ('v' in pose) {
    const [x, y, z, Qx, Qy, Qz, Qw] = pose.v

    if (hand === 'leftHand') {
      avatar.setAttributes({
        leftHandPosition: { x, y, z },
        leftHandRotation: { x: Qx, y: Qy, z: Qz, w: Qw }
      })
    } else if (hand === 'rightHand') {
      avatar.setAttributes({
        rightHandPosition: { x, y, z },
        rightHandRotation: { x: Qx, y: Qy, z: Qz, w: Qw }
      })
    }
  }
}

/**
 * In some cases, like minimizing the window, the user will be invisible to the rest of the world.
 * This function handles those visible changes.
 */
export function receiveUserVisible(uuid: string, visible: boolean) {
  const avatar: AvatarEntity = ensureAvatar(uuid)
  if (avatar) {
    avatar.setAttributes({ visible })
  }
}

/**
 * This function is used to get the current user's information. The result is read-only.
 */
export function getCurrentPeer(): Readonly<PeerInformation> | null {
  if (!localProfileUUID) return null
  return peerMap.get(localProfileUUID) || null
}

/**
 * This function is used to get the current user's information. The result is read-only.
 */
export function getCurrentUser(): Readonly<UserInformation> | null {
  const user = getUserProfile()
  return user || null
}

/**
 * This function is used to get the current user's information. The result is read-only.
 */
export function getPeer(uuid: UUID): Readonly<PeerInformation> | null {
  if (!uuid) return null
  return peerMap.get(uuid) || null
}

/**
 * This function is used to get the current user's information. The result is read-only.
 */
export function getUser(uuid: UUID): Readonly<UserInformation> | null {
  const peer = getPeer(uuid)
  if (!peer) return null
  return peer.user || null
}
