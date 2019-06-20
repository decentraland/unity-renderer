import { DEBUG, ENGINE_DEBUG_PANEL, SCENE_DEBUG_PANEL, parcelLimits } from 'config'
import { initShared } from 'shared'
import { ILandToLoadableParcelScene } from 'shared/types'
import { lastPlayerPosition, getWorldSpawnpoint, teleportObservable } from 'shared/world/positionThings'
import { enableParcelSceneLoading } from 'shared/world/parcelSceneManager'
import { chatObservable } from 'shared/comms/chat'
import { queueTrackingEvent } from 'shared/analytics'
import EngineInterface from './EngineInterface'
import { UnityGame } from './types'
import { getUnityClass } from './unityParcelScene'
import browserInterface from './browserInterface'
import initializeDecentralandUI from './initializeDecentralandUI'

declare var window: Window & {
  unityInterface?: any
  messages?: (e: any) => any
}

export async function initializeEngine(gameInstance: UnityGame) {
  const unityInterface = new EngineInterface(gameInstance)
  window.unityInterface = unityInterface

  const net = await initShared()
  unityInterface.SetPosition(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerPosition.z)

  teleportObservable.add((position: { x: number; y: number }) => {
    unityInterface.SetPosition(position.x * parcelLimits.parcelSize, 0, position.y * parcelLimits.parcelSize)
  })

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }

  await initializeDecentralandUI(unityInterface)

  await enableParcelSceneLoading(net, {
    parcelSceneClass: getUnityClass(unityInterface),
    shouldLoadParcelScene: () => {
      return true
      // TODO integrate with unity the preloading feature
      // tslint:disable-next-line: no-commented-out-code
      // return preloadedScenes.has(land.scene.scene.base)
    },
    onSpawnpoint: initialLand => {
      const newPosition = getWorldSpawnpoint(initialLand)
      unityInterface.SetPosition(newPosition.x, newPosition.y, newPosition.z)
      queueTrackingEvent('Scene Spawn', { parcel: initialLand.scene.scene.base, spawnpoint: newPosition })
    },
    onLoadParcelScenes: lands => {
      unityInterface.LoadParcelScenes(
        lands.map($ => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    }
  })

  return {
    net,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        // tslint:disable-next-line:no-console
        console.log('MessageFromEngine', type, message)
      }
    }
  }
}

window.messages = (e: any) => chatObservable.notifyObservers(e)
