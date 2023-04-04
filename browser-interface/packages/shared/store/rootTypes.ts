import { Store } from 'redux'

import { RootAtlasState } from 'shared/atlas/types'
import { RootCatalogState } from 'shared/catalogs/types'
import { RootChatState } from 'shared/chat/types'
import { RootCommsState } from 'shared/comms/types'
import { RootDaoState } from 'shared/dao/types'
import { RootFriendsState } from 'shared/friends/types'
import { RootMetaState } from 'shared/meta/types'
import { RootPortableExperiencesState } from 'shared/portableExperiences/types'
import { RootProfileState } from 'shared/profiles/types'
import { RootQuestsState } from 'shared/quests/types'
import { RootRealmState } from 'shared/realm/types'
import { RootRendererState } from 'shared/renderer/types'
import { RootSceneLoaderState } from 'shared/scene-loader/types'
import { RootSessionState } from 'shared/session/types'
import { RootVoiceChatState } from 'shared/voiceChat/types'
import { RootWearablesPortableExperienceState } from 'shared/wearablesPortableExperience/types'
import { RootWorldState } from 'shared/world/types'
import { RootLoadingState } from 'shared/loading/reducer'

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
