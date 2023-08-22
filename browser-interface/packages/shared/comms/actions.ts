import { action } from 'typesafe-actions'
import { RoomConnection } from './interface'
import { LivekitAdapter } from './adapters/LivekitAdapter'

export const SET_COMMS_ISLAND = '[COMMS] setCommsIsland'
export const setCommsIsland = (island: string | undefined) => action(SET_COMMS_ISLAND, { island })
export type SetCommsIsland = ReturnType<typeof setCommsIsland>

export const SET_ROOM_CONNECTION = '[COMMS] setRoomConnection'
export const setRoomConnection = (room: RoomConnection | undefined) => action(SET_ROOM_CONNECTION, room)
export type SetRoomConnectionAction = ReturnType<typeof setRoomConnection>

export const HANDLE_ROOM_DISCONNECTION = '[COMMS] handleRoomDisconnection'
export const handleRoomDisconnection = (room: RoomConnection) => action(HANDLE_ROOM_DISCONNECTION, { context: room })
export type HandleRoomDisconnection = ReturnType<typeof handleRoomDisconnection>

export const SET_LIVEKIT_ADAPTER = '[COMMS] setLiveKitAdapter'
export const setLiveKitAdapter = (livekitAdapter: LivekitAdapter | undefined) => action(SET_LIVEKIT_ADAPTER, livekitAdapter)
export type SetLiveKitAdapterAction = ReturnType<typeof setLiveKitAdapter>