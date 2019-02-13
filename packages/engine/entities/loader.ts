import * as BABYLON from 'babylonjs'
import { future, IFuture } from 'fp-future'
import { scene } from '../renderer'

// tslint:disable-next-line:whitespace
type SharedSceneContext = import('./SharedSceneContext').SharedSceneContext

export const loadingManager = new BABYLON.AssetsManager(scene)
export const loadedTextures = new Map<string, IFuture<BABYLON.Texture>>()

const registeredSharedContexts = new Set<SharedSceneContext>()

function registerTexture(texture: BABYLON.Texture, url: string) {
  texture.onDisposeObservable.add(() => {
    loadedTextures.delete(url)
  })
}

function deRegisterContext(context: SharedSceneContext) {
  registeredSharedContexts.delete(context)
  queueMicrotask(deleteUnusedTextures)
}

export function registerContextInResourceManager(context: SharedSceneContext) {
  registeredSharedContexts.add(context)
  context.onDisposeObservable.add(deRegisterContext)
}

let deletionPending = false

export function deleteUnusedTextures() {
  deletionPending = true
  queueMicrotask(() => {
    if (!deletionPending) return
    deletionPending = false

    const usedTextures = getUsedTextures()

    loadedTextures.forEach((value, key) => {
      if (!value.isPending && !usedTextures.has(key)) {
        loadedTextures.get(key).then(
          $ => {
            $.dispose()
          },
          () => void 0
        )
      }
    })
  })
}

function getUsedTextures(): Set<string> {
  const usedTextures = new Set<string>()

  registeredSharedContexts.forEach(current => {
    current.getCurrentUsages().textures.forEach($ => usedTextures.add($.url))
  })

  return usedTextures
}

export async function loadFile(url: string): Promise<ArrayBuffer> {
  return new Promise<ArrayBuffer>((ok, reject) => {
    BABYLON.Tools.LoadFile(
      url,
      ab => {
        ok(ab as ArrayBuffer)
      },
      null,
      scene.database,
      true,
      (_xhr, exc) => {
        reject(exc)
      }
    )
  })
}

export async function loadTexture(url: string): Promise<BABYLON.Texture> {
  if (loadedTextures.has(url)) {
    return loadedTextures.get(url)
  }

  const defer = future<BABYLON.Texture>()
  loadedTextures.set(url, defer)

  if (url.match(/^data:[^\/]+\/[^;]+;base64,/)) {
    const texture = new BABYLON.Texture(
      null,
      scene,
      false,
      false,
      BABYLON.Texture.BILINEAR_SAMPLINGMODE,
      () => {
        texture.url = url
        defer.resolve(texture)
        registerTexture(texture, url)
      },
      (message, exception) => {
        defer.reject(message || exception || `Error loading texture (base64)`)
        loadedTextures.delete(url)
      },
      url,
      true
    )
  } else {
    const task = loadingManager.addTextureTask(url, url)

    task.run(
      scene,
      () => {
        task.texture.url = url
        defer.resolve(task.texture)
        registerTexture(task.texture, url)
      },
      (message, error) => defer.reject(error || message || task.errorObject || `Error loading texture ${url}`)
    )
  }
  return defer
}

const scriptsCache: Map<string, Promise<URL>> = new Map()

export async function loadAsLocalUrl(src: string | Blob) {
  if (src instanceof Blob) {
    return URL.createObjectURL(src)
  }

  if (scriptsCache.has(src)) {
    return scriptsCache.get(src)
  }

  const resolvable = future()
  scriptsCache.set(src, resolvable)

  if (src.startsWith('blob:')) {
    resolvable.resolve(src)
    return resolvable
  }

  loadFile(src)
    .then(file => {
      const theBlob = file instanceof Blob ? file : new Blob([file])
      const theUrl = URL.createObjectURL(theBlob)
      resolvable.resolve(theUrl)
    })
    .catch(err => {
      resolvable.reject(err)
    })

  return resolvable
}
