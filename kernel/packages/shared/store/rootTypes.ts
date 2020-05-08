import { AtlasState } from '../atlas/types'
import { ProfileState } from '../profiles/types'
import { DaoState } from '../dao/types'
import { MetaState } from '../meta/types'
import { ChatState } from '../chat/types'
import { Store } from 'redux'

export type RootState = {
  atlas: AtlasState
  profiles: ProfileState
  dao: DaoState
  meta: MetaState
  chat: ChatState
}

export type StoreContainer = { globalStore: Store<RootState> }
