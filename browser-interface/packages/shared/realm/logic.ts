import { IRealmAdapter } from './types'

// This logic runs for all IBff. It emits a synthetic setIsland events in cases where archipelago doesn't exist
export function hookConnectToFixedAdaptersIfNecessary(bff: IRealmAdapter) {
  let connStr: string | undefined = undefined

  if (bff.about.comms?.fixedAdapter) {
    connStr = bff.about.comms?.fixedAdapter
  } else if (bff.about.comms?.protocol === 'v2') {
    connStr = `lighthouse:${bff.baseUrl}/comms`
  } else if (bff.about.comms?.protocol === 'v3' && !bff.about.bff?.healthy) {
    connStr = `offline`
  }

  if (connStr) {
    console.log('Will connect to ', connStr)
    setTimeout(() => {
      // send the island_changed message
      bff.events.emit('setIsland', {
        connStr: connStr!,
        islandId: '',
        peers: {}
      })
    }, 100)
  }
}
