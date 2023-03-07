import { storeCondition } from 'lib/redux/storeCondition'
import { initializeRenderer } from 'shared/renderer/actions'
import { KernelOptions } from 'kernel-web-interface'
import { store } from 'shared/store/isolatedStore'
import { browserInterface } from '../BrowserInterface'
import { isRendererInitialized } from 'shared/renderer/selectors'
import { rendererOptions, loadWsEditorDelegate, loadInjectedUnityDelegate } from './initializer'
import { InitializeUnityResult } from './types'

/** Initialize the injected engine in a container */

export async function startInjectionofUnityToDOM(
  options: KernelOptions['rendererOptions']
): Promise<InitializeUnityResult> {
  const queryParams = new URLSearchParams(document.location.search)

  Object.assign(rendererOptions, options)
  const { container } = rendererOptions

  if (queryParams.has('ws')) {
    // load unity renderer using WebSocket
    store.dispatch(initializeRenderer(loadWsEditorDelegate, container))
    browserInterface.onUserInteraction.resolve()
  } else {
    // load injected renderer
    store.dispatch(initializeRenderer(loadInjectedUnityDelegate, container))
  }

  // wait until the renderer is fully loaded before returning, this
  // is important because once this function returns, it is assumed
  // that the renderer will be ready
  await storeCondition(isRendererInitialized)

  return {
    container
  }
}
