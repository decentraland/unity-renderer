import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { EnvironmentData } from 'shared/types'
import { Realm } from '../dao/types'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'

declare const window: any

@registerAPI('EnvironmentAPI')
export class EnvironmentAPI extends ExposableAPI {
  data!: EnvironmentData<any>
  /**
   * Returns the coordinates and the definition of a parcel
   */
  @exposeMethod
  async getBootstrapData(): Promise<EnvironmentData<any>> {
    return this.data
  }

  /**
   * Returns the current connected realm
   */
  @exposeMethod
  async getCurrentRealm(): Promise<Realm | undefined> {
    const store: Store<RootState> = window['globalStore']
    return store.getState().dao.realm
  }
}
