import { isAddress } from 'eth-connect'
import { ethereumConfigurations } from 'config'
import { fetchENSOwnersContains } from 'shared/web3'
import { getEthereumNetworkFromProvider } from "shared/getEthereumNetworkFromProvider"
import type { Avatar } from '@dcl/schemas'
import { retrieveProfile } from 'shared/profiles/retrieveProfile'

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
