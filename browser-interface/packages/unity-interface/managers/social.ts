import { ethereumConfigurations } from 'config'
import { isAddress } from 'eth-connect'
import { defaultLogger } from 'lib/logger'
import { fetchENSOwner } from 'lib/web3/fetchENSOwner'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { updateUserData } from 'shared/friends/actions'
import { ensureFriendProfile } from 'shared/friends/ensureFriendProfile'
import { UpdateFriendshipAsPromise } from 'shared/friends/sagas'
import { getMatrixIdFromUser } from 'shared/friends/utils'
import { findProfileByName } from 'shared/profiles/selectors'
import { store } from 'shared/store/isolatedStore'
import { FriendshipAction, FriendshipUpdateStatusMessage } from 'shared/types'
import { getUnityInstance } from 'unity-interface/IUnityInterface'

export async function handleUpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
  try {
    let { userId } = message
    let found = false
    const state = store.getState()

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    // @TODO! @deprecated - With the new friend request flow, the only action that will be triggered by this message is FriendshipAction.DELETED.
    if (message.action === FriendshipAction.REQUESTED_TO) {
      const avatar = await ensureFriendProfile(userId)

      if (isAddress(userId)) {
        found = avatar.hasConnectedWeb3 || false
      } else {
        const profileByName = findProfileByName(state, userId)
        if (profileByName) {
          userId = profileByName.userId
          found = true
        }
      }
    }

    if (!found) {
      // if user profile was not found on server -> no connected web3, check if it's a claimed name
      const net = getSelectedNetwork(state)
      const address = await fetchENSOwner(ethereumConfigurations[net].names, userId)
      if (address) {
        // if an address was found for the name -> set as user id & add that instead
        userId = address
        found = true
      }
    }

    // @TODO! @deprecated - With the new friend request flow, the only action that will be triggered by this message is FriendshipAction.DELETED.
    if (message.action === FriendshipAction.REQUESTED_TO && !found) {
      // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
      getUnityInstance().FriendNotFound(userId)
      return
    }

    store.dispatch(updateUserData(userId.toLowerCase(), getMatrixIdFromUser(userId)))
    await UpdateFriendshipAsPromise(message.action, userId.toLowerCase(), false)
  } catch (error) {
    const message = 'Failed while processing updating friendship status'
    defaultLogger.error(message, error)

    trackEvent('error', {
      context: 'kernel#saga',
      message: message,
      stack: '' + error
    })
  }
}
