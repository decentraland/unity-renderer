import { AtlasState } from '../atlas/types'
import { ProfileState } from '../profiles/types'
import { DaoState } from '../dao/types'
import { MetaState } from '../meta/types'
import { ChatState } from '../chat/types'
import { CommsState } from '../comms/reducer'

import { Store } from 'redux'

export type RootState = {
  atlas: AtlasState
  profiles: ProfileState
  dao: DaoState
  meta: MetaState
  chat: ChatState
  comms: CommsState
}

export type StoreContainer = { globalStore: Store<RootState> }
