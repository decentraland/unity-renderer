import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import type { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations/types'
import { deepEqual } from 'lib/javascript/deepEqual'
import { call, put, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { updateAvatarScenePeerData } from 'shared/comms/peers'
import { addProfileToLastSentProfileVersionAndCatalog, SendProfileToRenderer } from 'shared/profiles/actions'
import { getLastSentProfileVersion, getProfileFromStore } from 'shared/profiles/selectors'
import { getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import type { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getCurrentIdentity } from 'shared/session/selectors'
import type { RootState } from 'shared/store/rootTypes'
import { getUnityInterface } from 'unity-interface/IUnityInterface'
import { waitForRendererInstance } from '../sagas-helper'

let lastSentProfile: NewProfileForRenderer | null = null
export function* handleSubmitProfileToRenderer(action: SendProfileToRenderer): any {
  const { userId } = action.payload

  yield call(waitForRendererInstance)
  const bff: IRealmAdapter = yield call(waitForRealm)
  const { profile, identity, isCurrentUser, lastSentProfileVersion } = (yield select(
    getInformationToSubmitProfileFromStore,
    userId
  )) as ReturnType<typeof getInformationToSubmitProfileFromStore>
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(bff)

  if (!profile || !profile.data) {
    return
  }

  if (isCurrentUser) {
    const forRenderer = profileToRendererFormat(profile.data, {
      address: identity?.address,
      baseUrl: fetchContentServerWithPrefix
    })
    forRenderer.hasConnectedWeb3 = identity?.hasConnectedWeb3 || false

    // TODO: this condition shouldn't be necessary. Unity fails with setThrew
    //       if LoadProfile is called rapidly because it cancels ongoing
    //       requests and those cancellations throw exceptions
    if (lastSentProfile && lastSentProfile?.version > forRenderer.version) {
      const event = 'Invalid user version' as const
      trackEvent(event, { address: userId, version: forRenderer.version })
    } else if (!deepEqual(lastSentProfile, forRenderer)) {
      lastSentProfile = forRenderer
      getUnityInterface().LoadProfile(forRenderer)
    }
  } else {
    // Add version check before submitting profile to renderer
    // Technically profile version might be `0` and make `!lastSentProfileVersion` always true
    if (typeof lastSentProfileVersion !== 'number' || lastSentProfileVersion < profile.data.version) {
      const forRenderer = profileToRendererFormat(profile.data, {
        baseUrl: fetchContentServerWithPrefix
      })
      getUnityInterface().AddUserProfileToCatalog(forRenderer)
      // Update catalog and last sent profile version
      yield put(addProfileToLastSentProfileVersionAndCatalog(userId, forRenderer.version))
    }

    // Send to Avatars scene
    // TODO: Consider refactor this so that it's distinguishable from a message received over the network
    // (`receivePeerUserData` is a handler for a comms message!!!)
    updateAvatarScenePeerData(profile.data, fetchContentServerWithPrefix)
  }
}

export function getInformationToSubmitProfileFromStore(state: RootState, userId: string) {
  const identity = getCurrentIdentity(state)
  const isCurrentUser = identity?.address.toLowerCase() === userId.toLowerCase()
  return {
    profile: getProfileFromStore(state, userId),
    identity,
    isCurrentUser,
    lastSentProfileVersion: getLastSentProfileVersion(state, userId)
  }
}
