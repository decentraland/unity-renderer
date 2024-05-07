import type { Avatar } from '@dcl/schemas'
import type { ETHEREUM_NETWORK } from 'config'
import type { ExplorerIdentity } from 'shared/session/types'
import {apply, put, select} from 'redux-saga/effects'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { localProfilesRepo } from './localProfilesRepo'
import { getCurrentIdentity, getCurrentNetwork } from 'shared/session/selectors'
import defaultLogger from 'lib/logger'
import {profileSuccess} from "../../actions";

export function* fetchLocalProfile(submit: boolean = false) {
  const network: ETHEREUM_NETWORK = yield select(getCurrentNetwork)
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const profile = (yield apply(localProfilesRepo, localProfilesRepo.get, [identity.address, network])) as Avatar | null
  if (profile && profile.userId === identity.address) {
    try {
      const avatar = ensureAvatarCompatibilityFormat(profile);
      if (submit) {
        yield put(profileSuccess(avatar))
      }
      return avatar
    } catch (error) {
      defaultLogger.log(`Invalid profile stored: ${JSON.stringify(profile, null, 2)}`)
      return null
    }
  } else {
    return null
  }
}
