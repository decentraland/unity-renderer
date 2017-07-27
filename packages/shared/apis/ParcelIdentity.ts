import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { ILand } from 'shared/types'

export interface IParcelIdentity {
  getParcel(): Promise<{ x: number; y: number; land: ILand }>
}

@registerAPI('ParcelIdentity')
export class ParcelIdentity extends ExposableAPI implements IParcelIdentity {
  x!: number
  y!: number
  land!: ILand

  /**
   * Returns the coordinates and the definition of a parcel
   */
  @exposeMethod
  async getParcel(): Promise<{ x: number; y: number; land: ILand }> {
    return {
      x: this.x,
      y: this.y,
      land: this.land
    }
  }
}
