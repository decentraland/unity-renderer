import { put, takeEvery, select, call, takeLatest, fork, take, race, delay, apply } from 'redux-saga/effects'

import { commsEstablished, establishingComms, FATAL_ERROR } from 'shared/loading/types'
import { commsLogger } from './context'
import { getCommsRoom } from './selectors'
import { BEFORE_UNLOAD } from 'shared/actions'
import {
  HandleRoomDisconnection,
  HANDLE_ROOM_DISCONNECTION,
  setCommsIsland,
  setRoomConnection,
  SetRoomConnectionAction,
  SET_COMMS_ISLAND,
  SET_ROOM_CONNECTION
} from './actions'
import { notifyStatusThroughChat } from 'shared/chat'
import { bindHandlersToCommsContext, createSendMyProfileOverCommsChannel, sendPing } from './handlers'
import { Rfc4RoomConnection } from './logic/rfc-4-room-connection'
import { DEPLOY_PROFILE_SUCCESS, SendProfileToRenderer, SEND_PROFILE_TO_RENDERER } from 'shared/profiles/actions'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { Avatar, IPFSv2, Snapshots } from '@dcl/schemas'
import { commConfigurations, COMMS_GRAPH, DEBUG_COMMS, genericAvatarSnapshots, PREFERED_ISLAND } from 'config'
import { isURL } from 'atomicHelpers/isURL'
import { getAllPeers, processAvatarVisibility } from './peers'
import { getFatalError } from 'shared/loading/selectors'
import { EventChannel } from 'redux-saga'
import { ExplorerIdentity } from 'shared/session/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import * as rfc4 from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'
import { selectAndReconnectRealm } from 'shared/dao/sagas'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'
import { getCommsConfig, getFeatureFlagEnabled, getMaxVisiblePeers } from 'shared/meta/selectors'
import { getCurrentIdentity } from 'shared/session/selectors'
import { OfflineAdapter } from './adapters/OfflineAdapter'
import { WebSocketAdapter } from './adapters/WebSocketAdapter'
import { LivekitAdapter } from './adapters/LivekitAdapter'
import { SimulationRoom } from './adapters/SimulatorAdapter'
import { IRealmAdapter } from 'shared/realm/types'
import { CommsConfig } from 'shared/meta/types'
import { Authenticator } from '@dcl/crypto'
import { LighthouseConnectionConfig, LighthouseWorldInstanceConnection } from './v2/LighthouseWorldInstanceConnection'
import { lastPlayerPositionReport, positionObservable, PositionReport } from 'shared/world/positionThings'
import { store } from 'shared/store/isolatedStore'
import { ConnectToCommsAction, CONNECT_TO_COMMS, setRealmAdapter, SET_REALM_ADAPTER } from 'shared/realm/actions'
import { getRealmAdapter, getFetchContentUrlPrefixFromRealmAdapter, waitForRealmAdapter } from 'shared/realm/selectors'
import { positionReportToCommsPositionRfc4 } from './interface/utils'
import { deepEqual } from 'atomicHelpers/deepEqual'
import { incrementCounter } from 'shared/occurences'
import { RoomConnection } from './interface'
import {
  debugCommsGraph,
  measurePingTime,
  measurePingTimePercentages,
  overrideCommsProtocol
} from 'shared/session/getPerformanceInfo'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { NotificationType } from 'shared/types'
import { trackEvent } from 'shared/analytics'
import { getCatalystCandidates } from 'shared/dao/selectors'
import { setCatalystCandidates } from 'shared/dao/actions'
import { signedFetch } from 'atomicHelpers/signedFetch'

const TIME_BETWEEN_PROFILE_RESPONSES = 1000
// this interval should be fast because this will be the delay other people around
// you will experience to fully show your avatar. i.e. if we set it to 10sec, people
// in the genesis plaza will have to wait up to 10 seconds (if already connected) to
// see you. if they missed the report by one second, then they will wait 19seconds to
// see you.
const INTERVAL_ANNOUNCE_PROFILE = 2_000 // 2 seconds

export function* commsSaga() {
  yield takeLatest(HANDLE_ROOM_DISCONNECTION, handleRoomDisconnectionSaga)

  yield takeEvery(FATAL_ERROR, function* () {
    // set null context on fatal error. this will bring down comms.
    yield put(setRoomConnection(undefined))
  })

  yield takeEvery(BEFORE_UNLOAD, function* () {
    // this would disconnect the comms context
    yield put(setRoomConnection(undefined))
  })

  yield takeEvery(CONNECT_TO_COMMS, handleConnectToComms)

  yield fork(handleNewCommsContext)

  // respond to profile requests over comms
  yield fork(respondCommsProfileRequests)

  yield fork(handleAnnounceProfile)
  yield fork(initAvatarVisibilityProcess)
  yield fork(handleCommsReconnectionInterval)
  yield fork(pingerProcess)
  yield fork(reportPositionSaga)

  if (COMMS_GRAPH) {
    yield call(debugCommsGraph)
  }
}

/**
 * This saga reports the position of our player:
 * - once every one second
 * - or when a new comms context is set
 * - and every time a positionObservable is called with a maximum of 10Hz
 */
function* reportPositionSaga() {
  let latestRoom: RoomConnection | undefined = undefined

  let lastNetworkUpdatePosition = 0
  let lastPositionSent: rfc4.Position

  const observer = positionObservable.add((obj: Readonly<PositionReport>) => {
    if (latestRoom) {
      const newPosition = positionReportToCommsPositionRfc4(obj)
      const now = Date.now()
      const elapsed = now - lastNetworkUpdatePosition

      // We only send the same position message as a ping if we have not sent positions in the last 1 second
      if (elapsed < 1000) {
        if (deepEqual(newPosition, lastPositionSent)) {
          return
        }
      }

      // Otherwise we simply respect the 10Hz
      if (elapsed > 100) {
        lastPositionSent = newPosition
        lastNetworkUpdatePosition = now
        latestRoom.sendPositionMessage(newPosition).catch((e) => {
          incrementCounter('failed:sendPositionMessage')
          commsLogger.warn(`error while sending message `, e)
        })
      }
    }
  })

  while (true) {
    const reason = yield race({
      UNLOAD: take(BEFORE_UNLOAD),
      ERROR: take(FATAL_ERROR),
      timeout: delay(1000),
      setNewContext: take(SET_ROOM_CONNECTION)
    })

    if (reason.UNLOAD || reason.ERROR) break

    if (!latestRoom) lastNetworkUpdatePosition = 0
    latestRoom = yield select(getCommsRoom)

    if (latestRoom && lastPlayerPositionReport) {
      latestRoom
        .sendPositionMessage(positionReportToCommsPositionRfc4(lastPlayerPositionReport))
        .catch(commsLogger.error)
    }
  }

  positionObservable.remove(observer)
}

/**
 * This saga sends random pings to all peers if the conditions are met.
 */
function* pingerProcess() {
  yield call(waitForMetaConfigurationInitialization)

  const enabled: boolean = yield select(getFeatureFlagEnabled, 'ping_enabled')

  if (enabled) {
    while (true) {
      yield delay(15_000 + Math.random() * 60_000)

      const responses = new Map<string, number[]>()
      const expectedResponses = getAllPeers().size

      yield call(sendPing, (dt, address) => {
        const list = responses.get(address) || []
        responses.set(address, list)
        list.push(dt)
        measurePingTime(dt)
        if (list.length > 1) {
          incrementCounter('pong_duplicated_response_counter')
        }
      })

      yield delay(15_000)

      // measure the response ratio
      if (expectedResponses) {
        measurePingTimePercentages(Math.round((responses.size / expectedResponses) * 100))
      }

      incrementCounter('pong_expected_counter', expectedResponses)
      incrementCounter('pong_given_counter', responses.size)
    }
  }
}

/**
 * This saga handles the action to connect a specific comms
 * adapter.
 */
function* handleConnectToComms(action: ConnectToCommsAction) {
  try {
    const identity: ExplorerIdentity = yield select(getCurrentIdentity)

    yield put(setCommsIsland(action.payload.event.islandId))

    const adapter: RoomConnection = yield call(connectAdapter, action.payload.event.connStr, identity)

    globalThis.__DEBUG_ADAPTER = adapter

    yield put(establishingComms())
    yield apply(adapter, adapter.connect, [])
    yield put(setRoomConnection(adapter))
  } catch (error: any) {
    notifyStatusThroughChat('Error connecting to comms. Will try another realm')

    const realmAdapter: IRealmAdapter | undefined = yield select(getRealmAdapter)
    const candidates = yield select(getCatalystCandidates)
    for (const candidate of candidates) {
      if (candidate.domain === realmAdapter!.baseUrl) {
        candidate.lastConnectionAttempt = Date.now()
        break
      }
    }
    store.dispatch(setCatalystCandidates(candidates))

    yield put(setRealmAdapter(undefined))
    yield put(setRoomConnection(undefined))
  }
}

async function connectAdapter(connStr: string, identity: ExplorerIdentity): Promise<RoomConnection> {
  const ix = connStr.indexOf(':')
  const protocol = connStr.substring(0, ix)
  const url = connStr.substring(ix + 1)

  // TODO: move this to a saga
  overrideCommsProtocol(protocol)

  switch (protocol) {
    case 'signed-login': {
      // this communications protocol signals a "required handshake" to connect
      // to a server which requires a signature from part of the user in order
      // to authenticate them
      const result = await signedFetch(
        url,
        identity,
        { method: 'POST', responseBodyType: 'json' },
        {
          intent: 'dcl:explorer:comms-handshake',
          signer: 'dcl:explorer',
          isGuest: !identity.hasConnectedWeb3
        }
      )

      const response: SignedLoginResult = result.json
      if (!result.ok || typeof response !== 'object') {
        throw new Error(
          'There was an error acquiring the communications connection. Decentraland will try to connect to another realm'
        )
      }

      type SignedLoginResult = {
        fixedAdapter?: string
        message?: string
      }

      if (typeof response.fixedAdapter === 'string' && !response.fixedAdapter.startsWith('signed-login:')) {
        return connectAdapter(response.fixedAdapter, identity)
      }

      if (typeof response.message === 'string') {
        throw new Error(`There was an error acquiring the communications connection: ${response.message}`)
      }

      trackEvent('error', {
        message: 'Error in signed-login response: ' + JSON.stringify(response),
        context: 'comms',
        stack: 'connectAdapter'
      })

      throw new Error(`An unknown error was detected while trying to connect to the selected realm.`)
    }
    case 'offline': {
      return new Rfc4RoomConnection(new OfflineAdapter())
    }
    case 'ws-room': {
      const finalUrl = !url.startsWith('ws:') && !url.startsWith('wss:') ? 'wss://' + url : url

      return new Rfc4RoomConnection(new WebSocketAdapter(finalUrl, identity))
    }
    case 'simulator': {
      return new SimulationRoom(url)
    }
    case 'livekit': {
      const theUrl = new URL(url)
      const token = theUrl.searchParams.get('access_token')
      if (!token) {
        throw new Error('No access token')
      }
      return new Rfc4RoomConnection(
        new LivekitAdapter({
          logger: commsLogger,
          url: theUrl.origin + theUrl.pathname,
          token
        })
      )
    }
    case 'lighthouse': {
      return createLighthouseConnection(url, identity)
    }
  }
  throw new Error(`A communications adapter could not be created for protocol=${protocol}`)
}

function createLighthouseConnection(url: string, identity: ExplorerIdentity) {
  const commsConfig: CommsConfig = getCommsConfig(store.getState())
  const peerConfig: LighthouseConnectionConfig = {
    connectionConfig: {
      iceServers: commConfigurations.defaultIceServers
    },
    authHandler: async (msg: string) => {
      try {
        return Authenticator.signPayload(identity, msg)
      } catch (e) {
        commsLogger.info(`error while trying to sign message from lighthouse '${msg}'`)
      }
      // if any error occurs
      return getCurrentIdentity(store.getState())
    },
    logLevel: DEBUG_COMMS ? 'TRACE' : 'NONE',
    targetConnections: commsConfig.targetConnections ?? 4,
    maxConnections: commsConfig.maxConnections ?? 6,
    positionConfig: {
      selfPosition: () => {
        if (lastPlayerPositionReport) {
          const { x, y, z } = lastPlayerPositionReport.position
          return [x, y, z]
        }
      },
      maxConnectionDistance: 4,
      nearbyPeersDistance: 5,
      disconnectDistance: 6
    },
    preferedIslandId: PREFERED_ISLAND ?? ''
  }

  if (!commsConfig.relaySuspensionDisabled) {
    peerConfig.relaySuspensionConfig = {
      relaySuspensionInterval: commsConfig.relaySuspensionInterval ?? 750,
      relaySuspensionDuration: commsConfig.relaySuspensionDuration ?? 5000
    }
  }

  const lighthouse = new LighthouseWorldInstanceConnection(
    url,
    peerConfig,
    (status) => {
      commsLogger.log('Lighthouse status: ', status)
      switch (status.status) {
        case 'realm-full':
          disconnect(status.status, 'The realm is full, reconnecting')
          break
        case 'reconnection-error':
          disconnect(status.status, 'Reconnection comms error')
          break
        case 'id-taken':
          disconnect(status.status, 'A previous connection to the connection server is still active')
          break
        case 'error':
          disconnect(status.status, 'An error has ocurred in the communications server, reconnecting.')
          break
      }
    },
    identity
  )

  function disconnect(reason: string, message: string) {
    trackEvent('disconnect_lighthouse', { message, reason, url })

    getUnityInstance().ShowNotification({
      type: NotificationType.GENERIC,
      message: message,
      buttonMessage: 'OK',
      timer: 10
    })

    lighthouse
      .disconnect({ kicked: reason === 'id-taken', error: new Error(message) })
      .catch(commsLogger.error)
      .finally(() => {
        setTimeout(
          () => {
            store.dispatch(setRealmAdapter(undefined))
            store.dispatch(setRoomConnection(undefined))
          },
          reason === 'id-taken' ? 10000 : 300
        )
      })
  }

  lighthouse.onIslandChangedObservable.add(({ island }) => {
    store.dispatch(setCommsIsland(island))
  })

  return lighthouse
}

/**
 * This saga runs every 100ms and checks the visibility of all avatars, hiding
 * to the avatar scene the ones that are far away
 */
function* initAvatarVisibilityProcess() {
  yield call(waitForMetaConfigurationInitialization)
  const maxVisiblePeers = yield select(getMaxVisiblePeers)

  while (true) {
    const reason = yield race({
      delay: delay(100),
      unload: take(BEFORE_UNLOAD)
    })

    if (reason.unload) break

    const account: ExplorerIdentity | undefined = yield select(getCurrentIdentity)

    processAvatarVisibility(maxVisiblePeers, account?.address)
  }
}

/**
 * This handler sends profile responses over comms.
 */
function* respondCommsProfileRequests() {
  const chan: EventChannel<any> = yield call(createSendMyProfileOverCommsChannel)

  let lastMessage = 0
  while (true) {
    // wait for the next event of the channel
    yield take(chan)

    const context = (yield select(getCommsRoom)) as RoomConnection | undefined
    const profile: Avatar | null = yield select(getCurrentUserProfile)
    const realmAdapter: IRealmAdapter = yield call(waitForRealmAdapter)
    const contentServer: string = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)
    const identity: ExplorerIdentity | null = yield select(getCurrentIdentity)

    if (profile && context) {
      profile.hasConnectedWeb3 = identity?.hasConnectedWeb3 || profile.hasConnectedWeb3

      // naive throttling
      const now = Date.now()
      const elapsed = now - lastMessage
      if (elapsed < TIME_BETWEEN_PROFILE_RESPONSES) continue
      lastMessage = now

      const response: rfc4.ProfileResponse = {
        serializedProfile: JSON.stringify(stripSnapshots(profile)),
        baseUrl: contentServer
      }
      yield apply(context, context.sendProfileResponse, [response])
    }
  }
}

function stripSnapshots(profile: Avatar): Avatar {
  const newSnapshots: Record<string, string> = {}
  const currentSnapshots: Record<string, string> = profile.avatar.snapshots

  for (const snapshotKey of ['face256', 'body'] as const) {
    const snapshot = currentSnapshots[snapshotKey]
    const defaultValue = genericAvatarSnapshots[snapshotKey]
    const newValue =
      snapshot &&
      (snapshot.startsWith('/') || snapshot.startsWith('./') || isURL(snapshot) || IPFSv2.validate(snapshot))
        ? snapshot
        : null
    newSnapshots[snapshotKey] = newValue || defaultValue
  }

  return {
    ...profile,
    avatar: { ...profile.avatar, snapshots: newSnapshots as Snapshots },
    // THIS IS IMPORTANT, the blocked and muted sizes are too big for the network and are unnecesary
    blocked: [],
    muted: []
  }
}

/**
 * This saga handle reconnections of comms contexts.
 */
function* handleCommsReconnectionInterval() {
  while (true) {
    const reason: any = yield race({
      SET_WORLD_CONTEXT: take(SET_ROOM_CONNECTION),
      SET_REALM_ADAPTER: take(SET_REALM_ADAPTER),
      USER_AUTHENTIFIED: take(USER_AUTHENTIFIED),
      timeout: delay(1000)
    })

    const coomConnection: RoomConnection | undefined = yield select(getCommsRoom)
    const realmAdapter: IRealmAdapter | undefined = yield select(getRealmAdapter)
    const hasFatalError: string | undefined = yield select(getFatalError)
    const identity: ExplorerIdentity | undefined = yield select(getCurrentIdentity)

    const shouldReconnect = !coomConnection && !hasFatalError && identity?.address && !realmAdapter

    if (shouldReconnect) {
      // reconnect
      commsLogger.info('Trying to reconnect to a realm. reason:', Object.keys(reason)[0])
      yield call(selectAndReconnectRealm)
    }
  }
}

/**
 * This saga waits for one of the conditions that may trigger a
 * sendCurrentProfile and then does it.
 */
function* handleAnnounceProfile() {
  while (true) {
    // We notify the network of our profile's latest version when:
    const reason: { sendProfileToRenderer?: SendProfileToRenderer } = yield race({
      // A local profile is updated in the renderer
      sendProfileToRenderer: take(SEND_PROFILE_TO_RENDERER),
      // The profile got updated on a catalyst
      DEPLOY_PROFILE_SUCCESS: take(DEPLOY_PROFILE_SUCCESS),
      // The current user's island changed
      SET_COMMS_ISLAND: take(SET_COMMS_ISLAND),
      // The current user's realm/catalyst changed
      SET_WORLD_CONTEXT: take(SET_ROOM_CONNECTION),
      // Periodically just in case other users did not notice the current user
      timeout: delay(INTERVAL_ANNOUNCE_PROFILE)
    })

    const profile: Avatar | null = yield select(getCurrentUserProfile)

    // skip this process when there is no local profile
    if (!profile) continue

    if (reason.sendProfileToRenderer && profile.userId !== reason.sendProfileToRenderer.payload.userId) {
      // skip this process when sendProfileToRenderer is called for a different avatar
      continue
    }

    const roomConnection: RoomConnection | undefined = yield select(getCommsRoom)

    if (roomConnection) {
      roomConnection.sendProfileMessage({ profileVersion: profile.version }).catch(commsLogger.error)
    }
  }
}

// this saga reacts to changes in context and disconnects the old context
export function* handleNewCommsContext() {
  let oldRoomConnection: RoomConnection | undefined = undefined

  while (true) {
    const action: SetRoomConnectionAction = yield take(SET_ROOM_CONNECTION)
    const newRoomConnection = action.payload

    if (oldRoomConnection !== newRoomConnection) {
      if (newRoomConnection) {
        // bind messages to this comms instance
        yield call(bindHandlersToCommsContext, newRoomConnection)
        yield put(commsEstablished())
      }

      if (oldRoomConnection) {
        // disconnect previous context
        yield call(disconnectRoom, oldRoomConnection)
      }

      // lastly, replace the state of this saga
      oldRoomConnection = newRoomConnection
    }
  }
}

export async function disconnectRoom(context: RoomConnection) {
  try {
    await context.disconnect()
  } catch (err: any) {
    // this only needs to be logged. try {} catch is used because the function needs
    // to wait for the disconnection to continue with the saga.
    commsLogger.error(err)
  }
}

// this saga handles the suddenly disconnection of a CommsContext
function* handleRoomDisconnectionSaga(action: HandleRoomDisconnection) {
  const room: RoomConnection = yield select(getCommsRoom)

  if (room && room === action.payload.context) {
    // this also remove the context
    yield put(setRoomConnection(undefined))

    if (action.payload.context) {
      notifyStatusThroughChat(`Lost connection to realm`)
    }
  }
}
