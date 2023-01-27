import { RoomConnection } from './interface'
import { RootCommsState } from './types'

export const getCommsIsland = (store: RootCommsState): string | undefined => store.comms.island
export const getCommsRoom = (state: RootCommsState): RoomConnection | undefined => state.comms.context
