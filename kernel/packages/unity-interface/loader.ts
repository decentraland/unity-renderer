import future from 'fp-future'
import { parseUrn } from '@dcl/urn-resolver'

declare const globalThis: any

export type DclRenderer = {}
export type UnityLoader = {}
export type LoadRendererResult = {
  DclRenderer: DclRenderer
  UnityLoader: UnityLoader
  buildConfigPath: string
}

/**
 * This function is used by preview, instead of using "." as root,
 * preview uses '/@/artifacts'
 */
function getRendererArtifactsRoot() {
  if (typeof globalThis.RENDERER_ARTIFACTS_ROOT == 'undefined') {
    throw new Error('RENDERER_ARTIFACTS_ROOT is undefined')
  } else {
    return new URL(globalThis.RENDERER_ARTIFACTS_ROOT, document.location.toString()).toString()
  }
}

// TODO: return type DclRenderer
async function injectRenderer(baseUrl: string): Promise<LoadRendererResult> {
  const scriptUrl = new URL('index.js', baseUrl).toString()
  const buildConfigPath = new URL('unity.json', baseUrl).toString()
  await injectScript(scriptUrl)

  if (typeof globalThis.UnityLoader == 'undefined') {
    throw new Error('Error while loading UnityLoader from ' + scriptUrl)
  }

  if (typeof globalThis.DclRenderer == 'undefined') {
    throw new Error('Error while loading the renderer from ' + scriptUrl)
  }

  return {
    DclRenderer: globalThis.DclRenderer,
    UnityLoader: globalThis.UnityLoader,
    buildConfigPath
  }
}
// TODO: return type DclRenderer
async function loadDefaultRenderer(): Promise<LoadRendererResult> {
  // TODO: after we remove unity from this folder, remove the following
  // block and uncomment the block next to it
  {
    const scriptUrl = new URL('DCLUnityLoader.js', getRendererArtifactsRoot()).toString()
    const buildConfigPath = new URL('unity.json', getRendererArtifactsRoot()).toString()
    await injectScript(scriptUrl)
    if (typeof globalThis.UnityLoader == 'undefined') {
      throw new Error('Error while loading the renderer from ' + scriptUrl)
    }
    return {
      DclRenderer: globalThis.DclRenderer,
      UnityLoader: globalThis.UnityLoader,
      buildConfigPath
    }
  }

  // {
  //   // Load the renderer from the artifacts root folder
  //   return await injectRenderer(getRendererArtifactsRoot())
  // }
}

async function loadRendererByBranch(branch: string): Promise<LoadRendererResult> {
  const baseUrl = `https://renderer-artifacts.decentraland.org/branch/${branch}/`
  return await injectRenderer(baseUrl)
}

// TODO: return type DclRenderer
export async function loadUnity(urn?: string): Promise<LoadRendererResult> {
  if (!urn) {
    return loadDefaultRenderer()
  } else {
    const parsedUrn = await parseUrn(urn)

    if (!parsedUrn) {
      throw new Error('An invalid urn was provided for the renderer')
    }

    // urn:decentraland:off-chain:renderer-artifacts:${branch}
    if (parsedUrn.type == 'off-chain' && parsedUrn.registry == 'renderer-artifacts') {
      return loadRendererByBranch(parsedUrn.id)
    }

    throw new Error('It was impossible to resolve a renderer for the URN "' + urn + '"')
  }
}

async function injectScript(url: string) {
  const theFuture = future<Event>()
  const theScript = document.createElement('script')
  theScript.src = url
  theScript.async = true
  theScript.type = 'application/javascript'
  theScript.addEventListener('load', theFuture.resolve)
  theScript.addEventListener('error', (e) => theFuture.reject(e.error || (e as any)))
  document.body.appendChild(theScript)
  return theFuture
}
