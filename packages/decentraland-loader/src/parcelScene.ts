import { parseParcelPosition, isValidParcelSceneShape } from 'atomicHelpers/parcelScenePositions'
import { error } from 'engine/logger'
import { getLand } from './landLoader'
import { ILand } from 'shared/types'

interface Vector2Component {
  x: number
  y: number
}

export class ParcelScene {
  id: string = ''
  base: Vector2Component = { x: 0, y: 0 }
  parcels: Vector2Component[] = []
  data: ILand | null = null
  _isValid = false

  get position() {
    return `${this.x},${this.y}`
  }

  get isPending() {
    return !this.didLoad && !this.isRequestingData
  }

  private isRequestingData: Promise<any> | null = null
  private didLoad = false

  get isValid() {
    return this.parcels.length > 0 && !!this.data && this._isValid
  }

  constructor(public x: number, public y: number) {
    this.base.x = x | 0
    this.base.y = y | 0
    this.id = `${x},${y}`
  }

  async setData(data: ILand) {
    if (this.data) {
      error(`Trying to set ParcelScene.data twice`)
      return
    }

    this.data = data

    const parcels: Vector2Component[] =
      (data.scene && data.scene.scene && data.scene.scene.parcels.map(parseParcelPosition)) || []

    const sanitizedParcels: Vector2Component[] = []
    let isBasePresent = false

    for (let { x, y } of parcels) {
      isBasePresent = isBasePresent || (x === this.base.x && y === this.base.y)
      if (!sanitizedParcels.some($ => $.x === x && $.y === y)) {
        sanitizedParcels.push({ x, y })
      }
    }

    if (!isBasePresent) {
      error(`Error: Parcel with base ${this.base.x},${this.base.y} doesn't include itself in the list of parcels`)
      this._isValid = false
    } else if (!parcels.length) {
      error(`Error: Parcel with base ${this.base.x},${this.base.y} has no list of parcels`)
      this._isValid = false
    } else if (parcels.length > 1 && !isValidParcelSceneShape(parcels)) {
      error(`Error: the list of parcels aren't connected.`)
      this._isValid = false
    } else {
      for (let $ of sanitizedParcels) {
        const land = await getLand($.x, $.y)

        if (!land) {
          error(`Error: The parcel at ${$.x},${$.y} cannot be verified. Aborting load.`)
          this._isValid = false
        } else if (land.mappingsResponse.root_cid !== data.mappingsResponse.root_cid) {
          error(
            `Error: The parcel at ${$.x},${$.y} with root_cid ${land.mappingsResponse.root_cid}, is not equal to ${
              data.mappingsResponse.root_cid
            }. Aborting load.`
          )
          this._isValid = false
          return
        }
      }

      this.parcels = sanitizedParcels

      data.scene.scene.parcels = sanitizedParcels.map(({ x, y }) => `${x},${y}`)

      this._isValid = true
    }
  }

  async load() {
    if (this.isRequestingData) {
      return this.isRequestingData
    }

    try {
      this.isRequestingData = this.internalLoad()
    } catch (e) {
      setTimeout(() => (this.didLoad = false), 5000)
      error(e)
      this.isRequestingData = null
      throw e
    }

    return this.isRequestingData
  }

  private async internalLoad() {
    this.didLoad = true

    const land = await getLand(this.base.x, this.base.y)

    if (land && 'scene' in land && land.scene) {
      await this.setData(land)
    }
  }
}
