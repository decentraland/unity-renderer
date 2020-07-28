import { Store } from 'redux'

import { RootAtlasState } from 'shared/atlas/types'
import { RootProfileState } from 'shared/profiles/types'
import { RootDaoState } from 'shared/dao/types'
import { RootMetaState } from 'shared/meta/types'
import { RootChatState } from 'shared/chat/types'
import { RootCommsState } from 'shared/comms/types'
import { RootSessionState } from 'shared/session/types'
import { RootFriendsState } from 'shared/friends/types'
import { RootRendererState } from 'shared/renderer/types'

export type RootState = RootAtlasState &
  RootProfileState &
  RootDaoState &
  RootMetaState &
  RootChatState &
  RootCommsState &
  RootSessionState &
  RootFriendsState &
  RootRendererState

export type StoreContainer = { globalStore: Store<RootState> }
