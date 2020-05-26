export type AirdropInput = {
  id: string
  title: string
  subtitle: string
  items: AirdropItem[]
}
export type AirdropInfo = {
  id: string
} & AirdropInput

export type RarityEnum = 'common' | 'uncommon' | 'rare' | 'epic' | 'mythic' | 'legendary' | 'unique'

export type AirdropItem = {
  name: string
  subtitle?: string
  thumbnailURL: string
  rarity: RarityEnum
  type: 'collectible' | 'erc20'
}

export type unityAirdropInterface = {
  TriggerAirdropDisplay(data: AirdropInfo): void
}

export interface IAirdropController {
  openCrate(campaignId: string, chestId: string): Promise<void>
}
