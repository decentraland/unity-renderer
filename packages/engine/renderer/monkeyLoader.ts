import { future, IFuture } from 'fp-future'
import * as BABYLON from 'babylonjs'
import { error } from '../logger'
import { isRunningTest, DEBUG } from 'config'
import { scene, engineMicroQueue } from '../renderer'

// tslint:disable-next-line:whitespace
type SharedSceneContext = import('../entities/SharedSceneContext').SharedSceneContext

/// --- DECLARES ---

let deletionPending = false
const sceneTextureSymbol = Symbol('scene-texture-url')
const registeredSharedContexts = new Set<SharedSceneContext>()
const registeredContext = new Map<string, SharedSceneContext>()
const dclRE = /^dcl:\/\/([^/]+)\/(.*)$/

export const loadingManager = new BABYLON.AssetsManager(scene)
export const loadedTextures = new Map<string, IFuture<BABYLON.Texture>>()
export const loadedFiles = new Map<string, IFuture<ArrayBuffer>>()

/// --- PRIVATE ---

function readDclUrl(
  url: string,
  onError: (_: any, error?: Error) => void = (_, error) => {
    throw error
  }
) {
  const reResult = dclRE.exec(url)

  if (!reResult) {
    const err = new Error('Cannot resolve DCL request: ' + url)
    if (onError) {
      if (onError.length === 2) {
        onError(null, err)
      } else {
        onError(err)
      }
    }

    throw err
  }

  const domain = reResult[1]
  const path = reResult[2]

  return { domain, path }
}

function ensureContext(
  domain: string,
  onError: (_: any, error?: Error) => void = (_, error) => {
    throw error
  }
): SharedSceneContext {
  const ctx = registeredContext.get(domain)
  if (!ctx) {
    const err = new Error('Cannot resolve DCL domain: ' + domain)
    if (onError) {
      if (onError.length === 2) {
        onError(null, err)
      } else {
        onError(err)
      }
    }
    throw err
  }
  return ctx
}

function removeLoadingContext(sharedContext: SharedSceneContext) {
  registeredContext.delete(sharedContext.domain)
}

function deRegisterContext(context: SharedSceneContext) {
  registeredSharedContexts.delete(context)
  engineMicroQueue.queueMicroTask(deleteUnusedTextures)
}

function markSceneTexture(texture: BABYLON.Texture, url: string) {
  // tslint:disable-next-line:semicolon
  ;(texture as any)[sceneTextureSymbol] = url

  texture.onDisposeObservable.add(() => {
    const url = isSceneTexture(texture) || texture.url
    if (url) loadedTextures.delete(url)
  })
}

/// --- EXPORTS ---

export function getUsedTextures(): Set<string> {
  const usedTextures = new Set<string>()

  registeredSharedContexts.forEach(current => {
    current.getCurrentUsages().textures.forEach($ => {
      const usedUrl = isSceneTexture($)
      if (usedUrl) {
        usedTextures.add(usedUrl)
      }
    })
  })

  return usedTextures
}

export function registerLoadingContext(sharedContext: SharedSceneContext) {
  registeredContext.set(sharedContext.domain, sharedContext)
  sharedContext.onDisposeObservable.add(removeLoadingContext)
}

export function registerContextInResourceManager(context: SharedSceneContext) {
  registeredSharedContexts.add(context)
  context.onDisposeObservable.add(deRegisterContext)
}

export function isSceneTexture(texture: BABYLON.Texture): string | null {
  return (texture as any)[sceneTextureSymbol] || null
}

export function deleteUnusedTextures() {
  deletionPending = true
  engineMicroQueue.queueMicroTask(() => {
    if (!deletionPending) return
    deletionPending = false

    const usedTextures = getUsedTextures()

    loadedTextures.forEach(async (value, key) => {
      if (!value.isPending && !usedTextures.has(key)) {
        value.then(
          $ => {
            $.dispose()
          },
          () => void 0
        )
      }
    })

    loadedFiles.forEach(($, key) => {
      if (!$.isPending) {
        loadedFiles.delete(key)
      }
    })
  })
}

export async function loadFile(url: string, useArrayBuffer = true): Promise<ArrayBuffer> {
  if (loadedFiles.has(url)) {
    return loadedFiles.get(url)!
  }

  const defer = future<ArrayBuffer>()
  loadedFiles.set(url, defer)

  BABYLON.Tools.LoadFile(
    url,
    ab => {
      defer.resolve(ab as ArrayBuffer)
    },
    void 0,
    scene.database,
    useArrayBuffer,
    (_xhr, exc) => {
      defer.reject(exc)
    }
  )

  return defer
}

export async function loadTextureFromAB(
  url: string,
  ab: BlobPart,
  mimeType: string,
  samplerData?: BABYLON.GLTF2.Loader._ISamplerData
) {
  if (loadedTextures.has(url)) {
    return loadedTextures.get(url)!
  }

  const defer = future<BABYLON.Texture>()
  loadedTextures.set(url, defer)

  const texture = new BABYLON.Texture(
    null,
    scene,
    samplerData ? samplerData.noMipMaps : false,
    samplerData ? false : true,
    samplerData ? samplerData.samplingMode : BABYLON.Texture.BILINEAR_SAMPLINGMODE,
    () => {
      markSceneTexture(texture, url)
      defer.resolve(texture)
    },
    (message, exception) => {
      defer.reject(message || exception || `Error loading texture (base64)`)
      loadedTextures.delete(url)
    },
    null,
    false
  )

  const dataUrl = `data:${url}`
  texture.updateURL(dataUrl, new Blob([ab], { type: mimeType }))

  if (samplerData) {
    texture.wrapU = samplerData.wrapU
    texture.wrapV = samplerData.wrapV
  }

  return defer
}

export async function loadTexture(
  url: string,
  samplerData?: BABYLON.GLTF2.Loader._ISamplerData
): Promise<BABYLON.Texture> {
  if (url.startsWith('dcl://')) {
    const { domain, path } = readDclUrl(url)

    const ctx = ensureContext(domain)

    const newUrl = ctx.resolveUrl(path)

    return loadTexture(newUrl, samplerData)
  }

  if (loadedTextures.has(url)) {
    return loadedTextures.get(url)!
  }

  const defer = future<BABYLON.Texture>()
  loadedTextures.set(url, defer)

  if (url.match(/^data:[^\/]+\/[^;]+;base64,/)) {
    const texture = new BABYLON.Texture(
      url,
      scene,
      samplerData ? samplerData.noMipMaps : false,
      samplerData ? false : true,
      samplerData ? samplerData.samplingMode : BABYLON.Texture.BILINEAR_SAMPLINGMODE,
      () => void 0,
      (message, exception) => {
        defer.reject(message || exception || `Error loading texture (base64)`)
        loadedTextures.delete(url)
      },
      url,
      true
    )

    markSceneTexture(texture, url)
    defer.resolve(texture)

    if (samplerData) {
      texture.wrapU = samplerData.wrapU
      texture.wrapV = samplerData.wrapV
    }
  } else {
    const texture = new BABYLON.Texture(
      url,
      scene,
      samplerData ? samplerData.noMipMaps : false,
      samplerData ? false : true,
      samplerData ? samplerData.samplingMode : BABYLON.Texture.BILINEAR_SAMPLINGMODE,
      () => {
        markSceneTexture(texture, url)
        defer.resolve(texture)
      },
      function(this: any, message, exception) {
        loadedTextures.delete(url)
        if (!this._disposed) {
          defer.reject(message || exception || `Error loading texture (${url})`)
        }
      }
    )

    if (samplerData) {
      texture.wrapU = samplerData.wrapU
      texture.wrapV = samplerData.wrapV
    }
  }
  return defer
}

export function initMonkeyLoader() {
  const originalFileLoader = BABYLON.Tools.LoadFile
  const originalImageLoader = BABYLON.Tools.LoadImage

  const newFileLoader: typeof originalFileLoader = function(
    url: string,
    onSuccess,
    onProgress,
    _database,
    useArrayBuffer,
    onError
  ) {
    if (url.startsWith('dcl://')) {
      const { domain, path } = readDclUrl(url, onError)

      const ctx = ensureContext(domain, onError)

      let request: BABYLON.IFileRequest = {
        onCompleteObservable: new BABYLON.Observable<BABYLON.IFileRequest>(),
        abort: () => void 0
      }

      const abPromise = useArrayBuffer ? ctx.getArrayBuffer(path) : ctx.getText(path)

      abPromise
        .then($ => {
          onSuccess && onSuccess($)
          request.onCompleteObservable.notifyObservers(request)
        })
        .catch($ => {
          onError && onError(void 0, $)
        })

      return request
    }

    return originalFileLoader.apply(BABYLON.Tools, (Array.prototype.slice as any).call(arguments))
  }

  const newImageLoader: typeof originalImageLoader = function(url: string, onLoad, onError, database) {
    if (typeof (url as any) === 'string' && url.startsWith('dcl://')) {
      const { domain, path } = readDclUrl(url, onError)

      const ctx = ensureContext(domain, onError)

      const img = new Image()

      if (database) {
        const url = ctx.resolveUrl(path)

        const loadHandler = function() {
          img.removeEventListener('load', loadHandler)
          // tslint:disable-next-line
          img.removeEventListener('error', errorHandler)
          if (onLoad) {
            onLoad(img)
          }
        }

        const errorHandler = function(err: any) {
          img.removeEventListener('load', loadHandler)
          img.removeEventListener('error', errorHandler)
          if (onError) {
            onError('Error while trying to load image: ' + url, err)
          }
        }

        img.removeEventListener('load', loadHandler)
        img.removeEventListener('error', errorHandler)

        img.addEventListener('load', loadHandler)
        img.addEventListener('error', errorHandler)

        database.loadImageFromDB(url, img)
      } else {
        const loadHandler = function() {
          URL.revokeObjectURL(img.src)
          img.removeEventListener('load', loadHandler)
          // tslint:disable-next-line
          img.removeEventListener('error', errorHandler)
          if (onLoad) {
            onLoad(img)
          }
        }

        const errorHandler = function(err: any) {
          URL.revokeObjectURL(img.src)
          img.removeEventListener('load', loadHandler)
          img.removeEventListener('error', errorHandler)
          if (onError) {
            onError('Error while trying to load image: ' + url, err)
          }
        }

        img.removeEventListener('load', loadHandler)
        img.removeEventListener('error', errorHandler)

        img.addEventListener('load', loadHandler)
        img.addEventListener('error', errorHandler)

        ctx
          .getBlob(path)
          .then(blob => {
            img.src = URL.createObjectURL(blob)
          })
          .catch(e => {
            errorHandler(e)
          })
      }
      return img
    }

    return originalImageLoader.apply(BABYLON.Tools, (Array.prototype.slice as any).call(arguments))
  }

  BABYLON.Tools.LoadFile = newFileLoader
  BABYLON.Tools.LoadImage = newImageLoader

  if (DEBUG || isRunningTest) {
    const originalScriptLoader = BABYLON.Tools.LoadScript
    BABYLON.Tools.LoadScript = function(scriptUrl, _1, onError?) {
      error(`Warning. Loading script. This doesn't work in production. ${scriptUrl}`)
      return originalScriptLoader.apply(this, (Array.prototype.slice as any).call(arguments))
    }
  } else {
    BABYLON.Tools.LoadScript = function(scriptUrl, _1, onError?) {
      if (onError) {
        onError('Cannot load scripts in decentraland. ' + scriptUrl)
      }

      throw new Error('Cannot load scripts in decentraland. ' + scriptUrl)
    }
  }

  BABYLON.GLTF2.GLTFLoader.prototype['_loadTextureAsync'] = async function(
    this: BABYLON.GLTF2.GLTFLoader,
    context: string,
    texture: BABYLON.GLTF2.Loader.ITexture,
    assign: (babylonTexture: BABYLON.BaseTexture) => void = () => void 0
  ): Promise<BABYLON.BaseTexture> {
    this.logOpen(`${context} ${texture.name || ''}`)

    const sampler: BABYLON.GLTF2.Loader.ISampler =
      texture.sampler === undefined
        ? (BABYLON.GLTF2.GLTFLoader as any)._DefaultSampler
        : BABYLON.GLTF2.ArrayItem.Get(`${context}/sampler`, this.gltf.samplers, texture.sampler)

    const samplerData = (this as any)._loadSampler(`#/samplers/${sampler.index}`, sampler)

    const image = BABYLON.GLTF2.ArrayItem.Get(`${context}/source`, this.gltf.images, texture.source)

    let babylonTexture!: BABYLON.Texture

    if (!image._data) {
      if (image.uri) {
        if (BABYLON.Tools.IsBase64(image.uri)) {
          babylonTexture = await loadTexture(image.uri, samplerData)
        } else {
          babylonTexture = await loadTexture((this as any)._rootUrl + image.uri, samplerData)
        }
      } else {
        const bufferView = BABYLON.GLTF2.ArrayItem.Get(`${context}/bufferView`, this.gltf.bufferViews, image.bufferView)
        image._data = this.loadBufferViewAsync(`#/bufferViews/${bufferView.index}`, bufferView)
      }
    }

    if (!babylonTexture && image._data) {
      const data = await image._data
      const name = image.uri || `${(this as any)._fileName}#image${image.index}`
      const dataUrl = `${(this as any)._rootUrl}${name}`
      babylonTexture = await loadTextureFromAB(dataUrl, data, image.mimeType || 'image/png', samplerData)
    }

    babylonTexture.wrapU = samplerData.wrapU
    babylonTexture.wrapV = samplerData.wrapV

    assign(babylonTexture)
    ;(this as any)._parent.onTextureLoadedObservable.notifyObservers(babylonTexture)

    return babylonTexture
  }
}
