import { getFatalError } from 'shared/loading/selectors'
import { getRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { RootState } from 'shared/store/rootTypes'
import { RoomConnection } from './interface'
import { RootCommsState } from './types'

export const getCommsIsland = (store: RootCommsState): string | undefined => store.comms.island
export const getCommsRoom = (state: RootCommsState): RoomConnection | undefined => state.comms.context

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
