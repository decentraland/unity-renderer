import { getServerConfigurations } from 'config'
import { Authenticator } from '@dcl/crypto'
import { ClientResponse, QuestsClient } from 'dcl-quests-client'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'

export async function questsClient() {
  const { identity } = await onLoginCompleted()
  const net = getSelectedNetwork(store.getState())
  const servers = getServerConfigurations(net)
  return new QuestsClient({
    baseUrl: servers.questsUrl,
    authChainProvider: (payload) => Authenticator.signPayload(identity!, payload) as any
  })
}

export async function questsRequest<T>(
  request: (client: QuestsClient) => Promise<ClientResponse<T>>
): Promise<ClientResponse<T>> {
  try {
    const client = await questsClient()
    return await request(client)
  } catch (e: any) {
    return { ok: false, status: 0, body: { status: 'unknown error', message: e.message } }
  }
}
