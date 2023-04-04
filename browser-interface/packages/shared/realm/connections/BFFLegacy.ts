import mitt from 'mitt'
import { RealmConnectionEvents, BffServices, IRealmAdapter } from '../types'
import { ExplorerIdentity } from 'shared/session/types'
import { localCommsService } from '../local-services/comms'
import { legacyServices } from '../local-services/legacy'
import { AboutResponse } from 'shared/protocol/decentraland/bff/http_endpoints.gen'

export function localBff(baseUrl: string, about: AboutResponse, _identity: ExplorerIdentity): IRealmAdapter {
  const events = mitt<RealmConnectionEvents>()

  const services: BffServices = {
    comms: localCommsService(),
    legacy: legacyServices(baseUrl, about)
  }

  return {
    about,
    baseUrl,
    events,
    services,
    async disconnect(error?: Error) {
      events.emit('DISCONNECTION', { error })
    }
  }
}
