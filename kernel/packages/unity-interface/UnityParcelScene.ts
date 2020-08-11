import { gridToWorld } from '../atomicHelpers/parcelScenePositions'
import { DevTools } from 'shared/apis/DevTools'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { createLogger, createDummyLogger } from 'shared/logger'
import { EnvironmentData, LoadableParcelScene } from 'shared/types'
import { getParcelSceneID } from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { UnityScene } from './UnityScene'
import { DEBUG } from 'config'

export class UnityParcelScene extends UnityScene<LoadableParcelScene> {
  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    super(data)
    let loggerPrefix = data.data.basePosition.x + ',' + data.data.basePosition.y + ': '
    this.logger = DEBUG === true ? createLogger(loggerPrefix) : createDummyLogger()
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, worker.position)

    this.worker.system
      .then((system) => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
        parcelIdentity.cid = getParcelSceneID(worker.parcelScene)
      })
      .catch((e) => this.logger.error('Error initializing system', e))
  }
}
