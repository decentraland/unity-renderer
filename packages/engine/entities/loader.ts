import * as BABYLON from 'babylonjs'
import { future } from 'fp-future'
import { scene } from '../renderer'

export const loadingManager = new BABYLON.AssetsManager(scene)

export async function loadFile(url: string): Promise<ArrayBuffer> {
  return new Promise<ArrayBuffer>((ok, reject) => {
    const task = loadingManager.addBinaryFileTask(url, url)
    task.run(scene, () => ok(task.data), (message, error) => reject(error || message))
  })
}

export async function loadText(url: string): Promise<string> {
  return new Promise<string>((ok, reject) => {
    const task = loadingManager.addTextFileTask(url, url)
    task.run(scene, () => ok(task.text), (message, error) => reject(error || message))
  })
}

export function loadTexture(url: string) {
  return new Promise<BABYLON.Texture>((ok, reject) => {
    const task = loadingManager.addTextureTask(url, url)
    task.run(
      scene,
      () => ok(task.texture),
      (message, error) => reject(error || message || task.errorObject || `Error loading texture ${url}`)
    )
  })
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
