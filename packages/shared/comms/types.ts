export enum AvatarMessageType {
  // Networking related messages
  USER_DATA = 'USER_DATA',
  USER_POSE = 'USER_POSE',
  USER_VISIBLE = 'USER_VISIBLE',
  USER_REMOVED = 'USER_REMOVED',
  SET_LOCAL_UUID = 'SET_LOCAL_UUID',

  // Actions related messages
  USER_MUTED = 'USER_MUTED',
  USER_UNMUTED = 'USER_UNMUTED',
  USER_BLOCKED = 'USER_BLOCKED',
  USER_UNBLOCKED = 'USER_UNBLOCKED',

  ADD_FRIEND = 'ADD_FRIEND',
  SHOW_PROFILE = 'SHOW_PROFILE',
  HIDE_PROFILE = 'HIDE_PROFILE'
}

export type ReceiveUserDataMessage = {
  type: AvatarMessageType.USER_DATA
  uuid: string
  data: Partial<UserInformation>
}

export type ReceiveUserVisibleMessage = {
  type: AvatarMessageType.USER_VISIBLE
  uuid: string
  visible: boolean
}

export type ReceiveUserPoseMessage = {
  type: AvatarMessageType.USER_POSE
  uuid: string
  pose: Pose
}

export type UserRemovedMessage = {
  type: AvatarMessageType.USER_REMOVED
  uuid: string
}

export type HideProfileMessage = {
  type: AvatarMessageType.HIDE_PROFILE
}

export type ShowProfileMessage = {
  type: AvatarMessageType.SHOW_PROFILE
  uuid: string
  publicKey: string
  displayName: string
  isMuted: boolean
  isBlocked: boolean
  avatarUrl?: string
}

export type UserMessage = {
  type:
    | AvatarMessageType.SET_LOCAL_UUID
    | AvatarMessageType.USER_BLOCKED
    | AvatarMessageType.USER_UNBLOCKED
    | AvatarMessageType.USER_MUTED
    | AvatarMessageType.USER_UNMUTED
  uuid: string
}

export type AvatarMessage =
  | ReceiveUserDataMessage
  | ReceiveUserPoseMessage
  | ReceiveUserVisibleMessage
  | UserRemovedMessage
  | UserMessage
  | ShowProfileMessage
  | HideProfileMessage

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
