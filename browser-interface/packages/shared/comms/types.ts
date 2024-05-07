import { CommsEstablished } from 'shared/loading/types'
import { HandleRoomDisconnection, SetCommsIsland, SetLiveKitAdapterAction, SetRoomConnectionAction } from './actions'
import { LivekitAdapter } from './adapters/LivekitAdapter'
import { RoomConnection } from './interface'

export type CommsState = {
  initialized: boolean
  island?: string
  context: RoomConnection | undefined,
  livekitAdapter?: LivekitAdapter
}

export type CommsActions = 
 | SetCommsIsland
 | SetRoomConnectionAction
 | HandleRoomDisconnection
 | SetLiveKitAdapterAction
 | CommsEstablished

export type CommsConnectionState =
  | 'initial'
  | 'connecting'
  | 'connected'
  | 'error'
  | 'realm-full'
  | 'reconnection-error'
  | 'id-taken'
  | 'disconnecting'

export type CommsStatus = {
  status: CommsConnectionState
  connectedPeers: number
}

export type RootCommsState = {
  comms: CommsState
}
// These types appear to be unavailable when compiling for some reason, so we add them here

type RTCIceCredentialType = 'password'

export interface RTCIceServer {
  credential?: string
  credentialType?: RTCIceCredentialType
  urls: string | string[]
  username?: string
}
