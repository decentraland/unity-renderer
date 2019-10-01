import { disconnect } from '../comms'
import { setLoadingScreenVisible } from '../../unity-interface/dcl'
import { Auth } from 'shared/auth/Auth'

export class Session {
  private static _instance: Session = new Session()

  auth?: Auth

  static set current(instance: Session) {
    Session._instance = instance
  }

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    disconnect()
    this.auth && (await this.auth.logout())
  }
}
