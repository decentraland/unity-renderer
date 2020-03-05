import { Realm } from '../types'

export function realmToString(realm: Realm) {
  return `${realm.catalystName}-${realm.layer}`
}
