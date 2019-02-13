import { Tools, IFileRequest, Observable } from 'babylonjs'
import { error } from '../logger'
import { isRunningTest, DEBUG } from 'config'

// tslint:disable-next-line:whitespace
type SharedSceneContext = import('../entities/SharedSceneContext').SharedSceneContext

/// --- DECLARES ---

const registeredContext = new Map<string, SharedSceneContext>()
const dclRE = /^dcl:\/\/([^/]+)\/(.*)$/

/// --- PRIVATE ---

function readDclUrl(url: string, onError: (_, error?: Error) => void) {
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

function ensureContext(domain: string, onError: (_, error?: Error) => void): SharedSceneContext {
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

/**
 * Loads a file
 * @param fileToLoad defines the file to load
 * @param callback defines the callback to call when data is loaded
 * @param progressCallBack defines the callback to call during loading process
 * @param useArrayBuffer defines a boolean indicating that data must be returned as an ArrayBuffer
 * @returns a file request object
 */
function ReadFileAsync(
  fileToLoadPromise: Promise<File>,
  callback: (data: any) => void,
  progressCallBack?: (ev: ProgressEvent) => any,
  onError?: (xhr: any, exception: Error) => any,
  useArrayBuffer?: boolean
): IFileRequest {
  let reader = new FileReader()
  let request: IFileRequest = {
    onCompleteObservable: new Observable<IFileRequest>(),
    abort: () => reader.abort()
  }

  fileToLoadPromise
    .then(fileToLoad => {
      reader.onloadend = e => request.onCompleteObservable.notifyObservers(request)
      reader.onerror = e => {
        Tools.Log('Error while reading file: ' + fileToLoad.name)
        callback(
          JSON.stringify({
            autoClear: true,
            clearColor: [1, 0, 0],
            ambientColor: [0, 0, 0],
            gravity: [0, -9.807, 0],
            meshes: [],
            cameras: [],
            lights: []
          })
        )
      }
      reader.onload = e => {
        // target doesn't have result from ts 1.3
        callback((e.target as any)['result'])
      }
      if (progressCallBack) {
        reader.onprogress = progressCallBack
      }
      if (!useArrayBuffer) {
        // Asynchronous read
        reader.readAsText(fileToLoad)
      } else {
        reader.readAsArrayBuffer(fileToLoad)
      }
    })
    .catch(e => {
      if (onError) {
        onError(null, e)
      } else {
        error(e)
      }
    })

  return request
}

/// --- EXPORTS ---

export function initMonkeyLoader() {
  const originalFileLoader = Tools.LoadFile
  const originalImageLoader = Tools.LoadImage

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

      const filePromise = ctx.getFile(path)

      return ReadFileAsync(filePromise, onSuccess, onProgress, onError, useArrayBuffer)
    }

    return originalFileLoader.apply(Tools, arguments)
  }

  const newImageLoader: typeof originalImageLoader = function(url: string, onLoad, onError, database) {
    if (typeof url === 'string' && url.startsWith('dcl://')) {
      const { domain, path } = readDclUrl(url, onError)

      const ctx = ensureContext(domain, onError)

      const img = new Image()

      const loadHandler = function() {
        URL.revokeObjectURL(img.src)
        img.removeEventListener('load', loadHandler)
        // tslint:disable-next-line
        img.removeEventListener('error', errorHandler)
        if (onLoad) {
          onLoad(img)
        }
      }

      const errorHandler = function(err) {
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

      return img
    }

    return originalImageLoader.apply(Tools, arguments)
  }

  Tools.LoadFile = newFileLoader
  Tools.LoadImage = newImageLoader

  if (DEBUG || isRunningTest) {
    const originalScriptLoader = Tools.LoadScript
    Tools.LoadScript = function(scriptUrl, _1, onError?) {
      error(`Warning. Loading script. This doesn't work in production. ${scriptUrl}`)
      return originalScriptLoader.apply(this, arguments)
    }
  } else {
    Tools.LoadScript = function(scriptUrl, _1, onError?) {
      if (onError) {
        onError('Cannot load scripts in decentraland. ' + scriptUrl)
      }

      throw new Error('Cannot load scripts in decentraland. ' + scriptUrl)
    }
  }
}

export function registerLoadingContext(sharedContext: SharedSceneContext) {
  registeredContext.set(sharedContext.domain, sharedContext)
  sharedContext.onDisposeObservable.add(removeLoadingContext)
}

function removeLoadingContext(sharedContext: SharedSceneContext) {
  registeredContext.delete(sharedContext.domain)
}
