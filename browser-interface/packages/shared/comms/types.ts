import { RoomConnection } from './interface'

export type CommsState = {
  initialized: boolean
  island?: string
  context: RoomConnection | undefined
}

export type RootCommsState = {
  comms: CommsState
}