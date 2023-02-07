import type { ReadOnlyColor4 } from '@dcl/ecs-math'
import type { Snapshots } from '@dcl/schemas'

export type AvatarForRenderer = {
  bodyShape: string
  skinColor: ReadOnlyColor4
  hairColor: ReadOnlyColor4
  eyeColor: ReadOnlyColor4
  wearables: string[]
  emotes: {
    slot: number
    urn: string
  }[]
}

export type NewProfileForRenderer = {
  userId: string
  ethAddress: string
  name: string
  // @deprecated
  email: string
  snapshots: Snapshots
  blocked: string[]
  muted: string[]
  tutorialStep: number
  hasConnectedWeb3: boolean
  hasClaimedName: boolean
  baseUrl: string
  avatar: AvatarForRenderer

  // TODO evaluate usage of the following
  version: number
  description: string
  created_at: number
  updated_at: number
  inventory: string[]
  tutorialFlagsMask: number
}

export interface AddUserProfilesToCatalogPayload {
  users: NewProfileForRenderer[]
}
