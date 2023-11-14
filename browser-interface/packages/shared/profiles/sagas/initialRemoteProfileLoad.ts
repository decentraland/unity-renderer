import type {Avatar} from '@dcl/schemas'
import {ETHEREUM_NETWORK, ethereumConfigurations, RESET_TUTORIAL} from 'config'
import defaultLogger from 'lib/logger'
import {call, put, select} from 'redux-saga/effects'
import {BringDownClientAndReportFatalError, ErrorContext} from 'shared/loading/ReportFatalError'
import {getCurrentIdentity, getCurrentNetwork, isGuestLogin} from 'shared/session/selectors'
import type {ExplorerIdentity} from 'shared/session/types'
import {fetchOwnedENS} from 'lib/web3/fetchOwnedENS'
import {profileSuccess, saveProfileDelta} from '../actions'
import {fetchProfileFromCatalyst} from './fetchProfile'
import {fetchLocalProfile} from './local/index'
import {getFeatureFlagEnabled} from 'shared/meta/selectors'
import {generateRandomUserProfile} from "../../../lib/decentraland/profiles";
import {ensureAvatarCompatibilityFormat} from "../../../lib/decentraland/profiles/transformations";

export function* initialRemoteProfileLoad() {
  // initialize profile
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId = identity.address

  const isGuest = yield select(isGuestLogin)
  let profile: Avatar | null = yield call(fetchLocalProfile, isGuest)
  try {
    if (!isGuest) {
      const profileFromCatalyst = yield call(
        fetchProfileFromCatalyst,
        userId,
        profile && profile.userId === userId ? profile.version : 0
      )

      if (profileFromCatalyst) {
        profile = profileFromCatalyst;
      }
    }

    if (!profile) {
      profile = ensureAvatarCompatibilityFormat(generateRandomUserProfile(userId))
    }
  } catch (e: any) {
    BringDownClientAndReportFatalError(e, ErrorContext.KERNEL_INIT, { userId })
    throw e
  }

  if (!profile) {
    const error = new Error('Could not initialize profile')
    BringDownClientAndReportFatalError(error, ErrorContext.KERNEL_INIT, { userId })
    throw error
  }

  let profileDirty: boolean = false

  const net: ETHEREUM_NETWORK = yield select(getCurrentNetwork)
  const names: string[] = yield call(fetchOwnedENS, ethereumConfigurations[net].names, userId)

  // Check for consolidating strategies if the user has claimed name or owned names are available for the address
  if (profile.hasClaimedName || names.length) {
    const isMyAccountEnabled: boolean = yield select(getFeatureFlagEnabled, 'my_account')
    if (names.includes(profile.name)) {
      if (!profile.hasClaimedName) {
        // If the user has a name assigned and matches one of their owned names, consolidate the hasClaimedName setting to true
        defaultLogger.info(
          `Name currently set (${profile.name}) matches a claimed name for profile ${userId}, consolidating profile...`
        )
        profile = {
          ...profile,
          hasClaimedName: true,
          version: profile?.version || 0
        }
        profileDirty = true
      }
    } else if (!isMyAccountEnabled) {
      // User no longer has the current name but might have another, pick that one if available
      if (names.length) {
        defaultLogger.info(`Found missing claimed name '${names[0]}' for profile ${userId}, consolidating profile... `)
        profile = {
          ...profile,
          name: names[0],
          hasClaimedName: true,
          version: profile?.version || 0,
          tutorialStep: 0xfff
        }
      } else {
        // User has no available names, set the hasClaimedName to false
        profile = { ...profile, hasClaimedName: false, tutorialStep: 0x0 }
      }
      profileDirty = true
    }
  }

  if (RESET_TUTORIAL) {
    profile = { ...profile, tutorialStep: 0 }
    profileDirty = true
  }

  // if the profile is dirty, then save it
  if (profileDirty) {
    yield put(saveProfileDelta(profile))
  }
  yield put(profileSuccess(profile))
  return profile
}
