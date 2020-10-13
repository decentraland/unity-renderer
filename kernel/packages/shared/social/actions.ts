import { action } from 'typesafe-actions'

// Block / Mute updates

export const BLOCK_PLAYER = 'BLOCK_PLAYER'
export const blockPlayer = (playerId: string) => action(BLOCK_PLAYER, { playerId })
export type BlockPlayer = ReturnType<typeof blockPlayer>

export const MUTE_PLAYER = 'MUTE_PLAYER'
export const mutePlayer = (playerId: string) => action(MUTE_PLAYER, { playerId })
export type MutePlayer = ReturnType<typeof mutePlayer>

export const UNMUTE_PLAYER = 'UNMUTE_PLAYER'
export const unmutePlayer = (playerId: string) => action(UNMUTE_PLAYER, { playerId })
export type UnmutePlayer = ReturnType<typeof unmutePlayer>

export const UNBLOCK_PLAYER = 'UNBLOCK_PLAYER'
export const unblockPlayer = (playerId: string) => action(UNBLOCK_PLAYER, { playerId })
export type UnblockPlayer = ReturnType<typeof unblockPlayer>
