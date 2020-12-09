import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { EnvironmentData } from 'shared/types'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'
import { getRealm } from 'shared/dao/selectors'
import { PREVIEW } from 'config'

type EnvironmentRealm = {
  domain: string
  layer: string
  serverName: string
  displayName: string
}

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
   * Returns whether the scene is running in preview mode or not
   */
  @exposeMethod
  async isPreviewMode(): Promise<boolean> {
    return PREVIEW
  }

  /**
   * Returns the current connected realm
   */
  @exposeMethod
  async getCurrentRealm(): Promise<EnvironmentRealm | undefined> {
    const store: Store<RootState> = window['globalStore']
    const realm = getRealm(store.getState())

    if (!realm) {
      return undefined
    }
    const { domain, layer, catalystName: serverName } = realm
    return {
      domain,
      layer,
      serverName,
      displayName: `${serverName}-${layer}`
    }
  }
}
