import Hls from './hlsLoader'
import { isWebGLCompatible } from './validations'
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
import { globalObservable } from 'shared/observables'
import { teleportToAction } from 'shared/scene-loader/actions'
import { retrieveLastSessionByAddress } from 'shared/session'
import { authenticate, initSession } from 'shared/session/actions'
import { store } from 'shared/store/isolatedStore'
import type { RootState } from 'shared/store/rootTypes'
import { getInitialPositionFromUrl } from 'shared/world/positionThings'
import { loadWebsiteSystems } from './loadWebsiteSystems'
import { localProfilesRepo } from 'shared/profiles/sagas/local/localProfilesRepo'
import { startInjectionofUnityToDOM } from 'unity-interface/dom/startInjectionOfUnityToDOM'

declare const globalThis: { DecentralandKernel: IDecentralandKernel }

globalThis.DecentralandKernel = {
  async initKernel(options: KernelOptions): Promise<KernelResult> {
    ensureWebGL()
    ensureHls()

    await setupBaseUrls(options)
    checkRendererElementPresent(options)

    // initShared must be called immediately, before return
    initShared()

    // initInternal must be called asynchronously, _after_ returning
    setTimeout(
      () => initInternal(options).catch((err) => BringDownClientAndReportFatalError(err, ErrorContext.WEBSITE_INIT)),
      0
    )

    return {
      authenticate: authenticateUser,
      on: globalObservable.on.bind(globalObservable),
      version: 'mockedversion',
      hasStoredSession
    }
  }
}

function checkRendererElementPresent(options: KernelOptions) {
  const { container } = options.rendererOptions
  if (!container) {
    throw new Error('cannot find element #gameContainer')
  }
}

function ensureWebGL() {
  if (!isWebGLCompatible()) {
    throw new Error(
      "A WebGL2 could not be created. It is necessary to make Decentraland run, your browser may not be compatible. This error may also happen when many tabs are open and the browser doesn't have enough resources available to start Decentraland, if that's the case, please try closing other tabs and specially other Decentraland instances."
    )
  }
}

function ensureHls() {
  if (!Hls || !Hls.isSupported) {
    throw new Error('HTTP Live Streaming did not load')
  }

  if (!Hls.isSupported()) {
    throw new Error('HTTP Live Streaming is not supported in your browser')
  }
}

async function setupBaseUrls(options: KernelOptions) {
  options.kernelOptions.baseUrl = await resolveBaseUrl(
    options.kernelOptions.baseUrl || orFail('MISSING kernelOptions.baseUrl')
  )
  options.rendererOptions.baseUrl = await resolveBaseUrl(
    options.rendererOptions.baseUrl || orFail('MISSING rendererOptions.baseUrl')
  )

  if (options.kernelOptions.persistentStorage) {
    setPersistentStorage(options.kernelOptions.persistentStorage)
  }
}

async function initInternal(options: KernelOptions) {
  // load homepoint
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

  // Initializes the Session Saga
  store.dispatch(initSession())

  await Promise.all([startInjectionofUnityToDOM(options.rendererOptions), loadWebsiteSystems(options.kernelOptions)])
}

function authenticateUser(provider: any, isGuest: boolean) {
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

// this method is used for auto-login
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
