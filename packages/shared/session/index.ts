import { disconnect, sendToMordor } from '../comms'
import { setLoadingScreenVisible } from '../../unity-interface/dcl'
import { Auth } from 'shared/auth/Auth'
import { future, IFuture } from 'fp-future'
import { bringDownClientAndShowError } from '../loading/ReportFatalError'
import { NEW_LOGIN } from '../loading/types'

export class Session {
  private static _instance: IFuture<Session> = future()

  auth?: Auth

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    this.auth && (await this.auth.logout())
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
  }
}
