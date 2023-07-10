import type * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import type { Avatar } from '@dcl/schemas'
import { EthAddress } from '@dcl/schemas'
import { commConfigurations } from 'config'
import { Observable } from 'mz-observable'
import { getProfileFromStore } from 'shared/profiles/selectors'
import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { store } from 'shared/store/isolatedStore'
import { lastPlayerPositionReport } from 'shared/world/positionThings'
import { MORDOR_POSITION_RFC4 } from './const'
import type { AvatarMessage, PeerInformation } from './interface/types'
import { AvatarMessageType } from './interface/types'
import {
  CommunicationArea,
  position2parcelRfc4,
  positionReportToCommsPositionRfc4,
  squareDistanceRfc4
} from './interface/utils'

/**
 * peerInformationMap contains data received of the current peers that we have
 * information of.
 */
const peerInformationMap = new Map<string, PeerInformation>()
export const avatarMessageObservable = new Observable<AvatarMessage>()
export const avatarVersionUpdateObservable = new Observable<{ userId: string; version: number }>()

export function getAllPeers() {
  return new Map(peerInformationMap)
}
export function getVisiblePeerEthereumAddresses(): Array<{ userId: string }> {
  const result: Array<{ userId: string }> = []
  for (const peer of peerInformationMap.values()) {
    if (peer.visible) {
      result.push({ userId: peer.ethereumAddress })
    }
  }
  return result
}
export function getConnectedPeerCount() {
  return peerInformationMap.size
}

;(globalThis as any).peerMap = peerInformationMap

/**
 * Removes both the peer information and the Avatar from the world.
 * @param address
 */
export function removePeerByAddress(address: string): boolean {
  const peer = peerInformationMap.get(address.toLowerCase())
  if (peer) {
    peerInformationMap.delete(address.toLowerCase())
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_REMOVED,
      userId: peer.ethereumAddress
    })
    return true
  }
  return false
}

/**
 * This function is used to get the current user's information. The result is read-only.
 */
export function getPeer(address: string): Readonly<PeerInformation> | null {
  if (!address) return null
  return peerInformationMap.get(address.toLowerCase()) || null
}

/**
 * If not exist, sets up a new avatar and profile object
 * @param address
 */
export function setupPeer(address: string): PeerInformation {
  if (!address) throw new Error('Did not receive a valid Address')
  if (typeof (address as any) !== 'string') throw new Error('Did not receive a valid Address')
  if (!EthAddress.validate(address)) throw new Error('Did not receive a valid Address')

  const ethereumAddress = address.toLowerCase()

  if (!peerInformationMap.has(ethereumAddress)) {
    const peer: PeerInformation = {
      ethereumAddress,
      lastPositionIndex: 0,
      lastProfileVersion: -1,
      lastUpdate: Date.now(),
      talking: false,
      visible: true
    }

    peerInformationMap.set(ethereumAddress, peer)

    // if we have user data, then send it to the avatar-scene
    sendPeerUserData(address)

    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_VISIBLE,
      userId: ethereumAddress,
      visible: true
    })

    return peer
  } else {
    return peerInformationMap.get(ethereumAddress)!
  }
}

export function receivePeerUserData(avatar: Avatar, baseUrl: string) {
  const ethereumAddress = avatar.userId.toLowerCase()
  const peer = peerInformationMap.get(ethereumAddress)
  if (peer) {
    peer.baseUrl = baseUrl
    sendPeerUserData(ethereumAddress)
  }
}

function sendPeerUserData(address: string) {
  const peer = getPeer(address)
  if (peer && peer.baseUrl) {
    const profile = avatarUiProfileForUserId(peer.ethereumAddress, peer.baseUrl)
    if (profile) {
      avatarMessageObservable.notifyObservers({
        type: AvatarMessageType.USER_DATA,
        userId: peer.ethereumAddress,
        data: peer,
        profile
      })
    }
  }
}

export function receiveUserTalking(address: string, talking: boolean) {
  const peer = setupPeer(address)
  peer.talking = talking
  peer.lastUpdate = Date.now()
  avatarMessageObservable.notifyObservers({
    type: AvatarMessageType.USER_TALKING,
    userId: peer.ethereumAddress,
    talking
  })
}

export function receiveUserPosition(address: string, position: rfc4.Position) {
  if (
    position.positionX === MORDOR_POSITION_RFC4.positionX &&
    position.positionY === MORDOR_POSITION_RFC4.positionY &&
    position.positionZ === MORDOR_POSITION_RFC4.positionZ
  ) {
    removePeerByAddress(address)
    return
  }

  const peer = setupPeer(address)
  peer.lastUpdate = Date.now()

  if (position.index > peer.lastPositionIndex) {
    peer.position = position
    peer.lastPositionIndex = position.index

    sendPeerUserData(address)
  }
  return peer
}

function avatarUiProfileForUserId(address: string, contentServerBaseUrl: string) {
  const avatar = getProfileFromStore(store.getState(), address)
  if (avatar && avatar.data) {
    return profileToRendererFormat(avatar.data, {
      address: address,
      baseUrl: contentServerBaseUrl
    })
  }
  return null
}

/**
 * In some cases, like minimizing the window, the user will be invisible to the rest of the world.
 * This function handles those visible changes.
 */
export function receiveUserVisible(address: string, visible: boolean) {
  const peer = setupPeer(address)
  const didChange = peer.visible !== visible
  peer.visible = visible
  if (didChange) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_VISIBLE,
      userId: peer.ethereumAddress,
      visible
    })
    if (visible) {
      // often changes in visibility may delete the avatar remotely.
      // we send all the USER_DATA to make sure the scene always have
      // the required information to render the whole avatar
      sendPeerUserData(address)
    }
  }
}

export function removeAllPeers() {
  for (const alias of peerInformationMap.keys()) {
    removePeerByAddress(alias)
  }
}

/**
 * Ensures that there is only one peer tracking info for this identity.
 * Returns true if this is the latest update and the one that remains.
 *
 * TODO(Mendez 24/04/2022): wtf does this function do?
 */
export function ensureTrackingUniqueAndLatest(peer: PeerInformation) {
  let currentPeer = peer

  peerInformationMap.forEach((info, address) => {
    if (info.ethereumAddress === currentPeer.ethereumAddress && address !== peer.ethereumAddress) {
      if (info.lastProfileVersion < currentPeer.lastProfileVersion) {
        removePeerByAddress(address)
      } else if (info.lastProfileVersion > currentPeer.lastProfileVersion) {
        removePeerByAddress(currentPeer.ethereumAddress)

        info.position = info.position || currentPeer.position
        info.visible = info.visible || currentPeer.visible

        currentPeer = info
      }
    }
  })

  return currentPeer
}

export function processAvatarVisibility(maxVisiblePeers: number, myAddress: string | undefined) {
  if (!lastPlayerPositionReport) return
  const pos = positionReportToCommsPositionRfc4(lastPlayerPositionReport)
  const now = Date.now()

  type ProcessingPeerInfo = {
    alias: string
    squareDistance: number
    visible: boolean
  }

  const visiblePeers: ProcessingPeerInfo[] = []
  const commArea = new CommunicationArea(position2parcelRfc4(pos), commConfigurations.commRadius)

  for (const [peerAlias, trackingInfo] of getAllPeers()) {
    const msSinceLastUpdate = now - trackingInfo.lastUpdate

    if (msSinceLastUpdate > commConfigurations.peerTtlMs) {
      removePeerByAddress(peerAlias)
      continue
    }

    if (myAddress && trackingInfo.ethereumAddress === myAddress) {
      // If we are tracking a peer that is ourselves, we remove it
      removePeerByAddress(peerAlias)
      continue
    }

    if (!trackingInfo.position) {
      continue
    }

    if (!commArea.contains(trackingInfo.position)) {
      receiveUserVisible(peerAlias, false)
      continue
    }

    visiblePeers.push({
      squareDistance: squareDistanceRfc4(pos, trackingInfo.position),
      alias: peerAlias,
      visible: trackingInfo.visible || false
    })
  }

  if (visiblePeers.length <= maxVisiblePeers) {
    for (const peerInfo of visiblePeers) {
      receiveUserVisible(peerInfo.alias, true)
    }
  } else {
    const sortedBySqDistanceVisiblePeers = visiblePeers.sort((p1, p2) => p1.squareDistance - p2.squareDistance)
    for (let i = 0; i < sortedBySqDistanceVisiblePeers.length; ++i) {
      const peer = sortedBySqDistanceVisiblePeers[i]

      if (i < maxVisiblePeers) {
        receiveUserVisible(peer.alias, true)
      } else {
        receiveUserVisible(peer.alias, false)
      }
    }
  }
}
