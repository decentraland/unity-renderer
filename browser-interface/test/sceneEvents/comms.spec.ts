import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'
import mitt from 'mitt'
import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { toEnvironmentRealmType } from 'shared/apis/host/EnvironmentAPI'
import { setRealmAdapter } from 'shared/realm/actions'
import { legacyServices } from 'shared/realm/local-services/legacy'
import { realmToConnectionString } from 'shared/realm/resolver'
import { IRealmAdapter } from 'shared/realm/types'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { reducers } from 'shared/store/rootReducer'
import { setCommsIsland, setRoomConnection } from '../../packages/shared/comms/actions'
import { getCommsIsland } from '../../packages/shared/comms/selectors'
import { saveProfileDelta } from '../../packages/shared/profiles/actions'
import { sceneEventsSaga, updateLocation } from '../../packages/shared/sceneEvents/sagas'
import { allScenesEvent } from '../../packages/shared/world/parcelSceneManager'
import { localCommsService } from '../../packages/shared/realm/local-services/comms'
import { getRealmAdapter } from 'shared/realm/selectors'
import { disconnectRoom, handleNewCommsContext } from 'shared/comms/sagas'
import { bindHandlersToCommsContext } from 'shared/comms/handlers'
import { commsEstablished } from 'shared/loading/types'
import { RoomConnection } from 'shared/comms/interface'
import { expect } from 'chai'

const about: AboutResponse = {
  comms: { healthy: false, protocol: 'v2' },
  configurations: {
    scenesUrn: [],
    globalScenesUrn: [],
    networkId: 1
  },
  content: {
    healthy: false,
    publicUrl: 'https://content'
  },
  healthy: true,
  lambdas: {
    healthy: false,
    publicUrl: 'https://lambdas'
  },
  acceptingUsers: true
}

const realmAdapter: IRealmAdapter = {
  about,
  baseUrl: 'https://realm',
  events: mitt(),
  async disconnect() {},
  services: {
    legacy: legacyServices('https://realm', about),
    comms: localCommsService()
  }
}

describe('when the realm change: SET_WORLD_CONTEXT', () => {
  it('sanity toEnvironmentRealmType', () => {
    expect(toEnvironmentRealmType(realmAdapter, 'test')).to.deep.eq({
      protocol: 'v2',
      // domain explicitly expects the URL with the protocol
      domain: 'https://realm',
      layer: 'test',
      room: 'test',
      serverName: 'realm',
      displayName: 'realm'
    })
  })

  it('should call allScene events with empty string island', () => {
    const action = setRealmAdapter(realmAdapter)
    const island = ''
    return expectSaga(sceneEventsSaga)
      .provide([
        [select(getRealmAdapter), realmAdapter],
        [select(getCommsIsland), island]
      ])
      .dispatch(action)
      .call(allScenesEvent, { eventType: 'onRealmChanged', payload: toEnvironmentRealmType(realmAdapter, island) })
      .call(updateLocation, realmToConnectionString(realmAdapter), island)
      .run()
  })

  it('should call allScene events with null island', () => {
    const action = setRealmAdapter(realmAdapter)
    const island = undefined
    return expectSaga(sceneEventsSaga)
      .provide([
        [select(getRealmAdapter), realmAdapter],
        [select(getCommsIsland), island]
      ])
      .dispatch(action)
      .call(allScenesEvent, { eventType: 'onRealmChanged', payload: toEnvironmentRealmType(realmAdapter, island) })
      .call(updateLocation, realmToConnectionString(realmAdapter), island)
      .run()
  })

  it('should call allScene events fn with the specified realm & island', () => {
    const island = 'casla-island'
    const action = setRealmAdapter(realmAdapter)

    return expectSaga(sceneEventsSaga)
      .provide([
        [select(getRealmAdapter), realmAdapter],
        [select(getCommsIsland), island]
      ])
      .dispatch(action)
      .call(allScenesEvent, { eventType: 'onRealmChanged', payload: toEnvironmentRealmType(realmAdapter, island) })
      .call(updateLocation, realmToConnectionString(realmAdapter), island)
      .run()
  })
})

describe('Comms adapter', () => {
  it('setting adapter binds comms context to events and emits commsEstablished', () => {
    const action = setRoomConnection({} as any)

    return expectSaga(handleNewCommsContext)
      .withReducer(reducers)
      .provide([[call(bindHandlersToCommsContext, action.payload!), null]])
      .dispatch(action)
      .call(bindHandlersToCommsContext, action.payload)
      .put(commsEstablished())
      .run()
  })

  it('setting new adapter disconnects old adapter', async () => {
    const oldAdapter: RoomConnection = {} as any
    const actionOld = setRoomConnection(oldAdapter as any)
    const action = setRoomConnection({} as any)

    await expectSaga(handleNewCommsContext)
      .withReducer(reducers)
      .provide([
        [call(bindHandlersToCommsContext, action.payload!), null],
        [call(disconnectRoom, oldAdapter), null]
      ])
      .dispatch(actionOld)
      .dispatch(action)
      .call(bindHandlersToCommsContext, action.payload)
      .put(commsEstablished())
      .call(disconnectRoom, oldAdapter)
      .run()
  })
})

describe('when the island change: SET_COMMS_ISLAND', () => {
  it('should NOT call allScene events since the realm is null', () => {
    const island = 'casla-island'
    const action = setCommsIsland(island)
    return expectSaga(sceneEventsSaga)
      .withReducer(reducers)
      .provide([[select(getRealmAdapter), null]])
      .dispatch(action)
      .not.call.fn(allScenesEvent)
      .call(updateLocation, undefined, island)
      .run()
  })

  it('should call allScene events fn with the specified realm & island', () => {
    const island = 'casla-island'
    const action = setCommsIsland(island)

    return expectSaga(sceneEventsSaga)
      .provide([[select(getRealmAdapter), realmAdapter]])
      .withReducer(reducers)
      .dispatch(action)
      .call(allScenesEvent, { eventType: 'onRealmChanged', payload: toEnvironmentRealmType(realmAdapter, island) })
      .call(updateLocation, realmToConnectionString(realmAdapter), island)
      .run()
  })
})

describe('when the profile updates successfully: SAVE_PROFILE_SUCCESS', () => {
  it('should call allScene events with profileChanged using information from currentUserProfile', () => {
    const userId = 'user-id'
    const action = saveProfileDelta({ userId })
    const payload = {
      ethAddress: 'eth-address',
      version: 8
    }
    return expectSaga(sceneEventsSaga)
      .provide([[select(getCurrentUserProfile), payload]])
      .dispatch(action)
      .call(allScenesEvent, { eventType: 'profileChanged', payload })
      .run()
  })
})
