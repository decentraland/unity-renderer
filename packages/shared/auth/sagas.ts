import { fork, call, all, takeLatest, put } from 'redux-saga/effects'

import { AUTH_REQUEST, LOGIN, LOGOUT, authRequest, authSuccess, authFailure, LoginAction } from './actions'
import { AuthData } from './types'

export type CallbackResult = { data: AuthData; redirectUrl: string | null }

export interface CallableLogin {
  login: any
  logout: () => void
  isExpired: (store: any) => boolean
  handleCallback: any
  restoreSession: () => void
}

export function createSaga(callbackProvider: CallableLogin) {
  return function* authSaga(): any {
    yield fork(handleRestoreSession)
    yield all([
      takeLatest(LOGIN, handleLogin),
      takeLatest(LOGOUT, handleLogout),
      takeLatest(AUTH_REQUEST, handleAuthRequest)
    ])
  }

  function* handleLogin(action: LoginAction): any {
    yield call(() => callbackProvider.login(action.payload.redirectUrl))
  }

  function* handleLogout(): any {
    yield call(() => callbackProvider.logout())
  }

  function* handleRestoreSession(): any {
    yield put(authRequest())
  }

  function* handleAuthRequest(): any {
    let data: AuthData
    let redirectUrl: string | null = null
    try {
      const result: CallbackResult = yield call(() => callbackProvider.handleCallback())
      data = result.data
      redirectUrl = result.redirectUrl
    } catch (error) {
      try {
        data = yield call(() => callbackProvider.restoreSession())
      } catch (error) {
        yield put(authFailure(error.message))
        return
      }
    }
    yield put(authSuccess(data, redirectUrl))
  }
}
