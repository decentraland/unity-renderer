import { Avatar } from '@dcl/schemas'

export const REMOTE_AVATAR_IS_INVALID = `Remote avatar for profile is invalid`

export type ProfileStatus = 'ok' | 'error' | 'loading'

export type ProfileUserInfo = { status: 'ok' | 'loading' | 'error'; data: Avatar; addedToCatalog?: boolean }

export type ProfileState = {
  userInfo: {
    [key: string]: ProfileUserInfo
  }
  lastSentProfileVersion: Map<string, number>
}

export type RootProfileState = {
  profiles: ProfileState
}

export type ContentFile = {
  name: string
  content: Uint8Array
}

export interface RemoteProfile {
  timestamp: number
  avatars: Avatar[]
}
