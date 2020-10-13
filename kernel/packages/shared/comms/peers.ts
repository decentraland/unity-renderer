import { Observable } from 'decentraland-ecs/src'
import { UUID, PeerInformation, AvatarMessage, UserInformation, AvatarMessageType, Pose } from './interface/types'
import { getFromLocalStorage, removeFromLocalStorage } from 'atomicHelpers/localStorage'

export const getUserProfile = () => getFromLocalStorage('dcl-profile') || {}
export const removeUserProfile = () => removeFromLocalStorage('dcl-profile')

export const peerMap = new Map<UUID, PeerInformation>()
export const avatarMessageObservable = new Observable<AvatarMessage>()

export let localProfileUUID: UUID | null = null

export function findPeerByName(displayName: string): UserInformation | null {
  for (let [, peer] of peerMap) {
    if (peer.user && peer.user.profile && peer.user.profile.name === displayName) {
      return peer.user
    }
  }
  return null
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

  avatarMessageObservable.notifyObservers({
    type: AvatarMessageType.SET_LOCAL_UUID,
    uuid
  })

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

  if (peerMap.delete(uuid)) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_REMOVED,
      uuid
    })
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

/**
 * If not exist, sets up a new avatar and profile object
 * @param uuid
 */
export function setUpID(uuid: UUID): PeerInformation | null {
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
    peer = peerMap.get(uuid) as PeerInformation
  }

  return peer
}

export function receiveUserData(uuid: string, data: Partial<UserInformation>) {
  const peerData = setUpID(uuid)
  if (peerData) {
    const userData = peerData.user || (peerData.user = peerData.user || {})

    const profileChanged = (data.version && userData.version !== data.version) || (!userData.profile && data.profile)

    if (profileChanged) {
      Object.assign(userData, data)

      avatarMessageObservable.notifyObservers({
        type: AvatarMessageType.USER_DATA,
        uuid,
        data
      })
    }
  }
}

export function receiveUserTalking(uuid: string, talking: boolean) {
  avatarMessageObservable.notifyObservers({
    type: AvatarMessageType.USER_TALKING,
    uuid,
    talking
  })
}

export function receiveUserPose(uuid: string, pose: Pose) {
  avatarMessageObservable.notifyObservers({
    type: AvatarMessageType.USER_POSE,
    uuid,
    pose
  })
}

/**
 * In some cases, like minimizing the window, the user will be invisible to the rest of the world.
 * This function handles those visible changes.
 */
export function receiveUserVisible(uuid: string, visible: boolean) {
  avatarMessageObservable.notifyObservers({
    type: AvatarMessageType.USER_VISIBLE,
    uuid,
    visible
  })
}
