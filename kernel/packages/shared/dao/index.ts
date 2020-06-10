import defaultLogger from '../logger'
import future, { IFuture } from 'fp-future'
import { Layer, Realm, Candidate, RootDaoState, ServerConnectionStatus, PingResult } from './types'
import { RootState } from 'shared/store/rootTypes'
import { Store } from 'redux'
import {
  isRealmInitialized,
  getCatalystRealmCommsStatus,
  getRealm,
  getAllCatalystCandidates,
  areCandidatesFetched
} from './selectors'
import { fetchCatalystNodes } from 'shared/web3'
import { setCatalystRealm, setCatalystCandidates } from './actions'
import { deepEqual } from 'atomicHelpers/deepEqual'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { countParcelsCloseTo, ParcelArray } from 'shared/comms/interface/utils'
import { CatalystNode } from '../types'
import { zip } from './utils/zip'
import { realmToString } from './utils/realmToString'
const qs: any = require('query-string')

const v = 50
const score = ({ usersCount, maxUsers = 50 }: Layer) => {
  if (usersCount === 0) {
    return -v
  }
  if (usersCount >= maxUsers) {
    // We prefer empty layers to full layers
    return -10 * v
  }

  const phase = -Math.PI / 1.8

  const period = Math.PI / (0.67 * (maxUsers ? maxUsers : 50))

  return v + v * Math.cos(phase + period * usersCount)
}

export function ping(url: string, timeoutMs: number = 5000): Promise<PingResult> {
  const result = future<PingResult>()

  new Promise(() => {
    const http = new XMLHttpRequest()

    let started: Date

    http.timeout = timeoutMs

    http.onreadystatechange = () => {
      if (http.readyState === XMLHttpRequest.OPENED) {
        started = new Date()
      }
      if (http.readyState === XMLHttpRequest.DONE) {
        try {
          const ended = new Date().getTime()
          if (http.status !== 200) {
            result.resolve({
              status: ServerConnectionStatus.UNREACHABLE
            })
          } else {
            result.resolve({
              status: ServerConnectionStatus.OK,
              elapsed: ended - started.getTime(),
              result: JSON.parse(http.responseText)
            })
          }
        } catch (e) {
          defaultLogger.error('Error fetching status of Catalyst server', e)
          result.resolve({})
        }
      }
    }

    http.open('GET', url, true)

    try {
      http.send(null)
    } catch (exception) {
      result.resolve({
        status: ServerConnectionStatus.UNREACHABLE
      })
    }
  }).catch(defaultLogger.error)

  return result
}

export async function fecthCatalystRealms(): Promise<Candidate[]> {
  const nodes: CatalystNode[] = await fetchCatalystNodes()
  if (nodes.length === 0) {
    throw new Error('no nodes are available in the DAO for the current network')
  }

  return fetchCatalystStatuses(nodes)
}

export function commsStatusUrl(domain: string, includeLayers: boolean = false) {
  let url = `${domain}/comms/status`
  if (includeLayers) {
    url += `?includeLayers=true`
  }
  return url
}

export async function fetchCatalystStatuses(nodes: { domain: string }[]) {
  const results: PingResult[] = await Promise.all(nodes.map(node => ping(commsStatusUrl(node.domain, true))))

  return zip(nodes, results).reduce(
    (union: Candidate[], [{ domain }, { elapsed, result, status }]: [CatalystNode, PingResult]) =>
      status === ServerConnectionStatus.OK
        ? union.concat(
            result!.layers.map(layer => ({
              catalystName: result!.name,
              domain,
              status,
              elapsed: elapsed!,
              layer,
              score: score(layer),
              lighthouseVersion: result!.version
            }))
          )
        : union,
    new Array<Candidate>()
  )
}

export function pickCatalystRealm(candidates: Candidate[]): Realm {
  const usersByDomain: Record<string, number> = {}

  candidates.forEach(it => {
    if (!usersByDomain[it.domain]) {
      usersByDomain[it.domain] = 0
    }

    usersByDomain[it.domain] += it.layer.usersCount
  })

  const sorted = candidates
    .filter(it => it.status === ServerConnectionStatus.OK && it.layer.usersCount < it.layer.maxUsers)
    .sort((c1, c2) => {
      const elapsedDiff = c1.elapsed - c2.elapsed
      const usersDiff = usersByDomain[c1.domain] - usersByDomain[c2.domain]
      const scoreDiff = c2.score - c1.score

      return Math.abs(elapsedDiff) > 1500
        ? elapsedDiff // If the latency difference is greater than 1500, we consider that as the main factor
        : scoreDiff !== 0
        ? scoreDiff // If there's score difference, we consider that
        : usersDiff !== 0
        ? usersDiff // If the score is the same (as when they are empty)
        : elapsedDiff // If the candidates have the same score by users, we consider the latency again
    })

  if (sorted.length === 0 && candidates.length > 0) {
    throw new Error('No available realm found!')
  }

  return candidateToRealm(sorted[0])
}

export function candidatesFetched(): IFuture<void> {
  const result: IFuture<void> = future()

  const store: Store<RootState> = (window as any)['globalStore']

  const fetched = areCandidatesFetched(store.getState())
  if (fetched) {
    result.resolve()
    return result
  }

  new Promise(resolve => {
    const unsubscribe = store.subscribe(() => {
      const fetched = areCandidatesFetched(store.getState())
      if (fetched) {
        unsubscribe()
        return resolve()
      }
    })
  })
    .then(() => result.resolve())
    .catch(e => result.reject(e))

  return result
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

function candidateToRealm(candidate: Candidate) {
  return {
    catalystName: candidate.catalystName,
    domain: candidate.domain,
    layer: candidate.layer.name,
    lighthouseVersion: candidate.lighthouseVersion
  }
}

function realmFor(name: string, layer: string, candidates: Candidate[]): Realm | undefined {
  const candidate = candidates.find(it => it.catalystName === name && it.layer.name === layer)
  return candidate ? candidateToRealm(candidate) : undefined
}

export function changeRealm(realmString: string) {
  const store: Store<RootState> = (window as any)['globalStore']

  const candidates = getAllCatalystCandidates(store.getState())

  const realm = getRealmFromString(realmString, candidates)

  if (realm) {
    store.dispatch(setCatalystRealm(realm))
  }

  return realm
}

export async function changeToCrowdedRealm(): Promise<[boolean, Realm]> {
  const store: Store<RootState> = (window as any)['globalStore']

  const candidates = await refreshCandidatesStatuses()

  const currentRealm = getRealm(store.getState())!

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

export async function refreshCandidatesStatuses() {
  const store: Store<RootState> = (window as any)['globalStore']

  const candidates = await fetchCatalystStatuses(Array.from(getCandidateDomains(store)).map(it => ({ domain: it })))

  store.dispatch(setCatalystCandidates(candidates))

  return candidates
}

function getCandidateDomains(store: Store<RootDaoState>): Set<string> {
  return new Set(getAllCatalystCandidates(store.getState()).map(it => it.domain))
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
