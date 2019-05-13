import { Vector2Component } from 'atomicHelpers/landHelpers'
import { SceneLifeCycleController } from './scene'
import { EventEmitter } from 'events'
import { SceneLifeCycleStatus } from '../lib/scene.status'

export class PositionLifecycleController extends EventEmitter {
  currentPosition?: Vector2Component
  isSettled: boolean = false

  constructor(public sceneController: SceneLifeCycleController) {
    super()
  }

  reportCurrentPosition(position: Vector2Component) {
    if (this.currentPosition && this.currentPosition.x === position.x && this.currentPosition.y === position.y) {
      return
    }
    this.currentPosition = position
    const currentPosStr = `${position.x},${position.y}`
    if (this.isSettled && !this.sceneController.hasStarted(currentPosStr)) {
      // Teleport
      this.isSettled = false
      this.emit('Unsettled Position', currentPosStr)
    }
  }

  reportSceneStarted(scene: SceneLifeCycleStatus) {
    if (!this.isSettled && this.sceneController.contains(scene, this.currentPosition!)) {
      this.isSettled = true
      this.emit('Settled Position')
    }
  }
}
