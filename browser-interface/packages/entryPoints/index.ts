import { isInsideWorldLimits } from '@dcl/schemas'
import { ETHEREUM_NETWORK, HAS_INITIAL_POSITION_MARK } from 'config/index'
import { WebSocketProvider } from 'eth-connect'
import { IDecentralandKernel, IEthereumProvider, KernelOptions, KernelResult, LoginState } from 'kernel-web-interface'
import { getFromPersistentStorage, setPersistentStorage } from 'lib/browser/persistentStorage'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { resolveBaseUrl } from 'lib/decentraland/url/resolveBaseUrl'
import { storeCondition } from 'lib/redux/storeCondition'
import { initShared } from 'shared'
import { setupTracing } from 'shared/analytics/trace'
import { sendHomeScene } from 'shared/atlas/actions'
import { homePointKey } from 'shared/atlas/utils'
import { BringDownClientAndReportFatalError, ErrorContext } from 'shared/loading/ReportFatalError'
import { setResourcesURL } from 'shared/location'
import { globalObservable } from 'shared/observables'
import { localProfilesRepo } from 'shared/profiles/sagas/local/localProfilesRepo'
import { teleportToAction } from 'shared/scene-loader/actions'
import { retrieveLastSessionByAddress } from 'shared/session'
import { authenticate, initSession } from 'shared/session/actions'
import { store } from 'shared/store/isolatedStore'
import type { RootState } from 'shared/store/rootTypes'
import { initializeUnity } from 'unity-interface/initializer'
import Hls from './hlsLoader'
import { loadWebsiteSystems } from './loadWebsiteSystems'
import { isWebGLCompatible } from './validations'

/**
 * `DecentralandKernel` needs to be global, it's how `exporer-website` calls the initialization
 */
declare const globalThis: { DecentralandKernel: IDecentralandKernel }
globalThis.DecentralandKernel = {
  async initKernel(options: KernelOptions): Promise<KernelResult> {
    ensureWebGLCapability()
    ensureHLSCapability()
    setupTracing()

    await setupBaseUrl(options)

    ensureValidWebGLCanvasContainer(options)

    configurePersistentLocalStorage(options)

    initShared()

    executeOnNextTick(async () => {
      await setupHomeAndInitialTeleport()
      store.dispatch(initSession())

      await Promise.all([initializeUnity(options.rendererOptions), loadWebsiteSystems(options.kernelOptions)])
    })

    return {
      authenticate: authenticateCallback,
      on: globalObservable.on.bind(globalObservable),
      version: 'mockedversion',
      hasStoredSession
    }
  }
}

function ensureHLSCapability() {
  if (!Hls || !Hls.isSupported) {
    throw new Error('HTTP Live Streaming did not load')
  }

  if (!Hls.isSupported()) {
    throw new Error('HTTP Live Streaming is not supported in your browser')
  }
}

function ensureWebGLCapability() {
  if (!isWebGLCompatible()) {
    throw new Error(
      "A WebGL2 could not be created. It is necessary to make Decentraland run, your browser may not be compatible. This error may also happen when many tabs are open and the browser doesn't have enough resources available to start Decentraland, if that's the case, please try closing other tabs and specially other Decentraland instances."
    )
  }
}

async function setupBaseUrl(options: KernelOptions) {
  options.kernelOptions.baseUrl = await resolveBaseUrl(
    options.kernelOptions.baseUrl || orFail('MISSING kernelOptions.baseUrl')
  )
  options.rendererOptions.baseUrl = await resolveBaseUrl(
    options.rendererOptions.baseUrl || orFail('MISSING rendererOptions.baseUrl')
  )

  const { baseUrl } = options.kernelOptions

  if (baseUrl) {
    setResourcesURL(baseUrl)
  }
}

function ensureValidWebGLCanvasContainer(options: KernelOptions) {
  const { container } = options.rendererOptions
  if (!container) {
    throw new Error('cannot find element #gameContainer')
  }
}

function configurePersistentLocalStorage(options: KernelOptions) {
  if (options.kernelOptions.persistentStorage) {
    setPersistentStorage(options.kernelOptions.persistentStorage)
  }
}

function authenticateCallback(provider: any, isGuest: boolean) {
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
}

async function hasStoredSession(address: string, networkId: number) {
  const existingSession = await retrieveLastSessionByAddress(address)
  if (!existingSession) {
    return { result: false }
  }

  const profile = await localProfilesRepo.get(
    address,
    networkId === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.GOERLI
  )

  return { result: !!profile, profile: profile || null } as any
}

async function setupHomeAndInitialTeleport() {
  const homePoint: string = await getFromPersistentStorage(homePointKey)
  if (homePoint) {
    store.dispatch(sendHomeScene(homePoint))
  }

  const urlPosition = getInitialPositionFromUrl(location.search)

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

function getInitialPositionFromUrl(): ReadOnlyVector2 | undefined {
  const query = new URLSearchParams(location.search)
  const position = query.get('position')
  if (typeof position === 'string') {
    const { x, y } = parseParcelPosition(position)
    if (isInsideWorldLimits(x, y)) {
      return { x, y }
    }
  }
}

function executeOnNextTick(task: () => Promise<any>) {
  setTimeout(async () => {
    try {
      await task()
    } catch (err: any) {
      BringDownClientAndReportFatalError(err, ErrorContext.WEBSITE_INIT)
    }
  }, 0)
}
