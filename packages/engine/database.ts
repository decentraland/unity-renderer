import * as BABYLON from 'babylonjs'

/**
 * Class used to enable access to IndexedDB
 * @see @https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API
 */
export class Database {
  /**
   * Gets a boolean indicating if scene must be saved in the database
   */
  public get enableSceneOffline(): boolean {
    return this._enableSceneOffline
  }

  /**
   * Gets a boolean indicating if textures must be saved in the database
   */
  public get enableTexturesOffline(): boolean {
    return this._enableTexturesOffline
  }

  /** Gets a boolean indicating if the user agent supports blob storage (this value will be updated after creating the first Database object) */
  static IsUASupportingBlobStorage = true

  /** Gets a boolean indicating if Database storate is enabled */
  static IDBStorageEnabled = true
  private db: BABYLON.Nullable<IDBDatabase>
  private _enableSceneOffline: boolean
  private _enableTexturesOffline: boolean
  private manifestVersionFound: number
  private hasReachedQuota: boolean
  private isSupported: boolean

  // Handling various flavors of prefixed version of IndexedDB
  private idbFactory = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB

  /**
   * Creates a new Database
   * @param urlToScene defines the url to load the scene
   * @param callbackManifestChecked defines the callback to use when manifest is checked
   * @param disableManifestCheck defines a boolean indicating that we want to skip the manifest validation (it will be considered validated and up to date)
   */
  constructor() {
    this.db = null
    this._enableSceneOffline = true
    this._enableTexturesOffline = true
    this.manifestVersionFound = 0
    this.hasReachedQuota = false
  }

  private static _ParseURL = (url: string) => {
    let a = document.createElement('a')
    a.href = url
    let urlWithoutHash = url.substring(0, url.lastIndexOf('#'))
    let fileName = url.substring(urlWithoutHash.lastIndexOf('/') + 1, url.length)
    let absLocation = url.substring(0, url.indexOf(fileName, 0))
    return absLocation
  }

  private static _ReturnFullUrlLocation = (url: string): string => {
    if (url.indexOf('http:/') === -1 && url.indexOf('https:/') === -1) {
      return Database._ParseURL(window.location.href) + url
    } else {
      return url
    }
  }

  /**
   * Open the database and make it available
   * @param successCallback defines the callback to call on success
   * @param errorCallback defines the callback to call on error
   */
  public openAsync(successCallback: () => void, errorCallback: () => void) {
    let handleError = () => {
      this.isSupported = false
      if (errorCallback) {
        errorCallback()
      }
    }

    if (!this.idbFactory || !(this._enableSceneOffline || this._enableTexturesOffline)) {
      // Your browser doesn't support IndexedDB
      this.isSupported = false
      if (errorCallback) {
        errorCallback()
      }
    } else {
      // If the DB hasn't been opened or created yet
      if (!this.db) {
        this.hasReachedQuota = false
        this.isSupported = true

        let request: IDBOpenDBRequest = this.idbFactory.open('decentraland-assets', 1)

        // Could occur if user is blocking the quota for the DB and/or doesn't grant access to IndexedDB
        request.onerror = event => {
          handleError()
        }

        // executes when a version change transaction cannot complete due to other active transactions
        request.onblocked = event => {
          BABYLON.Tools.Error('IDB request blocked. Please reload the page.')
          handleError()
        }

        // DB has been opened successfully
        request.onsuccess = event => {
          this.db = request.result
          successCallback()
        }

        // Initialization of the DB. Creating Scenes & Textures stores
        request.onupgradeneeded = (event: IDBVersionChangeEvent) => {
          this.db = (event.target as any).result
          if (this.db) {
            try {
              this.db.createObjectStore('scenes', { keyPath: 'sceneUrl' })
              this.db.createObjectStore('versions', { keyPath: 'sceneUrl' })
              this.db.createObjectStore('textures', { keyPath: 'textureUrl' })
            } catch (ex) {
              BABYLON.Tools.Error('Error while creating object stores. Exception: ' + ex.message)
              handleError()
            }
          }
        }
      } else {
        if (successCallback) {
          successCallback()
        }
      }
    }
  }

  /**
   * Loads an image from the database
   * @param url defines the url to load from
   * @param image defines the target DOM image
   */
  public loadImageFromDB(url: string, image: HTMLImageElement) {
    let completeURL = Database._ReturnFullUrlLocation(url)

    let saveAndLoadImage = () => {
      if (!this.hasReachedQuota && this.db !== null) {
        // the texture is not yet in the DB, let's try to save it
        this._saveImageIntoDBAsync(completeURL, image)
      } else {
        image.src = url
      }
    }

    this._loadImageFromDBAsync(completeURL, image, saveAndLoadImage)
  }

  /**
   * Loads a file from database
   * @param url defines the URL to load from
   * @param sceneLoaded defines a callback to call on success
   * @param progressCallBack defines a callback to call when progress changed
   * @param errorCallback defines a callback to call on error
   * @param useArrayBuffer defines a boolean to use array buffer instead of text string
   */
  public loadFileFromDB(
    url: string,
    sceneLoaded: (data: any) => void,
    progressCallBack?: (data: any) => void,
    errorCallback?: () => void,
    useArrayBuffer?: boolean
  ) {
    let completeUrl = Database._ReturnFullUrlLocation(url)

    let saveAndLoadFile = () => {
      // the scene is not yet in the DB, let's try to save it
      this._saveFileIntoDBAsync(completeUrl, sceneLoaded, progressCallBack, useArrayBuffer, errorCallback)
    }

    this._checkVersionFromDB(completeUrl, version => {
      if (version !== -1) {
        this._loadFileFromDBAsync(completeUrl, sceneLoaded, saveAndLoadFile, useArrayBuffer)
      } else {
        if (errorCallback) {
          errorCallback()
        }
      }
    })
  }

  private _loadImageFromDBAsync(url: string, image: HTMLImageElement, notInDBCallback: () => any) {
    if (this.isSupported && this.db !== null) {
      let texture: any
      let transaction: IDBTransaction = this.db.transaction(['textures'])

      transaction.onabort = event => {
        image.src = url
      }

      transaction.oncomplete = event => {
        let blobTextureURL: string
        if (texture) {
          let URL = window.URL || window.webkitURL

          if (texture.data instanceof Blob) {
            blobTextureURL = URL.createObjectURL(texture.data)
          } else {
            blobTextureURL = URL.createObjectURL(new Blob([texture.data]))
          }

          image.onerror = () => {
            BABYLON.Tools.Error(
              'Error loading image from blob URL: ' + blobTextureURL + ' switching back to web url: ' + url
            )
            image.src = url
          }
          image.src = blobTextureURL
        } else {
          notInDBCallback()
        }
      }

      let getRequest: IDBRequest = transaction.objectStore('textures').get(url)

      getRequest.onsuccess = event => {
        texture = (event.target as any).result
      }
      getRequest.onerror = event => {
        BABYLON.Tools.Error('Error loading texture ' + url + ' from DB.')
        image.src = url
      }
    } else {
      BABYLON.Tools.Error('Error: IndexedDB not supported by your browser or BabylonJS Database is not open.')
      image.src = url
    }
  }

  private _saveImageIntoDBAsync(url: string, image: HTMLImageElement) {
    if (this.isSupported) {
      // In case of error (type not supported or quota exceeded), we're at least sending back XHR data to allow texture loading later on
      let generateBlobUrl = (blob: Blob) => {
        let blobTextureURL

        if (blob) {
          let URL = window.URL || window.webkitURL
          try {
            blobTextureURL = URL.createObjectURL(blob)
          } catch (ex) {
            // Chrome is raising a type error if we're setting the oneTimeOnly parameter
            blobTextureURL = URL.createObjectURL(blob)
          }
        }

        if (blobTextureURL) {
          image.src = blobTextureURL
        }
      }

      if (Database.IsUASupportingBlobStorage) {
        // Create XHR
        let xhr = new XMLHttpRequest()
        let blob: Blob

        xhr.open('GET', url, true)
        xhr.responseType = 'blob'

        xhr.addEventListener(
          'load',
          () => {
            if (xhr.status === 200 && this.db) {
              // Blob as response (XHR2)
              blob = xhr.response

              let transaction = this.db.transaction(['textures'], 'readwrite')

              // the transaction could abort because of a QuotaExceededError error
              transaction.onabort = event => {
                try {
                  // backwards compatibility with ts 1.0, srcElement doesn't have an "error" according to ts 1.3
                  let srcElement = (event.srcElement || event.target) as any
                  let error = srcElement.error
                  if (error && error.name === 'QuotaExceededError') {
                    this.hasReachedQuota = true
                  }
                } catch (ex) {
                  // stub
                }
                generateBlobUrl(blob)
              }

              transaction.oncomplete = event => {
                generateBlobUrl(blob)
              }

              let newTexture = { textureUrl: url, data: blob }

              try {
                // Put the blob into the dabase
                let addRequest = transaction.objectStore('textures').put(newTexture)
                addRequest.onsuccess = event => {
                  // blob
                }
                addRequest.onerror = event => {
                  generateBlobUrl(blob)
                }
              } catch (ex) {
                // "DataCloneError" generated by Chrome when you try to inject blob into IndexedDB
                if (ex.code === 25) {
                  Database.IsUASupportingBlobStorage = false
                }
                image.src = url
              }
            } else {
              image.src = url
            }
          },
          false
        )

        xhr.addEventListener(
          'error',
          event => {
            BABYLON.Tools.Error('Error in XHR request in BABYLON.Database.')
            image.src = url
          },
          false
        )

        xhr.send()
      } else {
        image.src = url
      }
    } else {
      BABYLON.Tools.Error('Error: IndexedDB not supported by your browser or BabylonJS Database is not open.')
      image.src = url
    }
  }

  private _checkVersionFromDB(url: string, versionLoaded: (version: number) => void) {
    let updateVersion = () => {
      // the version is not yet in the DB or we need to update it
      this._saveVersionIntoDBAsync(url, versionLoaded)
    }
    this._loadVersionFromDBAsync(url, versionLoaded, updateVersion)
  }

  private _loadVersionFromDBAsync(url: string, callback: (version: number) => void, updateInDBCallback: () => void) {
    if (this.isSupported && this.db) {
      let version: any
      try {
        let transaction = this.db.transaction(['versions'])

        transaction.oncomplete = event => {
          if (version) {
            // If the version in the JSON file is different from the version in DB
            callback(version.data)
          } else {
            updateInDBCallback()
          }
        }

        transaction.onabort = event => {
          callback(-1)
        }

        let getRequest = transaction.objectStore('versions').get(url)

        getRequest.onsuccess = event => {
          version = (event.target as any).result
        }
        getRequest.onerror = event => {
          BABYLON.Tools.Error('Error loading version for scene ' + url + ' from DB.')
          callback(-1)
        }
      } catch (ex) {
        BABYLON.Tools.Error("Error while accessing 'versions' object store (READ OP). Exception: " + ex.message)
        callback(-1)
      }
    } else {
      BABYLON.Tools.Error('Error: IndexedDB not supported by your browser or BabylonJS Database is not open.')
      callback(-1)
    }
  }

  private _saveVersionIntoDBAsync(url: string, callback: (version: number) => void) {
    if (this.isSupported && !this.hasReachedQuota && this.db) {
      try {
        // Open a transaction to the database
        let transaction = this.db.transaction(['versions'], 'readwrite')

        // the transaction could abort because of a QuotaExceededError error
        transaction.onabort = event => {
          try {
            // backwards compatibility with ts 1.0, srcElement doesn't have an "error" according to ts 1.3
            let error = (event.srcElement as any)['error']
            if (error && error.name === 'QuotaExceededError') {
              this.hasReachedQuota = true
            }
          } catch (ex) {
            // stub
          }
          callback(-1)
        }

        transaction.oncomplete = event => {
          callback(this.manifestVersionFound)
        }

        let newVersion = { sceneUrl: url, data: this.manifestVersionFound }

        // Put the scene into the database
        let addRequest = transaction.objectStore('versions').put(newVersion)
        addRequest.onsuccess = event => {
          // stub
        }
        addRequest.onerror = event => {
          BABYLON.Tools.Error('Error in DB add version request in BABYLON.Database.')
        }
      } catch (ex) {
        BABYLON.Tools.Error("Error while accessing 'versions' object store (WRITE OP). Exception: " + ex.message)
        callback(-1)
      }
    } else {
      callback(-1)
    }
  }

  private _loadFileFromDBAsync(
    url: string,
    callback: (data?: any) => void,
    notInDBCallback: () => void,
    useArrayBuffer?: boolean
  ) {
    if (this.isSupported && this.db) {
      let targetStore: string
      if (url.indexOf('.babylon') !== -1) {
        targetStore = 'scenes'
      } else {
        targetStore = 'textures'
      }

      let file: any
      let transaction = this.db.transaction([targetStore])

      transaction.oncomplete = event => {
        if (file) {
          callback(file.data)
        } else {
          notInDBCallback()
        }
      }

      transaction.onabort = event => {
        notInDBCallback()
      }

      let getRequest = transaction.objectStore(targetStore).get(url)

      getRequest.onsuccess = event => {
        file = (event.target as any).result
      }
      getRequest.onerror = event => {
        BABYLON.Tools.Error('Error loading file ' + url + ' from DB.')
        notInDBCallback()
      }
    } else {
      BABYLON.Tools.Error('Error: IndexedDB not supported by your browser or BabylonJS Database is not open.')
      callback()
    }
  }

  private _saveFileIntoDBAsync(
    url: string,
    callback: (data?: any) => void,
    progressCallback?: (this: XMLHttpRequestEventTarget, ev: ProgressEvent) => any,
    useArrayBuffer?: boolean,
    errorCallback?: (data?: any) => void
  ) {
    if (this.isSupported) {
      let targetStore: string
      if (url.indexOf('.babylon') !== -1) {
        targetStore = 'scenes'
      } else {
        targetStore = 'textures'
      }

      // Create XHR
      let xhr = new XMLHttpRequest()
      let fileData: any
      xhr.open('GET', url, true) // It has this thing: + '?' + Date.now()

      if (useArrayBuffer) {
        xhr.responseType = 'arraybuffer'
      }

      if (progressCallback) {
        xhr.onprogress = progressCallback
      }

      xhr.addEventListener(
        'load',
        () => {
          if (xhr.status === 200 || (xhr.status < 400 && BABYLON.Tools.ValidateXHRData(xhr, !useArrayBuffer ? 1 : 6))) {
            // Blob as response (XHR2)
            fileData = !useArrayBuffer ? xhr.responseText : xhr.response

            if (!this.hasReachedQuota && this.db) {
              // Open a transaction to the database
              let transaction = this.db.transaction([targetStore], 'readwrite')

              // the transaction could abort because of a QuotaExceededError error
              transaction.onabort = event => {
                try {
                  // backwards compatibility with ts 1.0, srcElement doesn't have an "error" according to ts 1.3
                  let error = (event.srcElement as any)['error']
                  if (error && error.name === 'QuotaExceededError') {
                    this.hasReachedQuota = true
                  }
                } catch (ex) {
                  // stub
                }
                callback(fileData)
              }

              transaction.oncomplete = event => {
                callback(fileData)
              }

              let newFile
              if (targetStore === 'scenes') {
                newFile = { sceneUrl: url, data: fileData, version: this.manifestVersionFound }
              } else {
                newFile = { textureUrl: url, data: fileData }
              }

              try {
                // Put the scene into the database
                let addRequest = transaction.objectStore(targetStore).put(newFile)
                addRequest.onsuccess = event => {
                  // stub
                }
                addRequest.onerror = event => {
                  BABYLON.Tools.Error('Error in DB add file request in BABYLON.Database.')
                }
              } catch (ex) {
                callback(fileData)
              }
            } else {
              callback(fileData)
            }
          } else {
            if (xhr.status >= 400 && errorCallback) {
              errorCallback(xhr)
            } else {
              callback()
            }
          }
        },
        false
      )

      xhr.addEventListener(
        'error',
        event => {
          BABYLON.Tools.Error('error on XHR request.')
          callback()
        },
        false
      )

      xhr.send()
    } else {
      BABYLON.Tools.Error('Error: IndexedDB not supported by your browser or BabylonJS Database is not open.')
      callback()
    }
  }
}
