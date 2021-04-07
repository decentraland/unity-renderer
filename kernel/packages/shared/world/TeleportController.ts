import { parcelLimits } from 'config'

import { lastPlayerPosition, teleportObservable, isInsideWorldLimits } from 'shared/world/positionThings'
import { POIs } from 'shared/comms/POIs'
import { countParcelsCloseTo, ParcelArray } from 'shared/comms/interface/utils'
import defaultLogger from 'shared/logger'
import { ensureUnityInterface } from 'shared/renderer'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'

import { StoreContainer } from '../store/rootTypes'
import { WORLD_EXPLORER } from '../../config/index'
import { isInitialLoading, isWaitingTutorial } from '../loading/selectors'
import Html from '../Html'
import { isLoginStageCompleted, isSignUp } from '../session/selectors'
import { getCommsServer, getRealm } from 'shared/dao/selectors'
import { LayerUserInfo } from 'shared/dao/types'

declare const globalThis: StoreContainer

export const CAMPAIGN_PARCEL_SEQUENCE = [
  { x: -3, y: -33 },
  { x: 72, y: -9 },
  { x: -55, y: 143 },
  { x: 58, y: 2 },
  { x: 61, y: -27 },
  { x: -49, y: -41 },
  { x: 36, y: 46 },
  { x: -71, y: -38 },
  { x: -129, y: -141 },
  { x: 52, y: 2 },
  { x: -37, y: 57 },
  { x: 59, y: 133 },
  { x: 57, y: 8 },
  { x: -40, y: -49 },
  { x: -12, y: -39 },
  { x: -9, y: 73 },
  { x: 87, y: 18 },
  { x: 67, y: -21 },
  { x: -75, y: 73 },
  { x: -15, y: -22 },
  { x: -32, y: -44 },
  { x: 52, y: 16 },
  { x: -71, y: -71 },
  { x: -55, y: 1 },
  { x: -25, y: 103 },
  { x: 52, y: 10 },
  { x: 12, y: 46 },
  { x: -5, y: -16 },
  { x: 105, y: -21 },
  { x: -11, y: -30 },
  { x: -49, y: -49 },
  { x: 113, y: -7 },
  { x: 52, y: -71 },
  { x: -43, y: 53 },
  { x: 63, y: 2 },
  { x: -134, y: -121 },
  { x: 28, y: 45 },
  { x: 137, y: 34 },
  { x: -43, y: 57 },
  { x: 16, y: 83 },
  { x: 60, y: 115 },
  { x: -40, y: 33 },
  { x: -69, y: 77 },
  { x: -48, y: 58 },
  { x: -35, y: -42 },
  { x: 24, y: -124 },
  { x: -148, y: -35 },
  { x: -109, y: -89 }
]

// TODO: don't do classess if it holds no state. Use namespaces or functions instead.
export class TeleportController {
  public static ensureTeleportAnimation() {
    if (
      !isInitialLoading(globalThis.globalStore.getState()) &&
      isLoginStageCompleted(globalThis.globalStore.getState())
    ) {
      Html.showTeleportAnimation()
    }
  }

  public static stopTeleportAnimation() {
    if (!isSignUp(globalThis.globalStore.getState()) && !isWaitingTutorial(globalThis.globalStore.getState())) {
      Html.hideTeleportAnimation()
      if (WORLD_EXPLORER) {
        ensureUnityInterface()
          .then((unity) => unity.unityInterface.ShowWelcomeNotification())
          .catch(defaultLogger.error)
      }
    }
  }

  public static goToMagic(): { message: string; success: boolean } {
    const target = POIs[Math.floor(Math.random() * POIs.length)]
    const { x, y } = target
    const tpMessage: string = `Teleporting to "${target.name}" (${x}, ${y})...`
    return TeleportController.goTo(parseInt('' + x, 10), parseInt('' + y, 10), tpMessage)
  }

  public static async goToCrowd(): Promise<{ message: string; success: boolean }> {
    try {
      let usersParcels = await fetchLayerUsersParcels()

      const currentParcel = worldToGrid(lastPlayerPosition)

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

  public static goToRandom(): { message: string; success: boolean } {
    const x = Math.floor(Math.random() * 301) - 150
    const y = Math.floor(Math.random() * 301) - 150
    const tpMessage = `Teleporting to random location (${x}, ${y})...`
    return TeleportController.goTo(x, y, tpMessage)
  }

  public static goToNext(): { message: string; success: boolean } {
    const current = getFromLocalStorage('launch-campaign-status') || 0
    saveToLocalStorage('launch-campaign-status', current + 1)
    const { x, y } = CAMPAIGN_PARCEL_SEQUENCE[current % CAMPAIGN_PARCEL_SEQUENCE.length]
    return TeleportController.goTo(x, y, `Teleporting you to the next scene... and more treasures!`)
  }

  public static goTo(x: number, y: number, teleportMessage?: string): { message: string; success: boolean } {
    const tpMessage: string = teleportMessage ? teleportMessage : `Teleporting to ${x}, ${y}...`

    if (isInsideWorldLimits(x, y)) {
      teleportObservable.notifyObservers({
        x: x,
        y: y,
        text: tpMessage
      } as any)

      TeleportController.ensureTeleportAnimation()

      return { message: tpMessage, success: true }
    } else {
      const errorMessage = `Coordinates are outside of the boundaries. Valid ranges are: ${parcelLimits.descriptiveValidWorldRanges}.`
      return { message: errorMessage, success: false }
    }
  }
}

async function fetchLayerUsersParcels(): Promise<ParcelArray[]> {
  const realm = getRealm(globalThis.globalStore.getState())
  const commsUrl = getCommsServer(globalThis.globalStore.getState())

  if (realm && realm.layer && commsUrl) {
    const layerUsersResponse = await fetch(`${commsUrl}/layers/${realm.layer}/users`)
    if (layerUsersResponse.ok) {
      const layerUsers: LayerUserInfo[] = await layerUsersResponse.json()
      return layerUsers.filter((it) => it.parcel).map((it) => it.parcel!)
    }
  }

  return []
}
