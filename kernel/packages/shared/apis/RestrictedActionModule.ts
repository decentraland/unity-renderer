import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import defaultLogger from '../logger'
import { unityInterface } from 'unity-interface/UnityInterface'
import { ParcelIdentity } from './ParcelIdentity'
import { Vector3 } from 'decentraland-ecs/src'
import { gridToWorld, isInParcel, parseParcelPosition } from '../../atomicHelpers/parcelScenePositions'
import { lastPlayerPosition } from '../world/positionThings'

export enum Permission {
  ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE = 'ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE'
}

export interface IRestrictedActionModule {
  movePlayerTo(newPosition: Vector3, cameraTarget?: Vector3): Promise<void>
}

@registerAPI('RestrictedActionModule')
export class RestrictedActionModule extends ExposableAPI implements IRestrictedActionModule {
  parcelIdentity = this.options.getAPIInstance(ParcelIdentity)

  getSceneData() {
    return this.parcelIdentity.land.sceneJsonData
  }

  hasPermission(permission: Permission) {
    const json = this.getSceneData()
    const list = json.requiredPermissions || []
    return list.indexOf(permission) !== -1
  }

  calculatePosition(newPosition: Vector3) {
    const base = parseParcelPosition(this.getSceneData().scene.base)

    const basePosition = new Vector3()
    gridToWorld(base.x, base.y, basePosition)

    return basePosition.add(newPosition)
  }

  isPositionValid(position: Vector3) {
    return this.getSceneData().scene.parcels.some((parcel) => {
      const { x, y } = parseParcelPosition(parcel)
      return isInParcel(position, gridToWorld(x, y))
    })
  }

  @exposeMethod
  async movePlayerTo(newPosition: Vector3, cameraTarget?: Vector3): Promise<void> {
    // checks permissions
    if (!this.hasPermission(Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE)) {
      defaultLogger.error(`Permission "${Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE}" is required`)
      return
    }
    const position = this.calculatePosition(newPosition)
    // validate new position is inside of some scene
    if (!this.isPositionValid(position)) {
      defaultLogger.error('Error: Position is out of scene', position)
      return
    }
    if (!this.isPositionValid(lastPlayerPosition)) {
      defaultLogger.error('Error: Player is not inside of scene', lastPlayerPosition)
      return
    }
    unityInterface.Teleport({ position, cameraTarget })
  }
}
