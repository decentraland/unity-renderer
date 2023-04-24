import mitt from 'mitt'
import { RealmConnectionEvents, IRealmAdapter } from '../types'
import { ExplorerIdentity } from 'shared/session/types'
import { legacyServices } from '../local-services/legacy'
import { Vector3 } from 'lib/math/Vector3'
import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'

export function localArchipelago(baseUrl: string, about: AboutResponse, _identity: ExplorerIdentity): IRealmAdapter {
  const events = mitt<RealmConnectionEvents>()

  return {
    about,
    baseUrl,
    events,
    services: legacyServices(baseUrl, about),
    sendHeartbeat: (_p: Vector3) => {},
    async disconnect(error?: Error) {
      events.emit('DISCONNECTION', { error })
    }
  }
}
