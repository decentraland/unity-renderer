import future from 'fp-future'
import { createStore, Store, applyMiddleware, combineReducers, compose } from 'redux'
import auth0 from 'auth0-js'
import { v4 as uuid } from 'uuid'
import { BasicEphemeralKey, MessageInput } from 'decentraland-auth-protocol'

import { CommsAuth } from './CommsAuth'
import { authReducer } from './reducer'
import { getAccessToken, isLoggedIn, getData, getSub } from './selectors'
import { AuthState, AuthData } from './types'
import { CallbackResult, createSaga } from './sagas'
import { createSelector } from 'reselect'
import createSagaMiddleware from '@redux-saga/core'
import { getServerConfigurations } from 'config'
import { login } from './actions'

type RootState = any

export function isTokenExpired(expiresAt: number) {
  return expiresAt > 0 && expiresAt < Date.now()
}

export class Auth {
  store: Store<{ auth: AuthState }>
  ephemeralKey?: BasicEphemeralKey

  webAuth: auth0.WebAuth
  sagaMiddleware: any

  comms: CommsAuth

  isExpired = createSelector<RootState, AuthState['data'], boolean>(
    getData,
    data => !!data && isTokenExpired(data.expiresAt)
  ) as (store: any) => boolean

  constructor(
    public config: {
      ephemeralKeyTTL: number
      clientId: string
      domain: string
      redirectUri: string
      audience: string
    }
  ) {
    this.sagaMiddleware = createSagaMiddleware()
    const composeEnhancers = (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose
    this.comms = new CommsAuth({ baseURL: getServerConfigurations().auth })
    this.store = createStore(
      combineReducers({ auth: authReducer }),
      composeEnhancers(applyMiddleware(this.sagaMiddleware))
    )

    this.webAuth = new auth0.WebAuth({
      clientID: this.config.clientId,
      domain: this.config.domain,
      redirectUri: this.config.redirectUri,
      responseType: 'token id_token',
      audience: this.config.audience,
      scope: 'openid email'
    })
  }
  setup() {
    this.sagaMiddleware.run(createSaga(this))
  }

  async getUserId() {
    await this.getAccessToken()
    return getSub(this.store.getState())
  }

  async getCommsAccessToken() {
    return this.comms.getCommsAccessToken(this.getEphemeralKey(), await this.getAccessToken())
  }

  getAccessToken() {
    if (isLoggedIn(this.store.getState())) {
      return Promise.resolve(getAccessToken(this.store.getState()))
    }
    const result = future<string>()
    const unsubscribe = this.store.subscribe(() => {
      const state = this.store.getState()
      if (isLoggedIn(state)) {
        result.resolve(getAccessToken(state))
        unsubscribe()
      } else if (state.auth.error) {
        this.login(this.config.redirectUri)
      }
    })
    return result
  }

  isLoggedIn() {
    return isLoggedIn(this.store.getState())
  }

  getEphemeralKey() {
    if (!this.ephemeralKey || this.ephemeralKey.hasExpired()) {
      this.ephemeralKey = BasicEphemeralKey.generateNewKey(this.config.ephemeralKeyTTL)
    }
    return this.ephemeralKey
  }

  async getMessageCredentials(message: string | null) {
    const msg = message === null ? null : Buffer.from(message)
    const input = MessageInput.fromMessage(msg)
    const accessToken = await this.getCommsAccessToken()

    const credentials = await this.getEphemeralKey().makeMessageCredentials(input, accessToken)

    let result: Record<string, string> = {}

    for (const [key, value] of credentials.entries()) {
      result[key] = value
    }

    return result
  }

  handleCallback(): Promise<CallbackResult> {
    return new Promise((resolve, reject) => {
      this.webAuth.parseHash((err, auth) => {
        if (err) {
          debugger
          reject(err)
          return
        }
        if (auth && auth.accessToken && auth.idToken) {
          this.webAuth.client.userInfo(auth.accessToken, (err, user) => {
            if (err) {
              this.store.dispatch(login(this.config.redirectUri))
              reject(err)
              return
            }

            let redirectUrl = null
            if (auth.state) {
              redirectUrl = localStorage.getItem(auth.state)
              if (redirectUrl) {
                localStorage.removeItem(auth.state)
              }
            }

            const data: AuthData = {
              email: user.email!,
              sub: user.sub,
              expiresAt: auth.expiresIn! * 1000 + new Date().getTime(),
              accessToken: auth.accessToken!,
              idToken: auth.idToken!
            }
            resolve({ data, redirectUrl })
          })
        } else {
          reject(new Error('No access token found in the url hash'))
        }
      })
    })
  }

  login(redirectUrl?: string) {
    let options: auth0.AuthorizeOptions = {}
    if (redirectUrl) {
      const nonce = uuid()
      localStorage.setItem(nonce, redirectUrl)
      options.state = nonce
    }
    try {
      this.webAuth.authorize(options)
    } catch (e) {
      throw e
    }
  }

  restoreSession(): Promise<AuthData> {
    return new Promise((resolve, reject) => {
      this.webAuth.checkSession({}, (err, auth) => {
        if (err) {
          this.store.dispatch(login(this.config.redirectUri))
          return
        }
        const result: AuthData = {
          email: auth.idTokenPayload.email,
          sub: auth.idTokenPayload.sub,
          expiresAt: auth.expiresIn * 1000 + new Date().getTime(),
          accessToken: auth.accessToken,
          idToken: auth.idToken
        }
        resolve(result)
      })
    })
  }

  logout(): Promise<void> {
    return new Promise(resolve => {
      this.webAuth.logout({
        returnTo: window.location.origin
      })
      resolve()
    })
  }

  createHeaders(idToken: string) {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${idToken}`
    }
    return headers
  }
}
