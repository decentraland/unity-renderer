import { RpcClientPort } from '@dcl/rpc'
import defaultLogger from 'lib/logger'
import { call, put, select, take, takeEvery } from 'redux-saga/effects'
import { registerEmotesService } from 'renderer-protocol/services/emotesService'
import { registerFriendRequestRendererService } from 'renderer-protocol/services/friendRequestService'
import { createRpcTransportService } from 'renderer-protocol/services/transportService'
import { getAllowedContentServer } from 'shared/meta/selectors'
import { SET_REALM_ADAPTER } from 'shared/realm/actions'
import { getExploreRealmsService, getFetchContentServerFromRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { CurrentRealmInfoForRenderer } from 'shared/types'
import { getUnityInterface } from 'unity-interface/IUnityInterface'
import { registerRendererModules, REGISTER_RPC_PORT } from '../actions'
import { waitForRendererInstance } from '../sagas-helper'
import { getClientPort } from '../selectors'
import { RendererModules } from '../types'

/**
 * This saga sends the BFF configuration changes to the renderer upon every change
 */
export function* reportRealmChangeToRenderer() {
  yield call(waitForRendererInstance)
  yield takeEvery(REGISTER_RPC_PORT, handleRegisterRpcPort)

  while (true) {
    const realmAdapter: IRealmAdapter = yield call(waitForRealm)

    try {
      const configuredContentServer: string = getFetchContentServerFromRealmAdapter(realmAdapter)
      const contentServerUrl: string = yield select(getAllowedContentServer, configuredContentServer)
      const current = convertCurrentRealmType(realmAdapter, contentServerUrl)
      defaultLogger.info('UpdateRealmsInfo', current)
      getUnityInterface().UpdateRealmsInfo({ current })
      getUnityInterface().UpdateRealmAbout(realmAdapter.about)

      const realmsService = yield select(getExploreRealmsService)

      if (realmsService) {
        yield call(fetchAndReportRealmsInfo, realmsService)
      }

      // wait for the next context
    } catch (err: any) {
      defaultLogger.error(err)
    }

    yield take(SET_REALM_ADAPTER)
  }
}

/**
 * On every new port we register the services for it and starts the inverse RPC
 */
function* handleRegisterRpcPort() {
  const port: RpcClientPort | undefined = yield select(getClientPort)

  if (!port) {
    return
  }

  if (createRpcTransportService(port)) {
    const modules: RendererModules = {
      emotes: registerEmotesService(port),
      friendRequest: registerFriendRequestRendererService(port)
    }

    yield put(registerRendererModules(modules))
  }
}

async function fetchAndReportRealmsInfo(url: string) {
  try {
    const response = await fetch(url)
    if (response.ok) {
      const value = await response.json()
      getUnityInterface().UpdateRealmsInfo({ realms: value })
    }
  } catch (e) {
    defaultLogger.error(url, e)
  }
}

function convertCurrentRealmType(realmAdapter: IRealmAdapter, contentServerUrl: string): CurrentRealmInfoForRenderer {
  return {
    serverName: realmAdapter.about.configurations?.realmName || realmAdapter.baseUrl,
    layer: '',
    domain: realmAdapter.baseUrl,
    contentServerUrl: contentServerUrl
  }
}
