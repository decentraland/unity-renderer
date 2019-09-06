import * as jwt from 'jsonwebtoken'

export class CommsAuth {
  constructor(public config: { baseURL: string }) {}

  async getCommsAccessToken(ephKey: any, accessToken: string) {
    const pubKey = ephKey.key.publicKeyAsHexString()

    try {
      const tokenData = jwt.decode(accessToken) as any
      if (tokenData.ephemeral_key === pubKey) {
        const publicKey = await this.pubKey()
        jwt.verify(accessToken, publicKey)
        return accessToken
      }
    } catch (e) {
      // invalid token, generate a new one
    }
    const { token } = await this.token({
      userToken: accessToken,
      pubKey
    })
    return token
  }

  getPath(path: string) {
    return this.config.baseURL + path
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
