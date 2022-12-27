import { Avatar } from '@dcl/schemas'

export const REMOTE_AVATAR_IS_INVALID = `Remote avatar for profile is invalid`

export type ProfileStatus = 'ok' | 'error' | 'loading'

export type ProfileUserInfo = { status: 'ok' | 'loading' | 'error'; data: Avatar; addedToCatalog?: boolean }

export type ProfileState = {
  userInfo: {
    [key: string]: ProfileUserInfo
  }
}

export type RootProfileState = {
  profiles: ProfileState
}

export type ContentFile = {
  name: string
  content: Uint8Array
}

// NEVER CHANGE THIS ENUM, IT IS USED IN THE WIRE PROTOCOL
export enum ProfileType {
  LOCAL = 0,
  DEPLOYED = 1
}

export interface RemoteProfile {
  timestamp: number
  avatars: Avatar[]
}
