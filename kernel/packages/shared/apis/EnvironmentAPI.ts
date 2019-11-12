import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { EnvironmentData } from 'shared/types'

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
}
