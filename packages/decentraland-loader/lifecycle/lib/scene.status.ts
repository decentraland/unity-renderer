import { ILand } from 'shared/types'

export class SceneLifeCycleStatus {
  status: 'unloaded' | 'awake' | 'ready' = 'unloaded'

  constructor(public sceneDescription: ILand) {}

  isAwake() {
    return this.status !== 'unloaded'
  }

  isDead() {
    return this.status === 'unloaded'
  }

  isRunning() {
    return this.status === 'ready'
  }
}
