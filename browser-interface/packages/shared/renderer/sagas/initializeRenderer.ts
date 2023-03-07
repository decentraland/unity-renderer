import { RpcClient, RpcClientPort, Transport } from '@dcl/rpc'
import type { UnityGame } from '@dcl/unity-renderer/src/index'
import { ENGINE_DEBUG_PANEL, isRunningTest, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER, TRACE_RENDERER } from 'config'
import { call, put, select, takeLatest } from 'redux-saga/effects'
import { createRendererRpcClient } from 'renderer-protocol/rpcClient'
import { beginTrace, endTrace, traceDecoratorUnityGame } from 'shared/analytics/trace'
import { trackEvent } from 'shared/analytics/trackEvent'
import { waitingForRenderer } from 'shared/loading/types'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { sendProfileToRenderer } from 'shared/profiles/actions'
import { SignUpSetIsSignUp, SIGNUP_SET_IS_SIGNUP } from 'shared/session/actions'
import { getCurrentUserId } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { allScenesEvent } from 'shared/world/parcelSceneManager'
import { browserInterface } from 'unity-interface/BrowserInterface'
import { startGlobalScene } from 'unity-interface/initialScenes/startGlobalScene'
import { getUnityInterface } from 'unity-interface/IUnityInterface'
import { initializeUnityInterface } from 'unity-interface/UnityInterface'
import { kernelConfigForRenderer } from 'unity-interface/kernelConfigForRenderer'
import { InitializeRenderer, registerRendererPort, signalRendererInitializedCorrectly } from '../actions'
import { waitForRendererInstance } from '../sagas-helper'

export function* initializeRenderer(action: InitializeRenderer) {
  const { delegate, container } = action.payload

  // will start loading
  yield put(waitingForRenderer())
  yield takeLatest(SIGNUP_SET_IS_SIGNUP, sendSignUpToRenderer)

  // start loading the renderer
  try {
    const { renderer, transport }: { renderer: UnityGame; transport: Transport } = yield call(delegate, container)

    const startTime = performance.now()

    trackEvent('renderer_initializing_start', {})

    // register the RPC port
    const rpcHandles: {
      rpcClient: RpcClient
      rendererInterfacePort: RpcClientPort
    } = yield call(createRendererRpcClient, transport)
    yield put(registerRendererPort(rpcHandles.rpcClient, rpcHandles.rendererInterfacePort))

    if (isRunningTest) {
      return
    }
    yield call(initializeEngine, renderer)
    yield call(waitForRendererInstance)
    trackEvent('renderer_initializing_end', {
      loading_time: performance.now() - startTime
    })
  } catch (e) {
    trackEvent('renderer_initialization_error', {
      message: e + ''
    })
    if (e instanceof Error) {
      throw e
    } else {
      throw new Error('Error initializing rendering')
    }
  }
}

function* sendSignUpToRenderer(action: SignUpSetIsSignUp) {
  if (action.payload.isSignUp) {
    getUnityInterface().ShowAvatarEditorInSignIn()

    const userId: string = yield select(getCurrentUserId)
    yield put(sendProfileToRenderer(userId))
  }
}

// eslint-disable-next-line @typescript-eslint/no-var-requires
const avatarSceneRaw = require('../../../../static/systems/decentraland-ui.scene.js.txt')
const avatarSceneBlob = new Blob([avatarSceneRaw])
const avatarSceneUrl = URL.createObjectURL(avatarSceneBlob)

async function initializeEngine(_gameInstance: UnityGame): Promise<void> {
  initializeTracing()
  const unityGameInstance = traceDecoratorUnityGame(_gameInstance)

  await initializeUnityInterface()
  getUnityInterface().Init(unityGameInstance)
  configurePointerLock()

  await browserInterface.startedFuture

  queueMicrotask(() => {
    // send an "engineStarted" notification, use a queueMicrotask
    // to escape the current stack leveraging the JS event loop
    store.dispatch(signalRendererInitializedCorrectly())
  })

  await ensureMetaConfigurationInitialized()

  getUnityInterface().SetKernelConfiguration(kernelConfigForRenderer())

  if (SCENE_DEBUG_PANEL) {
    getUnityInterface().SetKernelConfiguration({ debugConfig: { sceneDebugPanelEnabled: true } })
    getUnityInterface().ShowFPSPanel()
  }

  if (SHOW_FPS_COUNTER) {
    getUnityInterface().ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    getUnityInterface().SetEngineDebugPanel()
  }

  await startGlobalScene('dcl-gs-avatars', 'Avatars', avatarSceneUrl)
}

function configurePointerLock() {
  // TODO: move to unity-renderer
  let isPointerLocked: boolean = false

  function pointerLockChange() {
    const doc: any = document
    const isLocked = !!(doc.pointerLockElement || doc.mozPointerLockElement || doc.webkitPointerLockElement)
    if (isPointerLocked !== isLocked && getUnityInterface()) {
      getUnityInterface().SetCursorState(isLocked)
    }
    isPointerLocked = isLocked
    allScenesEvent({
      eventType: 'onPointerLock',
      payload: {
        locked: isPointerLocked
      }
    })
  }

  document.addEventListener('pointerlockchange', pointerLockChange, false)
}

function initializeTracing() {
  ;(globalThis as any).beginTrace = beginTrace
  ;(globalThis as any).endTrace = endTrace

  const parametricTrace = parseInt(TRACE_RENDERER || '0', 10)
  if (!isNaN(parametricTrace) && parametricTrace > 0) {
    beginTrace(parametricTrace)
  }
}
