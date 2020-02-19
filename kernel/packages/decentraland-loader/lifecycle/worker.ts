// Entry point for the Lifecycle Worker.
// This doesn't execute on the main thread, so it's a "server" in terms of decentraland-rpc

import { WebWorkerTransport } from 'decentraland-rpc'
import { Adapter } from './lib/adapter'
import { ParcelLifeCycleController } from './controllers/parcel'
import { SceneLifeCycleController, SceneLifeCycleStatusReport } from './controllers/scene'
import { PositionLifecycleController } from './controllers/position'
import { SceneDataDownloadManager } from './controllers/download'
import { ILand, InstancedSpawnPoint } from 'shared/types'
import defaultLogger from 'shared/logger'
import { setTutorialEnabled } from './tutorial/tutorial'

const connector = new Adapter(WebWorkerTransport(self as any))

let parcelController: ParcelLifeCycleController
let sceneController: SceneLifeCycleController
let positionController: PositionLifecycleController
let downloadManager: SceneDataDownloadManager

/**
 * Hook all the events to the connector.
 *
 * Make sure the main thread watches for:
 * - 'Position.settled'
 * - 'Position.unsettled'
 * - 'Scene.shouldStart' (sceneId: string)
 * - 'Scene.shouldUnload' (sceneId: string)
 * - 'Scene.shouldPrefetch' (sceneId: string)
 *
 * Make sure the main thread reports:
 * - 'User.setPosition' { position: {x: number, y: number } }
 * - 'Scene.prefetchDone' { sceneId: string }
 */
{
  connector.on(
    'Lifecycle.initialize',
    (options: {
      contentServer: string
      contentServerBundles: string
      lineOfSightRadius: number
      secureRadius: number
      emptyScenes: boolean
      tutorialBaseURL: string
      tutorialSceneEnabled: boolean
    }) => {
      setTutorialEnabled(options.tutorialSceneEnabled)

      downloadManager = new SceneDataDownloadManager({
        contentServer: options.contentServer,
        contentServerBundles: options.contentServerBundles,
        tutorialBaseURL: options.tutorialBaseURL
      })
      parcelController = new ParcelLifeCycleController({
        lineOfSightRadius: options.lineOfSightRadius,
        secureRadius: options.secureRadius
      })
      sceneController = new SceneLifeCycleController({ downloadManager, enabledEmpty: options.emptyScenes })
      positionController = new PositionLifecycleController(downloadManager, parcelController, sceneController)
      parcelController.on('Sighted', (parcels: string[]) => connector.notify('Parcel.sighted', { parcels }))
      parcelController.on('Lost sight', (parcels: string[]) => connector.notify('Parcel.lostSight', { parcels }))

      positionController.on('Settled Position', (spawnPoint: InstancedSpawnPoint) => {
        connector.notify('Position.settled', { spawnPoint })
      })
      positionController.on('Unsettled Position', () => {
        connector.notify('Position.unsettled')
      })
      positionController.on('Tracking Event', (event: { name: string; data: any }) =>
        connector.notify('Event.track', event)
      )

      sceneController.on('Start scene', sceneId => {
        connector.notify('Scene.shouldStart', { sceneId })
      })
      sceneController.on('Preload scene', sceneId => {
        connector.notify('Scene.shouldPrefetch', { sceneId })
      })
      sceneController.on('Unload scene', sceneId => {
        connector.notify('Scene.shouldUnload', { sceneId })
      })

      connector.on('User.setPosition', (opt: { position: { x: number; y: number }; teleported: boolean }) => {
        positionController.reportCurrentPosition(opt.position, opt.teleported).catch(e => {
          defaultLogger.error(`error while resolving new scenes around`, e)
        })
      })

      connector.on('Scene.dataRequest', async (data: { sceneId: string }) =>
        connector.notify('Scene.dataResponse', {
          data: (await downloadManager.getParcelDataBySceneId(data.sceneId)) as ILand
        })
      )

      connector.on('Scene.prefetchDone', (opt: { sceneId: string }) => {
        sceneController.reportDataLoaded(opt.sceneId)
      })

      connector.on('Scene.status', (data: SceneLifeCycleStatusReport) => {
        sceneController.reportStatus(data.sceneId, data.status)
      })
    }
  )
}
