import { BasicEphemeralKey, MessageInput } from 'decentraland-auth-protocol'

import { defaultLogger } from 'shared/logger'
import { Login } from './Login'
import { API, APIOptions } from './API'

const jwt = require('jsonwebtoken')

type AuthOptions = {
  ephemeralKeyTTL: number
  api?: APIOptions
}

type AccessToken = {
  ephemeral_key: string
  exp: number
  user_id: string
  version: string
}

const LOCAL_STORAGE_KEY = 'decentraland-auth-user-token'

export class Auth {
  static defaultOptions: AuthOptions = {
    ephemeralKeyTTL: 60 * 60 * 2 // TTL for the ephemeral key
  }

  private options: AuthOptions
  private api: API
  private userToken: string | null = null
  private accessToken: string | null = null
  private serverPublicKey: string | null = null
  private ephemeralKey: BasicEphemeralKey | null = null
  private loginManager: Login

  constructor(options: Partial<AuthOptions> = {}) {
    this.options = {
      ...Auth.defaultOptions,
      ...options
    }
    this.api = new API(this.options.api)
    this.loginManager = new Login(this.api)
    this.userToken = localStorage.getItem(LOCAL_STORAGE_KEY) || null
    this.ephemeralKey = BasicEphemeralKey.generateNewKey(this.options.ephemeralKeyTTL)
  }

  // returns a user token
  async login(target?: HTMLElement) {
    if (this.userToken === null) {
      const [userToken] = await Promise.all([
        target ? this.loginManager.fromIFrame(target) : this.loginManager.fromPopup(),
        this.getServerPublicKey()
      ])
      this.userToken = userToken
      localStorage.setItem(LOCAL_STORAGE_KEY, this.userToken)
      return this.userToken
    }
    return this.userToken
  }

  async logout() {
    await this.loginManager.logout()

    // remove from local storage
    localStorage.removeItem(LOCAL_STORAGE_KEY)

    // remove from instance
    this.userToken = null
    this.accessToken = null
  }

  isLoggedIn() {
    return this.userToken !== null
  }

  getEphemeralKey() {
    if (!this.ephemeralKey || this.ephemeralKey.hasExpired()) {
      this.ephemeralKey = BasicEphemeralKey.generateNewKey(this.options.ephemeralKeyTTL)
    }
    return this.ephemeralKey
  }

  /**
   * Returns the user data of the JWT decoded payload
   */
  async getAccessTokenData() {
    return jwt.decode(await this.getAccessToken()) as AccessToken
  }

  async getAccessToken() {
    const ephKey = this.getEphemeralKey()
    const pubKey = ephKey.key.publicKeyAsHexString()

    if (this.accessToken) {
      try {
        const tokenData = jwt.decode(this.accessToken) as AccessToken
        if (tokenData.ephemeral_key === pubKey) {
          const publicKey = await this.getServerPublicKey()
          jwt.verify(this.accessToken, publicKey)
          return this.accessToken
        }
      } catch (e) {
        // invalid token, generate a new one
      }
    }
    const userToken = await this.login()

    try {
      const { token } = await this.api.token({
        userToken,
        pubKey
      })
      this.accessToken = token
      return token
    } catch (e) {
      defaultLogger.error(e.message)
      await this.logout()
      throw e
    }
  }

  async createHeaders(url: string, options: RequestInit = {}) {
    await this.login()

    let method = 'GET'
    let body: any = null
    let headers: Record<string, string> = {}

    if (options.method) {
      method = options.method.toUpperCase()
    }

    if (options.body) {
      body = Buffer.from(options.body as string)
    }

    const input = MessageInput.fromHttpRequest(method, url, body)
    const accessToken = await this.getAccessToken()

    // add required headers
    const requiredHeaders = this.getEphemeralKey().makeMessageCredentials(input, accessToken)
    for (const [key, value] of requiredHeaders.entries()) {
      headers[key] = value
    }

    // add optional headers
    if (options && options.headers) {
      const optionalHeaders = options.headers as Record<string, string>
      headers = {
        ...headers,
        ...optionalHeaders
      }
    }

    return headers
  }

  async createRequest(url: string, options: RequestInit = {}): Promise<Request> {
    let headers = await this.createHeaders(url, options)
    if (options.headers) {
      headers = { ...(options.headers as Record<string, string>), ...headers }
    }

    const request = new Request(url, {
      ...options,
      headers
    })

    return request
  }

  async getMessageCredentials(message: string | null) {
    const msg = message === null ? null : Buffer.from(message)
    const input = MessageInput.fromMessage(msg)
    const accessToken = await this.getAccessToken()

    const credentials = this.getEphemeralKey().makeMessageCredentials(input, accessToken)

    let result: Record<string, string> = {}

    for (const [key, value] of credentials.entries()) {
      result[key] = value
    }

    return result
  }

  dispose() {
    this.loginManager.dispose()
  }

  private async getServerPublicKey() {
    if (this.serverPublicKey) {
      return this.serverPublicKey
    }
    const serverPublicKey = await this.api.pubKey()
    this.serverPublicKey = serverPublicKey
    return serverPublicKey
  }
}
