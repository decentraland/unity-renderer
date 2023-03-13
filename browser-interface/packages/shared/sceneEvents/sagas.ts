import type { Avatar } from '@dcl/schemas'
import { call, select, takeLatest } from 'redux-saga/effects'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable } from 'shared/comms/peers'
import type { RootCommsState } from 'shared/comms/types'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { SET_REALM_ADAPTER } from 'shared/realm/actions'
import { realmToConnectionString } from 'shared/realm/resolver'
import { getRealmAdapter } from 'shared/realm/selectors'
import type { RootRealmState } from 'shared/realm/types'
import { toEnvironmentRealmType } from '../apis/host/EnvironmentAPI'
import { SET_COMMS_ISLAND, SET_ROOM_CONNECTION } from '../comms/actions'
import { getCommsIsland } from '../comms/selectors'
import { SAVE_DELTA_PROFILE_REQUEST } from '../profiles/actions'
import { takeLatestByUserId } from '../profiles/sagas'
import { allScenesEvent } from '../world/parcelSceneManager'

export function* sceneEventsSaga() {
  yield takeLatest([SET_COMMS_ISLAND, SET_ROOM_CONNECTION, SET_REALM_ADAPTER], islandChanged)
  yield takeLatestByUserId(SAVE_DELTA_PROFILE_REQUEST, submitProfileToScenes)
}

function* islandChanged() {
  const { adapter, island } = (yield select(getRealmAdapterAndIsland)) as ReturnType<typeof getRealmAdapterAndIsland>

  if (adapter) {
    const payload = toEnvironmentRealmType(adapter, island)
    yield call(allScenesEvent, { eventType: 'onRealmChanged', payload })
  }

  yield call(updateLocation, adapter ? realmToConnectionString(adapter) : undefined, island)
}
export function getRealmAdapterAndIsland(state: RootCommsState & RootRealmState) {
  return {
    adapter: getRealmAdapter(state),
    island: getCommsIsland(state)
  }
}

// @internal
export function updateLocation(realm: string | undefined, island: string | undefined) {
  const q = new URLSearchParams(window.location.search)
  if (realm) {
    q.set('realm', realm)
  } else {
    q.delete('realm')
  }
  if (island) {
    q.set('island', island)
  } else {
    q.delete('island')
  }
  history.replaceState({ island, realm }, '', `?${q.toString()}`)
}

function* submitProfileToScenes() {
  const profile: Avatar | null = yield select(getCurrentUserProfile)
  if (profile) {
    yield call(allScenesEvent, {
      eventType: 'profileChanged',
      payload: {
        ethAddress: profile.ethAddress,
        version: profile.version
      }
    })
  }
}

// this imperative code should be erased from the world
avatarMessageObservable.add((evt) => {
  // Tracks avatar state from comms side.
  // Listen to which avatars are visible or removed to keep track of connected and visible players.
  if (evt.type === AvatarMessageType.USER_VISIBLE) {
    allScenesEvent({
      eventType: evt.visible ? 'playerConnected' : 'playerDisconnected',
      payload: { userId: evt.userId }
    })
  } else if (evt.type === AvatarMessageType.USER_REMOVED) {
    allScenesEvent({
      eventType: 'playerDisconnected',
      payload: { userId: evt.userId }
    })
  }
})
