import Hls from "./hlsLoader"
import { isWebGLCompatible } from "./validations"

declare const globalThis: { DecentralandKernel: IDecentralandKernel }

import { IDecentralandKernel, IEthereumProvider, KernelOptions, KernelResult, LoginState } from '@dcl/kernel-interface'
import { sdk } from '@dcl/schemas'
import { resolveUrlFromUrn } from '@dcl/urn-resolver'
import { getFromPersistentStorage, setPersistentStorage } from 'atomicHelpers/persistentStorage'
import { WebSocketProvider } from 'eth-connect'
import { storeCondition } from "lib/redux/storeCondition"
import { initShared } from 'shared'
import { BringDownClientAndReportFatalError, ErrorContext } from 'shared/loading/ReportFatalError'
import { setResourcesURL } from 'shared/location'
import { createLogger } from 'shared/logger'
import { globalObservable } from 'shared/observables'
import { localProfilesRepo } from 'shared/profiles/sagas'
import { teleportToAction } from 'shared/scene-loader/actions'
import { getStoredSession } from 'shared/session'
import { authenticate, initSession } from 'shared/session/actions'
import { store } from 'shared/store/isolatedStore'
import { RootState } from "shared/store/rootTypes"
import { getInitialPositionFromUrl } from 'shared/world/positionThings'
import { clientDebug } from 'unity-interface/ClientDebug'
import { IUnityInterface } from 'unity-interface/IUnityInterface'
import 'unity-interface/trace'
import { gridToWorld, parseParcelPosition } from '../atomicHelpers/parcelScenePositions'
import {
  DEBUG_WS_MESSAGES,
  ETHEREUM_NETWORK,
  HAS_INITIAL_POSITION_MARK
} from '../config/index'
import { sendHomeScene } from '../shared/atlas/actions'
import { homePointKey } from '../shared/atlas/utils'
import { getPreviewSceneId, loadPreviewScene, reloadPlaygroundScene } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { loadWebsiteSystems } from "./loadWebsiteSystems"

const logger = createLogger('kernel: ')

async function resolveBaseUrl(urn: string): Promise<string> {
  if (urn.startsWith('urn:')) {
    const t = await resolveUrlFromUrn(urn)
    if (t) {
      return (t + '/').replace(/(\/)+$/, '/')
    }
    throw new Error('Cannot resolve content for URN ' + urn)
  }
  return (urn + '/').replace(/(\/)+$/, '/')
}

function orFail(withError: string): never {
  throw new Error(withError)
}

function isLoginStateWaitingForProvider(state: RootState) {
  return state.session.loginState === LoginState.WAITING_PROVIDER
}
async function authenticateWhenItsReady(provider: IEthereumProvider, isGuest: boolean) {
  await storeCondition(isLoginStateWaitingForProvider)
  store.dispatch(authenticate(provider, isGuest))
}

globalThis.DecentralandKernel = {
  async initKernel(options: KernelOptions): Promise<KernelResult> {


    if (!isWebGLCompatible()) {
      throw new Error(
        "A WebGL2 could not be created. It is necessary to make Decentraland run, your browser may not be compatible. This error may also happen when many tabs are open and the browser doesn't have enough resources available to start Decentraland, if that's the case, please try closing other tabs and specially other Decentraland instances."
      )
    }

    if (!Hls || !Hls.isSupported) {
      throw new Error("HTTP Live Streaming did not load")
    }

    if (!Hls.isSupported()) {
      throw new Error("HTTP Live Streaming is not supported in your browser")
    }


    options.kernelOptions.baseUrl = await resolveBaseUrl(
      options.kernelOptions.baseUrl || orFail('MISSING kernelOptions.baseUrl')
    )
    options.rendererOptions.baseUrl = await resolveBaseUrl(
      options.rendererOptions.baseUrl || orFail('MISSING rendererOptions.baseUrl')
    )

    if (options.kernelOptions.persistentStorage) {
      setPersistentStorage(options.kernelOptions.persistentStorage)
    }

    const { container } = options.rendererOptions
    const { baseUrl } = options.kernelOptions

    if (baseUrl) {
      setResourcesURL(baseUrl)
    }

    if (!container) throw new Error('cannot find element #gameContainer')

    // initShared must be called immediately, before return
    initShared()

    // initInternal must be called asynchronously, _after_ returning
    async function initInternal() {
      // load homepoint
      const homePoint: string = await getFromPersistentStorage(homePointKey)
      if (homePoint) {
        store.dispatch(sendHomeScene(homePoint))
      }

      const urlPosition = getInitialPositionFromUrl()

      // teleport to initial location
      if (urlPosition) {
        // 1. by URL
        const { x, y } = urlPosition
        store.dispatch(teleportToAction({ position: gridToWorld(x, y) }))
      } else if (homePoint && !HAS_INITIAL_POSITION_MARK) {
        // 2. by homepoint
        const { x, y } = parseParcelPosition(homePoint)
        store.dispatch(teleportToAction({ position: gridToWorld(x, y) }))
      } else {
        // 3. fallback to 0,0
        const { x, y } = { x: 0, y: 0 } // '0,0'
        store.dispatch(teleportToAction({ position: gridToWorld(x, y) }))
      }

      // Initializes the Session Saga
      store.dispatch(initSession())

      await initializeUnity(options.rendererOptions)
      await loadWebsiteSystems(options.kernelOptions)
    }

    setTimeout(
      () =>
        initInternal().catch((err) => {
          BringDownClientAndReportFatalError(err, ErrorContext.WEBSITE_INIT)
        }),
      0
    )

    return {
      authenticate(provider: any, isGuest: boolean) {
        if (!provider) {
          throw new Error('A provider must be provided')
        }
        if (typeof provider === 'string') {
          if (provider.startsWith('ws:') || provider.startsWith('wss:')) {
            provider = new WebSocketProvider(provider)
          } else {
            throw new Error('Text provider can only be WebSocket')
          }
        }
        authenticateWhenItsReady(provider, isGuest)
      },
      on: globalObservable.on.bind(globalObservable),
      version: 'mockedversion',
      // this method is used for auto-login
      async hasStoredSession(address: string, networkId: number) {
        if (!(await getStoredSession(address))) return { result: false }

        const profile = await localProfilesRepo.get(
          address,
          networkId === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.GOERLI
        )

        return { result: !!profile, profile: profile || null } as any
      }
    }
  }
}

export async function startPreview(unityInterface: IUnityInterface) {
  getPreviewSceneId()
    .then((sceneData) => {
      if (sceneData.sceneId) {
        unityInterface.SetKernelConfiguration({
          debugConfig: {
            sceneDebugPanelTargetSceneId: sceneData.sceneId,
            sceneLimitsWarningSceneId: sceneData.sceneId
          }
        })
        clientDebug.ToggleSceneBoundingBoxes(sceneData.sceneId, false).catch((e) => logger.error(e))
        unityInterface.SendMessageToUnity('Main', 'TogglePreviewMenu', JSON.stringify({ enabled: true }))
      }
    })
    .catch((_err) => {
      logger.info('Warning: cannot get preview scene id')
    })

  function handleServerMessage(message: sdk.Messages) {
    if (DEBUG_WS_MESSAGES) {
      logger.info('Message received: ', message)
    }
    if (message.type === sdk.UPDATE || message.type === sdk.SCENE_UPDATE) {
      void loadPreviewScene(message)
    }
  }

  const ws = new WebSocket(`${location.protocol === 'https:' ? 'wss' : 'ws'}://${document.location.host}`)

  ws.addEventListener('message', (msg) => {
    if (msg.data.startsWith('{')) {
      logger.log('Update message from CLI', msg.data)
      const message: sdk.Messages = JSON.parse(msg.data)
      handleServerMessage(message)
    }
  })

  window.addEventListener('message', async (msg) => {
    if (typeof msg.data === 'string') {
      if (msg.data === 'sdk-playground-update') {
        await reloadPlaygroundScene()
      }
    }
  })
}
