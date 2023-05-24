import { getWorld, isInsideWorldLimits } from '@dcl/schemas'

import { countParcelsCloseTo, ParcelArray } from 'shared/comms/interface/utils'
import defaultLogger from 'lib/logger'

import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'

import { store } from 'shared/store/isolatedStore'
import { getRealmAdapter } from 'shared/realm/selectors'
import { Parcel } from 'shared/dao/types'
import { urlWithProtocol } from 'shared/realm/resolver'
import { trackTeleportTriggered } from 'shared/loading/types'
import { teleportToAction } from 'shared/scene-loader/actions'
import { getParcelPosition } from 'shared/scene-loader/selectors'

import { lastPlayerPosition } from 'shared/world/positionThings'
import { homePointKey } from 'shared/atlas/utils'
import { getFromPersistentStorage } from 'lib/browser/persistentStorage'
import { changeToMostPopulatedRealm } from '../dao'

const descriptiveValidWorldRanges = getWorld()
  .validWorldRanges.map((range) => `(X from ${range.xMin} to ${range.xMax}, and Y from ${range.yMin} to ${range.yMax})`)
  .join(' or ')

// TODO: don't do classes if it holds no state. Use namespaces or functions instead.
export class TeleportController {
  public static async goToCrowd(): Promise<{ message: string; success: boolean }> {
    try {
      let usersParcels = await fetchLayerUsersParcels()

      const currentParcel = getParcelPosition(store.getState())

      usersParcels = usersParcels.filter(
        (it) => isInsideWorldLimits(it[0], it[1]) && currentParcel.x !== it[0] && currentParcel.y !== it[1]
      )

      if (usersParcels.length > 0) {
        // Sorting from most close users
        const [target, closeUsers] = usersParcels
          .map((it) => [it, countParcelsCloseTo(it, usersParcels)] as [ParcelArray, number])
          .sort(([_, score1], [__, score2]) => score2 - score1)[0]

        return TeleportController.goTo(
          target[0],
          target[1],
          `Found a parcel with ${closeUsers} user(s) nearby: ${target[0]},${target[1]}. Teleporting...`
        )
      } else {
        return {
          message: 'There seems to be no users in other parcels at the current realm. Could not teleport.',
          success: false
        }
      }
    } catch (e) {
      defaultLogger.error('Error while trying to teleport to crowd', e)
      return {
        message: 'Could not teleport to crowd! Could not get information about other users in the realm',
        success: false
      }
    }
  }

  public static async goToRandom(): Promise<{ message: string; success: boolean }> {
    const x = Math.floor(Math.random() * 301) - 150
    const y = Math.floor(Math.random() * 301) - 150
    const tpMessage = `Teleporting to random location (${x}, ${y})...`
    return TeleportController.goTo(x, y, tpMessage)
  }

  public static async goToHome(): Promise<{ message: string; success: boolean }> {
    try {
      const homeCoordinates = await fetchHomePoint()

      return TeleportController.goTo(
        homeCoordinates.x,
        homeCoordinates.y,
        `Teleporting to Home (${homeCoordinates.x},${homeCoordinates.y})...`
      )
    } catch (e) {
      defaultLogger.error('Error while trying to teleport to Home', e)
      return {
        message: 'Could not teleport to Home!',
        success: false
      }
    }
  }

  public static async goTo(
    x: number,
    y: number,
    teleportMessage?: string
  ): Promise<{ message: string; success: boolean }> {
    const tpMessage: string = teleportMessage ? teleportMessage : `Teleporting to ${x}, ${y}...`
    if (isInsideWorldLimits(x, y)) {
      try {
        await changeToMostPopulatedRealm()

        store.dispatch(trackTeleportTriggered(tpMessage))
        store.dispatch(teleportToAction({ position: gridToWorld(x, y) }))

        return { message: tpMessage, success: true }
      } catch (e: any) {
        return { message: e.message, success: false }
      }
    } else {
      const errorMessage = `Coordinates are outside of the boundaries. Valid ranges are: ${descriptiveValidWorldRanges}.`
      return { message: errorMessage, success: false }
    }
  }
}

async function fetchHomePoint(): Promise<{ x: number; y: number }> {
  const homePoint: string = await getFromPersistentStorage(homePointKey)
  if (homePoint) {
    const [x, y] = homePoint.split(',').map((p) => parseFloat(p))
    gridToWorld(x, y, lastPlayerPosition)
    return { x, y }
  }
  return { x: 0, y: 0 }
}

async function fetchLayerUsersParcels(): Promise<ParcelArray[]> {
  const realmAdapter = getRealmAdapter(store.getState())

  try {
    if (realmAdapter) {
      const parcelsResponse = await fetch(`${urlWithProtocol(realmAdapter.baseUrl)}/stats/parcels`)

      if (parcelsResponse.ok) {
        const parcelsBody = await parcelsResponse.json()
        const usersParcels: Parcel[] = []

        if (parcelsBody.parcels) {
          for (const {
            peersCount,
            parcel: { x, y }
          } of parcelsBody.parcels) {
            const parcel: Parcel = [x, y]
            for (let i = 0; i < peersCount; i++) {
              usersParcels.push(parcel)
            }
          }
        }
        return usersParcels
      }
    }
  } catch {}

  return []
}
