import { Avatar } from '@dcl/schemas'
import { createFakeName } from 'lib/decentraland/profiles/names/fakeName'
import { deepEqual } from 'lib/javascript/deepEqual'
import defaultLogger from 'lib/logger'
import { apply, call, put, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { waitForNetworkSelected } from 'shared/meta/sagas'
import { getCurrentIdentity, getCurrentNetwork, getCurrentUserId } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'
import type { SaveProfileDelta } from '../actions'
import { deployProfile, profileSuccess, saveProfileFailure } from '../actions'
import { validateAvatar } from '../schemaValidation'
import { getCurrentUserProfileDirty } from '../selectors'
import { localProfilesRepo } from './local/localProfilesRepo'

function getInformationForSaveAvatar(state: RootState) {
  return {
    userId: getCurrentUserId(state),
    savedProfile: getCurrentUserProfileDirty(state),
    identity: getCurrentIdentity(state),
    network: getCurrentNetwork(state)
  }
}

export function* handleSaveLocalAvatar(saveAvatar: SaveProfileDelta) {
  const { userId, savedProfile, identity, network } = (yield select(getInformationForSaveAvatar)) as ReturnType<
    typeof getInformationForSaveAvatar
  >
  if (userId !== identity?.address) {
    // This function should only save the current user's avatar
    defaultLogger.error(
      `The saga handleSaveLocalAvatar was called for user ${userId} which is not the current user ${identity?.address}`
    )
    return
  }
  if (!userId) {
    defaultLogger.error(`The saga handleSaveLocalAvatar was called when no ${userId} was available`)
    return
  }

  try {
    const currentVersion: number = Math.max(savedProfile?.version || 0, 0)

    const profile: Avatar = {
      // Default values to populate if they are missing
      hasClaimedName: false,
      description: '',
      tutorialStep: 0,

      // Populate with the current data
      ...savedProfile,

      // Append information from the action
      ...saveAvatar.payload.profile,

      // Prevent unwanted override of these values
      userId,
      ethAddress: userId,
      hasConnectedWeb3: identity?.hasConnectedWeb3
    } as Avatar

    if (deepEqual(savedProfile, profile)) {
      // No action required
      defaultLogger.warn(`The saga handleSaveLocalAvatar was called with no change to the avatar`)
      return
    }

    // Otherwise, sanitize and increase the version
    profile.version = currentVersion + 1
    if (!profile.name) {
      profile.name = createFakeName(profile.userId)
    }

    if (!validateAvatar(profile)) {
      defaultLogger.error('error validating schemas', validateAvatar.errors)
      trackEvent('invalid_schema', {
        schema: 'avatar',
        payload: profile,
        errors: (validateAvatar.errors ?? []).map(($) => $.message).join(',')
      })
    }

    // Ensure the network was set
    if (!network) {
      yield call(waitForNetworkSelected)
    }
    // Save the profile in local storage
    yield apply(localProfilesRepo, localProfilesRepo.persist, [profile.ethAddress, network!, profile])

    // Save the profile in the store
    yield put(profileSuccess(profile))

    // Update profile for other users if the user has a wallet
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
