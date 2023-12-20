import { getFatalError } from 'shared/loading/selectors'
import { getRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { RootState } from 'shared/store/rootTypes'
import { RoomConnection } from './interface'
import { RootCommsState } from './types'
import { ActiveVideoStreams } from './adapters/types'
import {
  AnnounceProfileVersion,
  ProfileRequest,
  ProfileResponse,
  Position,
  Scene,
  Chat,
  Voice
} from '../protocol/decentraland/kernel/comms/rfc4/comms.gen'

export const getCommsIsland = (store: RootCommsState): string | undefined => store.comms.island
export const getSceneRoomComms = (state: RootCommsState): RoomConnection | undefined => state.comms.scene
export const getSceneRooms = (state: RootCommsState): Map<string, RoomConnection> => state.comms.scenes

export const getCommsRoom = (state: RootCommsState): RoomConnection | undefined => {
  const islandRoom = state.comms.context
  const sceneRoom = state.comms.scene

  if (!islandRoom) return undefined

  return {
    connect: async () => {
      debugger
    },
    // events: islandRoom.events,
    disconnect: async () => {
      await islandRoom.disconnect()
      // TBD: should we disconnect from scenes here too ?
    },
    // TBD: This should be only be sent by the island ?
    // We may remove this before reach production, but to think about it
    sendProfileMessage: async (profile: AnnounceProfileVersion) => {
      const island = islandRoom.sendProfileMessage(profile)
      const scene = sceneRoom?.sendProfileMessage(profile)
      await Promise.all([island, scene])
    },
    sendProfileRequest: async (request: ProfileRequest) => {
      const island = islandRoom.sendProfileRequest(request)
      const scene = sceneRoom?.sendProfileRequest(request)
      await Promise.all([island, scene])
    },
    sendProfileResponse: async (response: ProfileResponse) => {
      const island = islandRoom.sendProfileResponse(response)
      const scene = sceneRoom?.sendProfileResponse(response)
      await Promise.all([island, scene])
    },
    sendPositionMessage: async (position: Omit<Position, 'index'>) => {
      const island = islandRoom.sendPositionMessage(position)
      const scene = sceneRoom?.sendPositionMessage(position)
      await Promise.all([island, scene])
    },
    sendParcelSceneMessage: async (message: Scene) => {
      if (message.sceneId !== sceneRoom?.id) {
        console.warn('Ignoring Scene Message', { sceneId: message.sceneId, connectedSceneId: sceneRoom?.id })
        return
      }
      // const island = islandRoom.sendParcelSceneMessage(message)
      await sceneRoom?.sendParcelSceneMessage(message)
    },
    sendChatMessage: async (message: Chat) => {
      const island = islandRoom.sendChatMessage(message)
      const scene = sceneRoom?.sendChatMessage(message)
      await Promise.all([island, scene])
    },
    // TBD: how voice chat works?
    sendVoiceMessage: async (message: Voice) => {
      if (!sceneRoom) debugger
      return sceneRoom!.sendVoiceMessage(message)
    },
    createVoiceHandler: async () => {
      // TBD: Feature flag for backwards compatibility
      if (!sceneRoom) {
        debugger
        throw new Error('Scene room not avaialble')
      }
      return sceneRoom.createVoiceHandler()
    }
  } as any as RoomConnection
}

export function reconnectionState(state: RootState): {
  commsConnection: RoomConnection | undefined
  realmAdapter: IRealmAdapter | undefined
  hasFatalError: string | null
  identity: ExplorerIdentity | undefined
} {
  return {
    commsConnection: getCommsRoom(state),
    realmAdapter: getRealmAdapter(state),
    hasFatalError: getFatalError(state),
    identity: getCurrentIdentity(state)
  }
}

export const getLivekitActiveVideoStreams = (store: RootCommsState): Map<string, ActiveVideoStreams> | undefined =>
  store.comms.livekitAdapter?.getActiveVideoStreams()
