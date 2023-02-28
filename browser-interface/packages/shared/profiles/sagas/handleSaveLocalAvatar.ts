import { call, put, select } from 'redux-saga/effects'
import { ETHEREUM_NETWORK } from 'config'
import defaultLogger from 'lib/logger'
import type { SaveProfileDelta } from '../actions'
import { saveProfileFailure, deployProfile, profileSuccess } from '../actions'
import { getCurrentUserProfileDirty } from '../selectors'
import type { ExplorerIdentity } from 'shared/session/types'
import { getCurrentUserId, getCurrentIdentity, getCurrentNetwork } from 'shared/session/selectors'
import { createFakeName } from 'lib/decentraland/profiles/names/fakeName'
import type { Avatar } from '@dcl/schemas'
import { validateAvatar } from '../schemaValidation'
import { trackEvent } from 'shared/analytics'
import { localProfilesRepo } from './local/localProfilesRepo'

export function* handleSaveLocalAvatar(saveAvatar: SaveProfileDelta) {
  const userId: string = yield select(getCurrentUserId)

  try {
    // get the avatar, no matter if it is in a loading or dirty state
    const savedProfile: Avatar | null = yield select(getCurrentUserProfileDirty)
    const currentVersion: number = Math.max(savedProfile?.version || 0, 0)

    const identity: ExplorerIdentity = yield select(getCurrentIdentity)
    const network: ETHEREUM_NETWORK = yield select(getCurrentNetwork)

    const profile: Avatar = {
      hasClaimedName: false,
      name: createFakeName(userId),
      description: '',
      tutorialStep: 0,
      ...savedProfile,
      ...saveAvatar.payload.profile,
      userId,
      version: currentVersion + 1,
      ethAddress: userId,
      hasConnectedWeb3: identity.hasConnectedWeb3
    } as Avatar

    if (!validateAvatar(profile)) {
      defaultLogger.error('error validating schemas', validateAvatar.errors)
      trackEvent('invalid_schema', {
        schema: 'avatar',
        payload: profile,
        errors: (validateAvatar.errors ?? []).map(($) => $.message).join(',')
      })
    }

    // save the profile in the local storage
    yield call(() => localProfilesRepo.persist(profile.ethAddress, network, profile))

    // save the profile in the store
    yield put(profileSuccess(profile))

    // only update profile on server if wallet is connected
    if (profile.hasConnectedWeb3) {
      yield put(deployProfile(profile))
    }
  } catch (error: any) {
    trackEvent('error', {
      message: `cant_persist_avatar ${error}`,
      context: 'kernel#saga',
      stack: error.stacktrace
    })
    yield put(saveProfileFailure(userId, 'unknown reason'))
  }
}
