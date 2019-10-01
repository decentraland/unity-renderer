import { action } from 'typesafe-actions'
import { Context } from '../comms'

export const SET_WORLD_CONTEXT = 'Set world connection context'
export const setWorldContext = (context: Context) => action(SET_WORLD_CONTEXT, context)
export type SetWorldContextAction = ReturnType<typeof setWorldContext>

export const ANNOUNCE_PROFILE = '[Request] Announce profile to nearby users'
export const announceProfile = (userId: string, version: number) => action(ANNOUNCE_PROFILE, { userId, version })
export type AnnounceProfileAction = ReturnType<typeof announceProfile>
