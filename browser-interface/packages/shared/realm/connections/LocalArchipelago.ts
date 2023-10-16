import mitt from 'mitt'
import { RealmConnectionEvents, IRealmAdapter } from '../types'
import { ExplorerIdentity } from 'shared/session/types'
import { legacyServices } from '../local-services/legacy'
import { Vector3 } from 'lib/math/Vector3'
import { AboutResponse } from 'shared/protocol/decentraland/renderer/about.gen'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'

export function localArchipelago(baseUrl: string, about: AboutResponse, _identity: ExplorerIdentity): IRealmAdapter {
  const events = mitt<RealmConnectionEvents>()

  const state = store.getState()
  const network = getSelectedNetwork(state)

  return {
    about,
    baseUrl,
    events,
    services: legacyServices(network, baseUrl, about),
    sendHeartbeat: (_p: Vector3) => {},
    async disconnect(error?: Error) {
      events.emit('DISCONNECTION', { error })
    }
  }
}
