// This compilated file is appended to the unity.loader.js. You can
// assume everything from the loader will be available here

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
  /** used to append a ?v={} to the URL. Useful to debug cache issues */
  versionQueryParam?: string
  /** baseUrl where all the assets are deployed */
  baseUrl: string
}

export type DecentralandRendererInstance = {
  // soon there will be more protocol functions here
  // https://github.com/decentraland/renderer-protocol

  // and originalUnity will be deprecated to abstract the kernel from unity
  originalUnity: UnityGame
}

export async function initializeWebRenderer(options: RendererOptions): Promise<DecentralandRendererInstance> {
  const rendererVersion = options.versionQueryParam || performance.now()
  const { canvas, baseUrl, onProgress, onSuccess, onError } = options
  const resolveWithBaseUrl = (file: string) => new URL(file + "?v=" + rendererVersion, baseUrl).toString()

  const config = {
    dataUrl: resolveWithBaseUrl("unity.data.unityweb"),
    frameworkUrl: resolveWithBaseUrl("unity.framework.js.unityweb"),
    codeUrl: resolveWithBaseUrl("unity.wasm.unityweb"),
    streamingAssetsUrl: resolveWithBaseUrl("StreamingAssets"),
    companyName: "Decentraland",
    productName: "Decentraland World Client",
    productVersion: "0.1",
  }

  const originalUnity = await createUnityInstance(canvas, config, onProgress, onSuccess, onError)

  return {
    originalUnity,
  }
}
