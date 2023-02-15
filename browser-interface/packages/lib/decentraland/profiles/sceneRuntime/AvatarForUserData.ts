import type { Snapshots, WearableId } from '@dcl/schemas'

export type AvatarForUserData = {
  bodyShape: WearableId
  skinColor: string
  hairColor: string
  eyeColor: string
  wearables: WearableId[]
  emotes?: {
    slot: number
    urn: string
  }[]
  snapshots: Snapshots
}
