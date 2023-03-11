import type { Avatar } from '@dcl/schemas'
import { createFakeName } from 'lib/decentraland/profiles/names/fakeName'
import defaultLogger from 'lib/logger'
import { call, put, select } from 'redux-saga/effects'
import { trackError, trackEvent } from 'shared/analytics/trackEvent'
import { getCurrentIdentity, getCurrentNetwork, getCurrentUserId } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'
import type { SaveProfileDelta } from '../actions'
import { deployProfile, profileSuccess, saveProfileFailure } from '../actions'
import { validateAvatar } from '../schemaValidation'
import { getCurrentUserProfileDirty } from '../selectors'
import { localProfilesRepo } from './local/localProfilesRepo'

export function* handleSaveLocalAvatar(saveAvatar: SaveProfileDelta) {
  const { userId, savedProfile, identity, network } = (yield select(getInformationToSaveLocalAvatar)) as ReturnType<
    typeof getInformationToSaveLocalAvatar
  >

  try {
    // get the avatar, no matter if it is in a loading or dirty state
    const currentVersion: number = Math.max(savedProfile?.version || 0, 0)

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
    trackError('kernel#saga', error, 'cant_persist_avatar ${error}')
    yield put(saveProfileFailure(userId, 'unknown reason'))
  }
}

function getInformationToSaveLocalAvatar(state: RootState) {
  return {
    // TODO: Validate if getCurrentUserId, getCurrentIdentity, getCurrentNetwork are always truthy
    userId: getCurrentUserId(state)!,
    savedProfile: getCurrentUserProfileDirty(state),
    identity: getCurrentIdentity(state)!,
    network: getCurrentNetwork(state)!
  }
}
