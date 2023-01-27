import { NewProfileForRenderer } from 'shared/profiles/transformations/types'
import * as rfc4 from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'

export const enum AvatarMessageType {
  // Networking related messages
  USER_DATA = 'USER_DATA',
  USER_VISIBLE = 'USER_VISIBLE',
  USER_EXPRESSION = 'USER_EXPRESSION',
  USER_REMOVED = 'USER_REMOVED',
  USER_TALKING = 'USER_TALKING',

  // TODO(mendez): what emits this messages?
  // Actions related messages
  USER_MUTED = 'USER_MUTED',
  USER_UNMUTED = 'USER_UNMUTED',
  USER_BLOCKED = 'USER_BLOCKED',
  USER_UNBLOCKED = 'USER_UNBLOCKED'
}

export type ReceiveUserExpressionMessage = {
  type: AvatarMessageType.USER_EXPRESSION
  userId: string
  expressionId: string
  timestamp: number
}

export type ReceiveUserDataMessage = {
  type: AvatarMessageType.USER_DATA
  userId: string
  data: Partial<UserInformation>
  profile: NewProfileForRenderer
}

export type ReceiveUserVisibleMessage = {
  type: AvatarMessageType.USER_VISIBLE
  userId: string
  visible: boolean
}

export type ReceiveUserTalkingMessage = {
  type: AvatarMessageType.USER_TALKING
  userId: string
  talking: boolean
}

export type UserRemovedMessage = {
  type: AvatarMessageType.USER_REMOVED
  userId: string
}

export type UserMessage = {
  type:
    | AvatarMessageType.USER_BLOCKED
    | AvatarMessageType.USER_UNBLOCKED
    | AvatarMessageType.USER_MUTED
    | AvatarMessageType.USER_UNMUTED
    | AvatarMessageType.USER_TALKING
  userId: string
}

export type AvatarMessage =
  | ReceiveUserDataMessage
  | ReceiveUserVisibleMessage
  | ReceiveUserExpressionMessage
  | UserRemovedMessage
  | UserMessage
  | ReceiveUserTalkingMessage

/**
 * This type contains information about the peers, the AvatarEntity must accept this whole object in setAttributes(obj).
 */
export type PeerInformation = UserInformation & {
  talking: boolean
  lastPositionIndex: number
  lastProfileVersion: number
  lastUpdate: number
}

export type UserInformation = {
  ethereumAddress: string
  // base URL to resolve the contents of the assets of the avatar
  baseUrl?: string
  expression?: AvatarExpression
  position?: rfc4.Position
  visible?: boolean
}

export type AvatarExpression = {
  expressionType: string
  expressionTimestamp: number
}

export type PackageType = keyof rfc4.Packet

export type Package<T> = {
  address: string
  data: T
}
