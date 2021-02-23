import { userAuthentified, getIdentity } from 'shared/session'
import { getServerConfigurations } from '../../config'
import { Authenticator } from 'dcl-crypto'
import { ClientResponse, QuestsClient } from 'dcl-quests-client'

export async function questsClient() {
  await userAuthentified()
  const servers = getServerConfigurations()
  return new QuestsClient({
    baseUrl: servers.questsUrl,
    // tslint:disable-next-line:no-unnecessary-type-assertion There seems to be a bug with tslint here
    authChainProvider: (payload) => Authenticator.signPayload(getIdentity()!, payload)
  })
}

export async function questsRequest<T>(request: (client: QuestsClient) => Promise<ClientResponse<T>>) {
  const client = await questsClient()
  return request(client)
}
