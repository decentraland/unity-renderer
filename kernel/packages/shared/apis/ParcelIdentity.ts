import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { ILand } from 'shared/types'

export interface IParcelIdentity {
  getParcel(): Promise<{ land: ILand; cid: string }>
}

@registerAPI('ParcelIdentity')
export class ParcelIdentity extends ExposableAPI implements IParcelIdentity {
  land!: ILand
  cid!: string
  isPortableExperience: boolean = false

  /**
   * Returns the coordinates and the definition of a parcel
   */
  @exposeMethod
  async getParcel(): Promise<{ land: ILand; cid: string }> {
    return {
      land: this.land,
      cid: this.cid
    }
  }
}
