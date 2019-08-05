import { getServerConfigurations } from 'config'

export type APIOptions = {
  baseURL?: string
}

export class API {
  static defaultOptions: APIOptions = {
    baseURL: getServerConfigurations().auth
  }

  options: APIOptions

  constructor(options: Partial<APIOptions> = {}) {
    this.options = {
      ...API.defaultOptions,
      ...options
    }
  }

  getPath(path: string) {
    return this.options.baseURL + path
  }

  auth() {
    return {
      loginURL: '/auth/login.html',
      logoutURL: '/auth/logout.html'
    }
  }

  async token(args: { userToken: string; pubKey: string }) {
    const { userToken, pubKey } = args
    const path = this.getPath('/token')
    const body = JSON.stringify({
      user_token: userToken,
      pub_key: pubKey
    })
    const response = await fetch(path, {
      method: 'post',
      headers: {
        'Content-Type': 'application/json'
      },
      body
    })
    const json = await response.json()

    if (!response.ok || json.error) {
      throw new Error(json.error)
    }

    return {
      token: json.access_token as string
    }
  }

  async pubKey() {
    const path = this.getPath('/public_key')
    const response = await fetch(path)
    const pubKey = await response.text()
    return pubKey
  }
}
