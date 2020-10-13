import { put, select, takeEvery } from 'redux-saga/effects'
import { getCurrentUserId } from 'shared/session/selectors'
import { getProfile } from 'shared/profiles/selectors'
import { saveProfileRequest } from 'shared/profiles/actions'
import {
  BlockPlayer,
  BLOCK_PLAYER,
  MutePlayer,
  MUTE_PLAYER,
  UnblockPlayer,
  UNBLOCK_PLAYER,
  UnmutePlayer,
  UNMUTE_PLAYER
} from './actions'
import { Profile } from 'shared/profiles/types'

type ProfileSetKey = 'muted' | 'blocked'

export function* socialSaga(): any {
  yield takeEvery(MUTE_PLAYER, saveMutedPlayer)
  yield takeEvery(BLOCK_PLAYER, saveBlockedPlayer)
  yield takeEvery(UNMUTE_PLAYER, saveUnmutedPlayer)
  yield takeEvery(UNBLOCK_PLAYER, saveUnblockedPlayer)
}

function* saveMutedPlayer(action: MutePlayer) {
  yield* addPlayerToProfileSet(action.payload.playerId, 'muted')
}

function* saveBlockedPlayer(action: BlockPlayer) {
  yield* addPlayerToProfileSet(action.payload.playerId, 'blocked')
}

function* saveUnmutedPlayer(action: UnmutePlayer) {
  yield* removePlayerFromProfileSet(action.payload.playerId, 'muted')
}

function* saveUnblockedPlayer(action: UnblockPlayer) {
  yield* removePlayerFromProfileSet(action.payload.playerId, 'blocked')
}

function* addPlayerToProfileSet(playerId: string, setKey: ProfileSetKey) {
  const profile = yield getCurrentProfile()

  if (profile) {
    let set: string[] = [playerId]
    if (profile[setKey]) {
      if (profile[setKey].indexOf(playerId) >= 0) {
        return
      }

      set = [...profile[setKey], playerId]
    }

    yield put(saveProfileRequest({ [setKey]: set }))
  }
}

function* removePlayerFromProfileSet(playerId: string, setKey: ProfileSetKey) {
  const profile = yield* getCurrentProfile()

  if (profile) {
    const set = profile[setKey] ? profile[setKey]!.filter((id) => id !== playerId) : []
    yield put(saveProfileRequest({ ...profile, [setKey]: set }))
  }
}

function* getCurrentProfile() {
  const address = yield select(getCurrentUserId)
  const profile: Profile | null = yield select(getProfile, address)
  return profile
}
