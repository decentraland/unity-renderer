import { disconnect, sendToMordor } from '../comms'
import { setLoadingScreenVisible } from '../../unity-interface/dcl'
import { future, IFuture } from 'fp-future'
import { bringDownClientAndShowError } from '../loading/ReportFatalError'
import { NEW_LOGIN } from '../loading/types'
import { removeUserProfile } from '../comms/peers'

export class Session {
  private static _instance: IFuture<Session> = future()

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    removeUserProfile()
    window.location.reload()
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
  }
}
