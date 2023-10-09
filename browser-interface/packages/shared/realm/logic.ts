import { IRealmAdapter } from './types'

// This logic runs for all IRealm. It emits a synthetic setIsland events in cases where archipelago doesn't exist
export function hookConnectToFixedAdaptersIfNecessary(realm: IRealmAdapter) {
  let connStr: string | undefined = undefined

  if (realm.about.comms?.fixedAdapter) {
    connStr = realm.about.comms?.fixedAdapter as string
  } else if (realm.about.comms?.adapter && realm.about.comms?.adapter.startsWith('fixed-adapter')) {
    const adapter = realm.about.comms?.adapter
    const ix = adapter.indexOf(':')
    connStr = adapter.substring(ix + 1)
  } else if (realm.about.comms?.protocol === 'v3' && !realm.about.comms?.healthy) {
    connStr = `offline`
  }

  if (connStr) {
    console.log('Will connect to ', connStr)
    setTimeout(() => {
      // send the island_changed message
      realm.events.emit('setIsland', {
        connStr: connStr!,
        islandId: '',
        peers: {}
      })
    }, 100)
  }
}
