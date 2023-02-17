import Hls from './hlsLoader'
import { isWebGLCompatible } from './validations'

declare const globalThis: { DecentralandKernel: IDecentralandKernel }

import { IDecentralandKernel, IEthereumProvider, KernelOptions, KernelResult, LoginState } from 'kernel-web-interface'
import { resolveBaseUrl } from 'lib/decentraland/url/resolveBaseUrl'
import { ETHEREUM_NETWORK, HAS_INITIAL_POSITION_MARK } from 'config/index'
import { WebSocketProvider } from 'eth-connect'
import { getFromPersistentStorage, setPersistentStorage } from 'lib/browser/persistentStorage'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { storeCondition } from 'lib/redux/storeCondition'
import { initShared } from 'shared'
import { sendHomeScene } from 'shared/atlas/actions'
import { homePointKey } from 'shared/atlas/utils'
import { BringDownClientAndReportFatalError, ErrorContext } from 'shared/loading/ReportFatalError'
import { setResourcesURL } from 'shared/location'
import { globalObservable } from 'shared/observables'
import { teleportToAction } from 'shared/scene-loader/actions'
import { getStoredSession } from 'shared/session'
import { authenticate, initSession } from 'shared/session/actions'
import { store } from 'shared/store/isolatedStore'
import type { RootState } from 'shared/store/rootTypes'
import { getInitialPositionFromUrl } from 'shared/world/positionThings'
import { initializeUnity } from 'unity-interface/initializer'
import 'unity-interface/trace'
import { loadWebsiteSystems } from './loadWebsiteSystems'
import { localProfilesRepo } from 'shared/profiles/sagas/local/localProfilesRepo'

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
      throw new Error('HTTP Live Streaming did not load')
    }

    if (!Hls.isSupported()) {
      // Hotfix:
      // The current hypothesis is that this is a race condition generated by the merge of browser and renderer.
      // Judging by Rollbar metrics, the issue was not in production two days ago -- for the time being, this is
      // making the VR client packed by Ong unusable for 100% of their users.
      // See also:
      //  - https://rollbar.com/decentraland/World/items/73491/
      // TODO: Gracefully degrade if Hls is not supported, or add a URL parameter
      // was:
      //     throw new Error('HTTP Live Streaming is not supported in your browser')
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

      await Promise.all([
        initializeUnity(options.rendererOptions),
        loadWebsiteSystems(options.kernelOptions)
      ])
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
        return authenticateWhenItsReady(provider, isGuest)
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
