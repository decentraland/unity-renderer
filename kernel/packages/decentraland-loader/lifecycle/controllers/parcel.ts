import { EventEmitter } from 'events'

import { Vector2Component } from 'atomicHelpers/landHelpers'

import { parcelsInScope, ParcelConfigurationOptions } from '../lib/scope'
import { ParcelLifeCycleStatus } from '../lib/parcel.status'

export type ParcelSightSeeingReport = {
  sighted: string[]
  lostSight: string[]
}

export class ParcelLifeCycleController extends EventEmitter {
  config: ParcelConfigurationOptions
  currentPosition?: Vector2Component

  isTargetPlaced: boolean = false

  missingDataParcelsCount = 0

  parcelStatus = new Map<string, ParcelLifeCycleStatus>()

  currentlySightedParcels = new Set<string>()

  constructor(config: ParcelConfigurationOptions) {
    super()
    this.config = config
  }

  reportCurrentPosition(position: Vector2Component): ParcelSightSeeingReport | undefined {
    if (this.currentPosition && this.currentPosition.x === position.x && this.currentPosition.y === position.y) {
      // same position, no news
      return undefined
    }

    this.currentPosition = position

    this.isTargetPlaced = true
    const sightedParcels = parcelsInScope(this.config.lineOfSightRadius, position)
    const sightedParcelsSet = new Set<string>()

    const newlySightedParcels = sightedParcels.filter(parcel => {
      sightedParcelsSet.add(parcel)
      return this.parcelSighted(parcel)
    })

    const secureParcels = new Set(parcelsInScope(this.config.lineOfSightRadius + this.config.secureRadius, position))

    const currentlyPlusNewlySightedParcels = [...this.currentlySightedParcels] // this.currentlySightedParcels from t - 1 + newSightedParcels (added on this#parcelSighted)

    const newlyOOSParcels = currentlyPlusNewlySightedParcels
      .filter(parcel => !secureParcels.has(parcel))
      .filter(parcel => this.switchParcelToOutOfSight(parcel))

    this.currentlySightedParcels = new Set(currentlyPlusNewlySightedParcels.filter($ => secureParcels.has($)))

    this.emit('Sighted', newlySightedParcels)
    this.emit('Lost sight', newlyOOSParcels)

    return { sighted: newlySightedParcels, lostSight: newlyOOSParcels }
  }

  inSight(parcel: string) {
    return !!this.currentlySightedParcels.has(parcel)
  }

  /**
   * Returns true if newly sighted, false otherwise
   *
   * @param parcel Parcel string position
   */
  parcelSighted(parcel: string): boolean {
    let status = this.getParcelStatus(parcel)
    if (status.isOutOfSight()) {
      this.currentlySightedParcels.add(parcel)
      status.setInSight()
      return true
    }
    return false
  }

  getParcelStatus(parcel: string) {
    let status
    this.parcelStatus.get(parcel)
    if (!status) {
      status = new ParcelLifeCycleStatus(parcel)
      this.parcelStatus.set(parcel, status)
    }
    return status
  }

  /**
   * Returns true if newly switched to out of sight, false otherwise
   *
   * @param parcel Parcel position string
   */
  switchParcelToOutOfSight(parcel: string) {
    if (!this.parcelStatus.has(parcel)) {
      return false
    }
    const status = this.parcelStatus.get(parcel)
    if (status && status.isInSight()) {
      status.setOffSight()
      return true
    }
    return false
  }
}
