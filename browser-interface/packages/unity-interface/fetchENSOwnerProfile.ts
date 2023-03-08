import { isAddress } from 'eth-connect'
import { ethereumConfigurations } from 'config'
import { getEthereumNetworkFromProvider } from 'lib/web3/getEthereumNetworkFromProvider'
import type { Avatar } from '@dcl/schemas'
import { retrieveProfile } from 'shared/profiles/retrieveProfile'
import { fetchENSOwnersContains } from 'lib/web3/fetchENSOwnersContains'

export async function fetchENSOwnerProfile(name: string, maxResults: number = 1): Promise<Avatar[]> {
  let userIds: string[]

  if (isAddress(name)) {
    userIds = [name]
  } else {
    const net = await getEthereumNetworkFromProvider()
    userIds = await fetchENSOwnersContains(ethereumConfigurations[net].names, name, maxResults)
  }
  return Promise.all(userIds.map((userId) => retrieveProfile(userId, undefined)))
}
