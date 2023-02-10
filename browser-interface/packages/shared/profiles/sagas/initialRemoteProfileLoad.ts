import type { Avatar } from '@dcl/schemas'
import { ethereumConfigurations, ETHEREUM_NETWORK, RESET_TUTORIAL } from 'config'
import { call, put, select } from 'redux-saga/effects'
import { BringDownClientAndReportFatalError, ErrorContext } from 'shared/loading/ReportFatalError'
import defaultLogger from 'lib/logger'
import { getCurrentIdentity, getCurrentNetwork } from 'shared/session/selectors'
import type { ExplorerIdentity } from 'shared/session/types'
import { fetchOwnedENS } from 'shared/web3'
import { profileRequest, profileSuccess, saveProfileDelta } from '../actions'
import { fetchProfile } from './fetchProfile'

export function* initialRemoteProfileLoad() {
  // initialize profile
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId = identity.address

  let profile: Avatar | null = null

  try {
    profile = yield call(fetchProfile, profileRequest(userId, 0))
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

  // check that the user still has the claimed name, otherwise pick one

  if (profile.hasClaimedName || names.length) {
    if (!names.includes(profile.name)) {
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
