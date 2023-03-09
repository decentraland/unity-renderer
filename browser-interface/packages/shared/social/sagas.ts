import { call, put, select, takeEvery } from 'redux-saga/effects'
import { getCurrentUserId } from 'shared/session/selectors'
import { getProfile as profileSelector } from 'shared/profiles/selectors'
import { saveProfileDelta } from 'shared/profiles/actions'
import {
  BlockPlayers,
  BLOCK_PLAYERS,
  MutePlayers,
  MUTE_PLAYERS,
  UnblockPlayers,
  UNBLOCK_PLAYERS,
  UnmutePlayers,
  UNMUTE_PLAYERS
} from './actions'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { Avatar } from '@dcl/schemas'
import { validateAvatar } from 'shared/profiles/schemaValidation'

type ProfileSetKey = 'muted' | 'blocked'

export function* socialSaga(): any {
  yield takeEvery(MUTE_PLAYERS, saveMutedPlayer)
  yield takeEvery(BLOCK_PLAYERS, saveBlockedPlayer)
  yield takeEvery(UNMUTE_PLAYERS, saveUnmutedPlayer)
  yield takeEvery(UNBLOCK_PLAYERS, saveUnblockedPlayer)
}

function* saveMutedPlayer(action: MutePlayers) {
  yield* addPlayerToProfileSet(action.payload.playersId, 'muted')
}

function* saveBlockedPlayer(action: BlockPlayers) {
  yield* addPlayerToProfileSet(action.payload.playersId, 'blocked')
}

function* saveUnmutedPlayer(action: UnmutePlayers) {
  yield* removePlayerFromProfileSet(action.payload.playersId, 'muted')
}

function* saveUnblockedPlayer(action: UnblockPlayers) {
  yield* removePlayerFromProfileSet(action.payload.playersId, 'blocked')
}

function* addPlayerToProfileSet(playersId: string[], setKey: ProfileSetKey) {
  const profile: Avatar | null = yield call(getCurrentProfile)

  if (profile) {
    let idsToAdd = playersId
    let set: string[] = playersId

    if (profile[setKey]) {
      idsToAdd = playersId.filter((id) => !profile[setKey]!.includes(id))
      set = profile[setKey]!.concat(idsToAdd)
    }

    yield put(saveProfileDelta({ [setKey]: set }))
    if (setKey === 'muted') {
      yield call(waitForRendererInstance)
      getUnityInstance().SetUsersMuted(idsToAdd, true)
    }
  }
}

function* removePlayerFromProfileSet(playersId: string[], setKey: ProfileSetKey) {
  const profile = yield call(getCurrentProfile)

  if (profile) {
    const set = profile[setKey] ? profile[setKey]!.filter((id) => !playersId.includes(id)) : []
    yield put(saveProfileDelta({ ...profile, [setKey]: set }))
    if (setKey === 'muted') {
      yield call(waitForRendererInstance)
      getUnityInstance().SetUsersMuted(playersId, false)
    }
  }
}

function* getCurrentProfile() {
  const address: string | undefined = yield select(getCurrentUserId)
  if (!address) return null
  const profile: Avatar | null = yield select(profileSelector, address)
  if (!profile || !validateAvatar(profile)) return null
  return profile
}
