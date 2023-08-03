import future from 'fp-future'
import { trackEvent } from 'shared/analytics/trackEvent'
import { ActiveVideoStreams } from 'shared/comms/adapters/types'
import { getLivekitActiveVideoStreams } from 'shared/comms/selectors'
import { BringDownClientAndShowError } from 'shared/loading/ReportFatalError'
import { store } from 'shared/store/isolatedStore'

const generatedFiles = {
  frameworkUrl: 'unity.framework.js',
  dataUrl: 'unity.data',
  codeUrl: 'unity.wasm'
}

export type LoadRendererResult = {
  createWebRenderer(canvas: HTMLCanvasElement): Promise<DecentralandRendererInstance>
}

// the following function is defined by unity, accessible via unity.loader.js
// https://docs.unity3d.com/Manual/webgl-templates.html
// prettier-ignore
declare function createUnityInstance(canvas: HTMLCanvasElement, config: any, onProgress?: (progress: number) => void, onSuccess?: (unityInstance: any) => void, onError?: (message: any) => void): Promise<UnityGame>

/** Expose the original interface from the Unity Instance. */
export type UnityGame = {
  Module: {
    /** this handler can be overwritten, return true to stop error propagation */
    errorHandler?: (message: string, filename: string, lineno: number) => boolean
  }
  SendMessage(object: string, method: string, args: number | string): void
  SetFullscreen(): void
  Quit(): Promise<void>
}

export type RendererOptions = {
  canvas: HTMLCanvasElement

  onProgress?: (progress: number) => void
  onSuccess?: (unityInstance: any) => void
  onError?: (error: any) => void
  /** Legacy messaging system */
  onMessageLegacy: (type: string, payload: string) => void
  /** scene binary messaging system */
  onBinaryMessage?: (data: Uint8Array) => void
  /** used to append a ?v={} to the URL. Useful to debug cache issues */
  versionQueryParam?: string
  /** baseUrl where all the assets are deployed */
  baseUrl: string

  enableBrotli?: boolean

  /** Extra config passed on to unity module */
  extraConfig?: any

  /** Prevents the renderer on initialization from failing if it detects a mobile browser */
  dontCheckMobile?: boolean
}

export type DecentralandRendererInstance = {
  /**
   * Signal sent by unity after it started correctly
   * it is a promise, that makes it awaitable.
   * The content of the resolved promise is an empty object to
   * enable future extensions.
   */
  engineStartedFuture: Promise<any>

  // soon there will be more protocol functions here https://github.com/decentraland/renderer-protocol
  // and originalUnity will be deprecated to decouple the kernel from unity's impl internals
  originalUnity: UnityGame
}

/**
 * The following options are common to all kinds of renderers, it abstracts
 * what we need to implement in our end to support a renderer. WIP
 */
export type CommonRendererOptions = {
  onMessage: (type: string, payload: string) => void
}

type EngineRequestsNames = keyof IEngineRequests

interface IEngineRequests {
  livekitVideoTrack: { videoTrackSrc: string }
}

function extractSemver(url: string): string | null {
  const r = url.match(/([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?/)

  if (r) {
    return r[0]
  }

  return null
}

async function initializeWebRenderer(options: RendererOptions): Promise<DecentralandRendererInstance> {
  const explorerVersion = options.versionQueryParam
  const { canvas, baseUrl, onProgress, onSuccess, onError, onMessageLegacy, onBinaryMessage } = options
  const resolveWithBaseUrl = (file: string) =>
    new URL(file + (explorerVersion ? '?v=' + explorerVersion : ''), baseUrl).toString()

  // only assets deployed to DCL's CDN, the @dcl/cdn-uploader takes care of that compression
  // this optimization is important, please do not remove
  const enableBrotli =
    baseUrl.startsWith('https://renderer-artifacts.decentraland.org') ||
    baseUrl.startsWith('https://cdn.decentraland.org') ||
    false

  const postfix = enableBrotli ? '.br' : ''

  const config = {
    dataUrl: resolveWithBaseUrl(generatedFiles.dataUrl + postfix),
    frameworkUrl: resolveWithBaseUrl(generatedFiles.frameworkUrl + postfix),
    codeUrl: resolveWithBaseUrl(generatedFiles.codeUrl + postfix),
    streamingAssetsUrl: new URL('StreamingAssets', baseUrl).toString(),
    companyName: 'Decentraland',
    productName: 'Decentraland World Client',
    productVersion: '0.1',
    ...(options.extraConfig || {})
  }

  const engineStartedFuture = future<any>()

  // The namespace DCL is exposed to global because the unity template uses it to send the messages
  // @see https://github.com/decentraland/unity-renderer/blob/bc2bf1ee0d685132c85606055e592bac038b3471/unity-renderer/Assets/Plugins/JSFunctions.jslib#L6-L29
  ;(globalThis as any).DCL = {
    // This function get's called by the engine
    EngineStarted() {
      engineStartedFuture.resolve({})
    },

    // This function is called from the unity renderer to send messages back to the scenes
    MessageFromEngine(type: string, jsonEncodedMessage: string) {
      onMessageLegacy(type, jsonEncodedMessage)
    },

    // This function is called from the unity renderer to send messages back to the scenes
    BinaryMessageFromEngine(data: Uint8Array) {
      if (!!onBinaryMessage) onBinaryMessage(data)
    },

    RequestFromEngine(request: { type: string, payload: any }) {
      const requestType = request.type as EngineRequestsNames
      if (requestType === 'livekitVideoTrack') {
        const split = (request.payload as IEngineRequests['livekitVideoTrack']).videoTrackSrc.split('/')
        if (split.length < 2)
          return undefined

        const videoSid = split[split.length - 1]
        const participantSid = split[split.length - 2]
        const activeVideoStreams: Map<string, ActiveVideoStreams> | undefined = getLivekitActiveVideoStreams(store.getState())
        return activeVideoStreams?.get(participantSid)?.videoTracks.get(videoSid)
      }
    }
  }

  const originalUnity = await createUnityInstance(canvas, config, onProgress, onSuccess, onError)

  // TODO: replace originalUnity.errorHandler with a version that redirects errors to -> onError

  return {
    engineStartedFuture,
    originalUnity
  }
}

export async function loadUnity(baseUrl: string, options: CommonRendererOptions): Promise<LoadRendererResult> {
  const explorerVersion = extractSemver(baseUrl) || 'dynamic'

  let startTime = performance.now()

  trackEvent('unity_loader_downloading_start', { renderer_version: explorerVersion })
  trackEvent('unity_loader_downloading_end', {
    renderer_version: explorerVersion,
    loading_time: performance.now() - startTime
  })

  return {
    createWebRenderer: async (canvas) => {
      let didLoadUnity = false

      startTime = performance.now()
      trackEvent('unity_downloading_start', { renderer_version: explorerVersion })

      function onProgress(progress: number) {
        // 0.9 is harcoded in unityLoader, it marks the download-complete event
        if (0.9 === progress && !didLoadUnity) {
          trackEvent('unity_downloading_end', {
            renderer_version: explorerVersion,
            loading_time: performance.now() - startTime
          })

          startTime = performance.now()
          trackEvent('unity_initializing_start', { renderer_version: explorerVersion })
          didLoadUnity = true
        }
        // 1.0 marks the engine-initialized event
        if (1.0 === progress) {
          trackEvent('unity_initializing_end', {
            renderer_version: explorerVersion,
            loading_time: performance.now() - startTime
          })
        }
      }

      return initializeWebRenderer({
        baseUrl,
        canvas,
        versionQueryParam: explorerVersion === 'dynamic' ? Date.now().toString() : explorerVersion,
        onProgress,
        onMessageLegacy: options.onMessage,
        onError: (error) => {
          BringDownClientAndShowError(error)
        },
        onBinaryMessage: (...args) => {
          console.log('onBinaryMessage', ...args)
        },
        extraConfig: {
          antialias: false,
          powerPreference: 'high-performance',
          failIfMajorPerformanceCaveat: true
        }
      })
    }
  }
}
