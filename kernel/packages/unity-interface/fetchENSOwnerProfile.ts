import { isAddress } from 'eth-connect'
import { ethereumConfigurations } from 'config'
import { fetchENSOwnersContains, getAppNetwork } from 'shared/web3'
import { Profile, ProfileType } from 'shared/profiles/types'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'

export function fetchENSOwnerProfile(name: string, maxResults: number = 1): Promise<Profile[]> {
  let userIds: Promise<string[]>

  if (isAddress(name)) {
    userIds = Promise.resolve([name])
  } else {
    userIds = getAppNetwork().then((net) => fetchENSOwnersContains(ethereumConfigurations[net].names, name, maxResults))
  }

  return userIds.then((userIds) =>
    Promise.all(userIds.map((userId) => ProfileAsPromise(userId, undefined, ProfileType.DEPLOYED)))
  )
}
