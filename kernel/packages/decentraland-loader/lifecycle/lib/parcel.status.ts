import { parseParcelPosition } from 'atomicHelpers/parcelScenePositions'

export class ParcelLifeCycleStatus {
  x: number
  y: number
  xy: string

  private status: 'in sight' | 'oos'

  constructor(coord: string) {
    const { x, y } = parseParcelPosition(coord)
    this.x = x
    this.y = y
    this.xy = coord
    this.status = 'oos'
  }

  isOutOfSight() {
    return this.status === 'oos'
  }
  isInSight() {
    return this.status === 'in sight'
  }
  setInSight() {
    this.status = 'in sight'
  }
  setOffSight() {
    this.status = 'oos'
  }
}
