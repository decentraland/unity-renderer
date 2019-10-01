import { all, call, fork, put, takeLatest } from 'redux-saga/effects'
import { authFailure, authRequest, authSuccess, AUTH_REQUEST, LOGIN, LOGOUT, LoginAction } from './actions'
import { AuthData } from './types'

export type CallbackResult = { data: AuthData; redirectUri: string | undefined }

export interface CallableLogin {
  login: any
  logout: () => void
  isExpired: (store: any) => boolean
  handleCallback: () => Promise<{ data: any; redirectUri: string | undefined }>
  restoreSession: () => void
}

export function authSaga(callbackProvider: CallableLogin) {
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
    let redirectUri: string | null = null
    try {
      const result: CallbackResult = yield call(() => callbackProvider.handleCallback())
      data = result.data
      redirectUri = result.redirectUri || window.location.href
    } catch (error) {
      if (error.error && error.error.includes('unauthorized')) {
        yield call(() => callbackProvider.logout())
        return
      } else {
        try {
          data = yield call(() => callbackProvider.restoreSession())
        } catch (error) {
          yield put(authFailure(error.message))
          return
        }
      }
    }
    yield put(authSuccess(data, redirectUri))
  }
}
