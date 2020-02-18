import defaultLogger from '../logger'
import future from 'fp-future'
import { Layer, Realm, Candidate, CatalystLayers, RootDaoState } from './types'
import { RootState } from 'shared/store/rootTypes'
import { Store } from 'redux'
import { isRealmInitialized, getCatalystCandidates, getCatalystRealmCommsStatus, getRealm } from './selectors'
import { fetchCatalystNodes } from 'shared/web3'
import { setCatalystRealm, setCatalystCandidates } from './actions'
import { deepEqual } from 'atomicHelpers/deepEqual'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { countParcelsCloseTo, ParcelArray } from 'shared/comms/interface/utils'
const qs: any = require('query-string')

const zip = <T, U>(arr: Array<T>, ...arrs: Array<Array<U>>) => {
  return arr.map((val, i) => arrs.reduce((a, arr) => [...a, arr[i]], [val] as Array<any>)) as Array<[T, U]>
}

const v = 50
const score = ({ usersCount, maxUsers = 50 }: Layer) => {
  if (usersCount === 0) {
    return -v
  }
  if (usersCount >= maxUsers) {
    // We prefer empty layers to full layers
    return -10 * v
  }

  const p = 3 / (maxUsers ? maxUsers : 50)

  return v + v * Math.cos(p * (usersCount - 1))
}

function ping(url: string): Promise<{ success: boolean; elapsed?: number; result?: CatalystLayers }> {
  const result = future()

  new Promise(() => {
    const http = new XMLHttpRequest()

    let started: Date

    http.timeout = 5000

    http.onreadystatechange = () => {
      if (http.readyState === XMLHttpRequest.OPENED) {
        started = new Date()
      }
      if (http.readyState === XMLHttpRequest.DONE) {
        const ended = new Date().getTime()
        if (http.status >= 400) {
          result.resolve({
            success: false
          })
        } else {
          result.resolve({
            success: true,
            elapsed: ended - started.getTime(),
            result: JSON.parse(http.responseText) as Layer[]
          })
        }
      }
    }

    http.open('GET', url, true)

    try {
      http.send(null)
    } catch (exception) {
      result.resolve({
        success: false
      })
    }
  }).catch(defaultLogger.error)

  return result
}

export async function fecthCatalystRealms(): Promise<Candidate[]> {
  const nodes = await fetchCatalystNodes()
  if (nodes.length === 0) {
    throw new Error('no nodes are available in the DAO for the current network')
  }

  return fetchCatalystStatuses(nodes)
}

async function fetchCatalystStatuses(nodes: { domain: string }[]) {
  const results = await Promise.all(nodes.map(node => ping(`${node.domain}/comms/status?includeLayers=true`)))
  const successfulResults = results.filter($ => $.success)
  if (successfulResults.length === 0) {
    throw new Error('no node responded')
  }
  return zip(nodes, successfulResults).reduce(
    (
      union: Candidate[],
      [{ domain }, { elapsed, result, success }]: [
        {
          domain: string
        },
        {
          elapsed?: number
          success: boolean
          result?: CatalystLayers
        }
      ]
    ) =>
      success
        ? union.concat(
            result!.layers.map(layer => ({
              catalystName: result!.name,
              domain,
              elapsed: elapsed!,
              layer,
              score: score(layer)
            }))
          )
        : union,
    new Array<Candidate>()
  )
}

export function pickCatalystRealm(candidates: Candidate[]): Realm {
  const sorted = candidates
    .filter(it => it.layer.usersCount < it.layer.maxUsers)
    .sort((c1, c2) => {
      const diff = c2.score - c1.score
      return diff === 0 ? c1.elapsed - c2.elapsed : diff
    })

  if (sorted.length === 0 && candidates.length > 0) {
    throw new Error('No available realm found!')
  }

  return candidateToRealm(sorted[0])
}

export async function realmInitialized(): Promise<void> {
  const store: Store<RootState> = (window as any)['globalStore']

  const initialized = isRealmInitialized(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise(resolve => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isRealmInitialized(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export function getRealmFromString(realmString: string, candidates: Candidate[]) {
  const parts = realmString.split('-')
  if (parts.length === 2) {
    return realmFor(parts[0], parts[1], candidates)
  }
}

function realmToString(realm: Realm) {
  return `${realm.catalystName}-${realm.layer}`
}

function candidateToRealm(candidate: Candidate) {
  return { catalystName: candidate.catalystName, domain: candidate.domain, layer: candidate.layer.name }
}

function realmFor(name: string, layer: string, candidates: Candidate[]): Realm | undefined {
  const candidate = candidates.find(it => it.catalystName === name && it.layer.name === layer)
  return candidate ? candidateToRealm(candidate) : undefined
}

export function changeRealm(realmString: string) {
  const store: Store<RootState> = (window as any)['globalStore']

  const candidates = getCatalystCandidates(store.getState())

  const realm = getRealmFromString(realmString, candidates)

  if (realm) {
    store.dispatch(setCatalystRealm(realm))
  }

  return realm
}

export async function changeToCrowdedRealm(): Promise<[boolean, Realm]> {
  const store: Store<RootState> = (window as any)['globalStore']

  const candidates = await fetchCatalystStatuses(Array.from(getCandidateDomains(store)).map(it => ({ domain: it })))

  const currentRealm = getRealm(store.getState())!

  store.dispatch(setCatalystCandidates(candidates))

  const positionAsVector = worldToGrid(lastPlayerPosition)
  const currentPosition = [positionAsVector.x, positionAsVector.y] as ParcelArray

  type RealmPeople = { realm: Realm; closePeople: number }

  let crowdedRealm: RealmPeople = { realm: currentRealm, closePeople: 0 }

  candidates
    .filter(it => it.layer.usersParcels && it.layer.usersParcels.length > 0 && it.layer.usersCount < it.layer.maxUsers)
    .forEach(candidate => {
      if (candidate.layer.usersParcels) {
        let closePeople = countParcelsCloseTo(currentPosition, candidate.layer.usersParcels, 4)
        // If it is the realm of the player, we substract 1 to not count ourselves
        if (candidate.catalystName === currentRealm.catalystName && candidate.layer.name === currentRealm.layer) {
          closePeople -= 1
        }

        if (closePeople > crowdedRealm.closePeople) {
          crowdedRealm = {
            realm: candidateToRealm(candidate),
            closePeople
          }
        }
      }
    })

  if (!deepEqual(crowdedRealm.realm, currentRealm)) {
    store.dispatch(setCatalystRealm(crowdedRealm.realm))
    await catalystRealmConnected()
    return [true, crowdedRealm.realm]
  } else {
    return [false, currentRealm]
  }
}

function getCandidateDomains(store: Store<RootDaoState>): Set<string> {
  return new Set(getCatalystCandidates(store.getState()).map(it => it.domain))
}

export async function catalystRealmConnected(): Promise<void> {
  const store: Store<RootState> = (window as any)['globalStore']

  const status = getCatalystRealmCommsStatus(store.getState())

  if (status.status === 'connected') {
    return Promise.resolve()
  } else if (status.status === 'error' || status.status === 'realm-full') {
    return Promise.reject(status.status)
  }

  return new Promise((resolve, reject) => {
    const unsubscribe = store.subscribe(() => {
      const status = getCatalystRealmCommsStatus(store.getState())
      if (status.status === 'connected') {
        resolve()
        unsubscribe()
      } else if (status.status === 'error' || status.status === 'realm-full') {
        reject(status.status)
        unsubscribe()
      }
    })
  })
}

export function observeRealmChange(
  store: Store<RootDaoState>,
  onRealmChange: (previousRealm: Realm | undefined, currentRealm: Realm) => any
) {
  let currentRealm: Realm | undefined = getRealm(store.getState())
  store.subscribe(() => {
    const previousRealm = currentRealm
    currentRealm = getRealm(store.getState())
    if (currentRealm && !deepEqual(previousRealm, currentRealm)) {
      onRealmChange(previousRealm, currentRealm)
    }
  })
}

export function initializeUrlRealmObserver() {
  const store: Store<RootState> = (window as any)['globalStore']
  observeRealmChange(store, (previousRealm, currentRealm) => {
    const q = qs.parse(location.search)
    const realmString = realmToString(currentRealm)

    q.realm = realmString

    history.replaceState({ realm: realmString }, '', `?${qs.stringify(q)}`)
  })
}
