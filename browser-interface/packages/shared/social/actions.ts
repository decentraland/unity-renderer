import { action } from 'typesafe-actions'

// Block / Mute updates

export const BLOCK_PLAYERS = 'BLOCK_PLAYERS'
export const blockPlayers = (playersId: string[]) => action(BLOCK_PLAYERS, { playersId })
export type BlockPlayers = ReturnType<typeof blockPlayers>

export const MUTE_PLAYERS = 'MUTE_PLAYERS'
export const mutePlayers = (playersId: string[]) => action(MUTE_PLAYERS, { playersId })
export type MutePlayers = ReturnType<typeof mutePlayers>

export const UNMUTE_PLAYERS = 'UNMUTE_PLAYERS'
export const unmutePlayers = (playersId: string[]) => action(UNMUTE_PLAYERS, { playersId })
export type UnmutePlayers = ReturnType<typeof unmutePlayers>

export const UNBLOCK_PLAYERS = 'UNBLOCK_PLAYERS'
export const unblockPlayers = (playersId: string[]) => action(UNBLOCK_PLAYERS, { playersId })
export type UnblockPlayers = ReturnType<typeof unblockPlayers>
