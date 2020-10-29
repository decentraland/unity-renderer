import { put, select, takeEvery } from 'redux-saga/effects'
import { getCurrentUserId } from 'shared/session/selectors'
import { getProfile } from 'shared/profiles/selectors'
import { saveProfileRequest } from 'shared/profiles/actions'
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
import { Profile } from 'shared/profiles/types'
import { unityInterface } from 'unity-interface/UnityInterface'

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
  const profile = yield getCurrentProfile()

  if (profile) {
    let idsToAdd = playersId
    let set: string[] = playersId
    if (profile[setKey]) {
      idsToAdd = playersId.filter((id) => !(profile[setKey].indexOf(id) >= 0))
      set = profile[setKey].concat(idsToAdd)
    }

    yield put(saveProfileRequest({ [setKey]: set }))
    if (setKey === 'muted') {
      unityInterface.SetUsersMuted(idsToAdd, true)
    }
  }
}

function* removePlayerFromProfileSet(playersId: string[], setKey: ProfileSetKey) {
  const profile = yield* getCurrentProfile()

  if (profile) {
    const set = profile[setKey] ? profile[setKey]!.filter((id) => !playersId.includes(id)) : []
    yield put(saveProfileRequest({ ...profile, [setKey]: set }))
    if (setKey === 'muted') {
      unityInterface.SetUsersMuted(playersId, false)
    }
  }
}

function* getCurrentProfile() {
  const address = yield select(getCurrentUserId)
  const profile: Profile | null = yield select(getProfile, address)
  return profile
}
