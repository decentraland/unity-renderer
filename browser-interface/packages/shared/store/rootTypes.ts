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
import { RootCatalogState } from 'shared/catalogs/types'
import { RootLoadingState } from '../loading/reducer'
import { RootQuestsState } from 'shared/quests/types'
import { RootPortableExperiencesState } from 'shared/portableExperiences/types'
import { RootWearablesPortableExperienceState } from 'shared/wearablesPortableExperience/types'
import { RootVoiceChatState } from 'shared/voiceChat/types'
import { RootRealmState } from 'shared/realm/types'
import { RootSceneLoaderState } from 'shared/scene-loader/types'
import { RootWorldState } from 'shared/world/types'

export type RootState = RootAtlasState &
  RootProfileState &
  RootDaoState &
  RootMetaState &
  RootChatState &
  RootRealmState &
  RootCommsState &
  RootSessionState &
  RootFriendsState &
  RootRendererState &
  RootLoadingState &
  RootCatalogState &
  RootQuestsState &
  RootPortableExperiencesState &
  RootSceneLoaderState &
  RootWearablesPortableExperienceState &
  RootWorldState &
  RootVoiceChatState

export type RootStore = Store<RootState>
