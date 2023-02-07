import type { Avatar } from '@dcl/schemas'
import type { ETHEREUM_NETWORK } from 'config'
import type { ExplorerIdentity } from 'shared/session/types'
import { apply, select } from 'redux-saga/effects'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { currentIdentity, currentNetwork } from 'shared/session/selectors'
import { localProfilesRepo } from './localProfilesRepo'

export function* fetchLocalProfile() {
  const network: ETHEREUM_NETWORK = yield select(currentNetwork)
  const identity: ExplorerIdentity = yield select(currentIdentity)
  const profile = (yield apply(localProfilesRepo, localProfilesRepo.get, [identity.address, network])) as Avatar | null
  if (profile && profile.userId === identity.address) {
    try {
      return ensureAvatarCompatibilityFormat(profile)
    } finally {
      return null
    }
  } else {
    return null
  }
}
