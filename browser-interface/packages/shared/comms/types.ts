import { CommsEstablished } from 'shared/loading/types'
import { HandleRoomDisconnection, SetCommsIsland, SetLiveKitAdapterAction, SetRoomConnectionAction, SetSceneRoomConnectionAction } from './actions'
import { LivekitAdapter } from './adapters/LivekitAdapter'
import { RoomConnection } from './interface'

type SceneId = string

export type CommsState = {
  initialized: boolean
  island?: string
  context: RoomConnection | undefined
  livekitAdapter?: LivekitAdapter
  scene: RoomConnection | undefined
  scenes: Map<SceneId, RoomConnection>
}

export type CommsActions =
 | SetCommsIsland
 | SetRoomConnectionAction
 | HandleRoomDisconnection
 | SetLiveKitAdapterAction
 | CommsEstablished
 | SetSceneRoomConnectionAction

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
