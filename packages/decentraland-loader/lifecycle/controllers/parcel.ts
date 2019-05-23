import { EventEmitter } from 'events'

import { Vector2Component } from 'atomicHelpers/landHelpers'

import { parcelsInScope, ParcelConfigurationOptions } from '../lib/scope'
import { ParcelLifeCycleStatus } from '../lib/parcel.status'

export class ParcelLifeCycleController extends EventEmitter {
  constructor(config: ParcelConfigurationOptions) {
    super()
    this.config = config
  }

  config: ParcelConfigurationOptions
  currentPosition?: Vector2Component

  isTargetPlaced: boolean = false

  missingDataParcelsCount = 0

  parcelStatus: { [key: string]: ParcelLifeCycleStatus } = {}

  currentlySightedParcels: { [key: string]: boolean } = {}
  currentlySightedParcelsArray: string[] = []

  reportCurrentPosition(position: Vector2Component) {
    if (this.currentPosition && this.currentPosition.x === position.x && this.currentPosition.y === position.y) {
      return
    }
    this.currentPosition = position

    this.isTargetPlaced = true
    const sightedParcels = parcelsInScope(this.config, position)
    const sightedParcelsDict: { [key: string]: boolean } = {}
    for (const parcel of sightedParcels) {
      this.parcelSighted(parcel)
    }
    for (const parcel of this.currentlySightedParcelsArray) {
      if (!sightedParcelsDict[parcel]) {
        this.switchParcelToOutOfSight(parcel)
      }
    }
    this.currentlySightedParcels = sightedParcelsDict
    this.currentlySightedParcelsArray = sightedParcels
  }

  inSight(parcel: string) {
    return !!this.currentlySightedParcels[parcel]
  }

  parcelSighted(parcel: string) {
    let status = this.parcelStatus[parcel]
    if (!status) {
      status = this.parcelStatus[parcel] = new ParcelLifeCycleStatus(parcel)
    }
    if (status.isOutOfSight()) {
      this.currentlySightedParcels[parcel] = true
      status.setInSight()
      this.emit('Sighted', parcel)
    }
  }

  switchParcelToOutOfSight(parcel: string) {
    if (!this.parcelStatus[parcel]) {
      return
    }
    const status = this.parcelStatus[parcel]
    if (status && status.isInSight()) {
      status.setOffSight()
      this.emit('Lost sight', parcel)
    }
  }
}
