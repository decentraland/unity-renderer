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

      let request: IFileRequest = {
        onCompleteObservable: new Observable<IFileRequest>(),
        abort: () => void 0
      }

      const abPromise = useArrayBuffer ? ctx.getArrayBuffer(path) : ctx.getText(path)

      abPromise
        .then($ => {
          onSuccess && onSuccess($)
          request.onCompleteObservable.notifyObservers(request)
        })
        .catch($ => {
          onError && onError(null, $)
        })

      return request
    }

    return originalFileLoader.apply(Tools, arguments)
  }

  const newImageLoader: typeof originalImageLoader = function(url: string, onLoad, onError, database) {
    if (typeof url === 'string' && url.startsWith('dcl://')) {
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

        const errorHandler = function(err) {
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
      }
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
