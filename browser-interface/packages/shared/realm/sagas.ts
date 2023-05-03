import { createLogger } from 'lib/logger'
import { store } from 'shared/store/isolatedStore'
import { lastPlayerPosition } from 'shared/world/positionThings'
import {
  connectToComms,
  handleRealmDisconnection,
  HandleRealmDisconnection,
  HANDLE_REALM_DISCONNECTION,
  setRealmAdapter,
  SetRealmAdapterAction,
  SET_REALM_ADAPTER
} from './actions'
import { setRoomConnection, SET_COMMS_ISLAND } from '../comms/actions'
import { IRealmAdapter } from './types'
import { call, delay, fork, put, race, select, take, takeEvery } from 'redux-saga/effects'
import { DEPLOY_PROFILE_SUCCESS } from 'shared/profiles/actions'
import { getRealmAdapter } from './selectors'
import { getCurrentIdentity } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { FATAL_ERROR } from 'shared/loading/types'
import { BEFORE_UNLOAD } from 'shared/meta/actions'
import { notifyStatusThroughChat } from 'shared/chat'
import { realmToConnectionString } from './resolver'
import { hookConnectToFixedAdaptersIfNecessary } from './logic'

const logger = createLogger('realm')

export function* bffSaga() {
  yield fork(handleNewBFF)
  yield fork(handleHeartBeat)
  yield takeEvery(HANDLE_REALM_DISCONNECTION, handleBffDisconnectionSaga)

  yield takeEvery(FATAL_ERROR, function* () {
    yield put(setRealmAdapter(undefined))
  })

  yield takeEvery(BEFORE_UNLOAD, function* () {
    yield put(setRealmAdapter(undefined))
  })
}

/**
 * This function binds the given IBff to the kernel and returns the "unbind"
 * function in charge of disconnecting it from kernel.
 */
async function bindHandlersToAdapter(realm: IRealmAdapter, _address: string): Promise<() => Promise<void>> {
  realm.events.on('DISCONNECTION', () => {
    store.dispatch(handleRealmDisconnection(realm))
  })

  realm.events.on('setIsland', (message) => {
    logger.log('Island message', message)
    store.dispatch(connectToComms(message))
  })

  hookConnectToFixedAdaptersIfNecessary(realm)
  const realmName = realm.about.configurations?.realmName || realmToConnectionString(realm)
  notifyStatusThroughChat(`Welcome to realm ${realmName}!`)

  return async function unbind(): Promise<void> {
    logger.info('Unbinding adapter', realm)

    realm.events.off('DISCONNECTION')

    try {
      await realm.disconnect()
    } catch (err: any) {
      // this only needs to be logged. try {} catch is used because the function needs
      // to wait for the disconnection to continue with the saga.
      logger.error(err)
    }
  }
}

// this function is called from the handleHeartbeat saga
async function sendHeartBeat(realm: IRealmAdapter) {
  try {
    realm.sendHeartbeat(lastPlayerPosition)
  } catch (err: any) {
    await realm.disconnect(err)
  }
}

function* handleHeartBeat() {
  while (true) {
    yield race({
      SET_REALM_ADAPTER: take(SET_REALM_ADAPTER),
      SET_COMMS_ISLAND: take(SET_COMMS_ISLAND),
      DEPLOY_PROFILE_SUCCESS: take(DEPLOY_PROFILE_SUCCESS),
      delay: delay(2500)
    })

    const adapter: IRealmAdapter | undefined = yield select(getRealmAdapter)

    if (adapter) {
      yield call(sendHeartBeat, adapter)
    }
  }
}

// this saga reacts to changes in BFF context
function* handleNewBFF() {
  let unbind: () => Promise<void> = async function () {}

  yield takeEvery(SET_REALM_ADAPTER, function* (action: SetRealmAdapterAction) {
    if (unbind) {
      // disconnect previous bff
      unbind().catch(logger.error)
      unbind = async () => {}
    }

    // disconnect the world adapter with every new BFF
    yield put(setRoomConnection(undefined))

    if (action.payload) {
      const identity: ExplorerIdentity = yield select(getCurrentIdentity)
      // bind messages to this comms instance
      unbind = yield call(bindHandlersToAdapter, action.payload, identity?.address)
    }
  })
}

// this saga handles the suddenly disconnection of a IBff
function* handleBffDisconnectionSaga(action: HandleRealmDisconnection) {
  const adapter: IRealmAdapter = yield select(getRealmAdapter)

  if (adapter && adapter === action.payload.context) {
    // this also remove the context
    yield put(setRealmAdapter(undefined))
  }
}

globalThis.setAdapter = function setAdapter(connectionString: string) {
  store.dispatch(
    connectToComms({
      connStr: connectionString,
      islandId: '',
      peers: {}
    })
  )
}
