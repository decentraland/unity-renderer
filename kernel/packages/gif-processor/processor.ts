import { GIF_WORKERS } from 'config'
import { GifAsset, ProcessorMessageData, WorkerMessage, WorkerMessageData } from './types'

declare const Worker: any
declare const DCL: any

const gifProcessorWorkerRaw = require('raw-loader!../../static/gif-processor/worker.js')
const gifProcessorWorkerUrl = URL.createObjectURL(new Blob([gifProcessorWorkerRaw]))
const multipleGIFWorkers = GIF_WORKERS

/**
 *
 * Class that triggers GIF processing in a worker and dispatches the generated texture pointers to Unity
 *
 */
export class GIFProcessor {
  gameInstance: any
  GLctx: any
  unityInterface: any
  isWebGL1: boolean = false
  lastCreatedWorker: any
  assets: Record<string, GifAsset> = {}

  constructor(gameInstance: any, unityInterface: any, isWebGL1: boolean) {
    this.gameInstance = gameInstance
    this.GLctx = this.gameInstance.Module.ctx
    this.unityInterface = unityInterface
    this.isWebGL1 = isWebGL1
  }

  /**
   *
   * Triggers the GIF processing in the worker (the comeback is executed at worker.onmessage)
   *
   */
  ProcessGIF(data: { imageSource: string; id: string }) {
    const gifInMemory = this.assets[data.id]

    if (gifInMemory) {
      if (!gifInMemory.pending) {
        this.setGLTextures(gifInMemory)
        this.reportToRenderer(gifInMemory)
        return
      }
    }

    const worker = this.GetWorker()

    this.assets[data.id] = {
      pending: true,
      id: data.id,
      url: data.imageSource,
      delays: [],
      width: 0,
      height: 0,
      textures: [],
      worker: worker
    }
    worker.postMessage({ url: data.imageSource, id: data.id, type: 'FETCH' } as ProcessorMessageData)
  }

  DeleteGIF(id: string) {
    const asset = this.assets[id]
    if (!asset) return

    if (!asset.pending) {
      const GLctx = this.gameInstance.Module.ctx
      for (let i = 0; i < asset.textures.length; i++) {
        const textureIdx = asset.textures[i].name
        const texture = DCL.GL.textures[textureIdx]
        GLctx.deleteTexture(texture)
        DCL.GL.textures[textureIdx] = null
      }
    } else {
      const worker = asset.worker
      worker.postMessage({ id: asset.id, type: 'CANCEL' } as Partial<ProcessorMessageData>)
    }
    delete this.assets[id]
  }

  /**
   *
   * Based on WebGL compiled "glGenTextures", created a WebGL texture and returns it
   *
   */
  GenerateTexture(): any {
    let GLctx = this.gameInstance.Module.ctx
    let texture = GLctx.createTexture()

    if (!texture) {
      DCL.GL.recordError(1282)
      return
    }

    let id = DCL.GL.getNewId(DCL.GL.textures)
    texture.name = id
    DCL.GL.textures[id] = texture

    return texture
  }

  /**
   *
   * "Prints" an ImageData into an already existent WebGL texture
   *
   */
  UpdateGIFTex(image: any, texId: any) {
    this.GLctx.bindTexture(this.GLctx.TEXTURE_2D, DCL.GL.textures[texId])

    if (this.isWebGL1) {
      this.GLctx.texImage2D(this.GLctx.TEXTURE_2D, 0, this.GLctx.RGBA, this.GLctx.RGBA, this.GLctx.UNSIGNED_BYTE, image)
    } else {
      this.GLctx.texImage2D(
        this.GLctx.TEXTURE_2D,
        0,
        this.GLctx.RGBA,
        image.width,
        image.height,
        0,
        this.GLctx.RGBA,
        this.GLctx.UNSIGNED_BYTE,
        image
      )
    }
  }

  GetWorker() {
    if (!this.lastCreatedWorker || multipleGIFWorkers) {
      const worker = new Worker(gifProcessorWorkerUrl, { name: 'gifProcessorWorker' })

      worker.onmessage = (e: WorkerMessage) => {
        const asset = this.assets[e.data.id]
        if (asset) {
          if (e.data.success) {
            if (this.setGifAsset(asset, e.data)) {
              this.reportToRenderer(asset)
            }
          } else {
            this.reportFailureToRenderer(e.data.id)
            delete this.assets[e.data.id]
          }
        } else {
          this.reportFailureToRenderer(e.data.id)
        }

        if (multipleGIFWorkers) {
          worker.terminate()
        }
      }

      this.lastCreatedWorker = worker
    }

    return this.lastCreatedWorker
  }

  private setGLTextures(gifAsset: GifAsset) {
    for (let i = 0; i < gifAsset.textures.length; i++) {
      this.UpdateGIFTex(gifAsset.textures[i].imageData, gifAsset.textures[i].name)
    }
  }

  private reportToRenderer(gifAsset: GifAsset) {
    this.unityInterface.SendGIFPointers(
      gifAsset.id,
      gifAsset.width,
      gifAsset.height,
      gifAsset.textures.map((id) => id.name),
      gifAsset.delays
    )
  }

  private reportFailureToRenderer(id: string) {
    this.unityInterface.SendGIFFetchFailure(id)
  }

  private setGifAsset(asset: GifAsset, data: WorkerMessageData): boolean {
    if (!data.arrayBufferFrames || data.arrayBufferFrames.length <= 0) return false

    asset.width = data.width
    asset.height = data.height
    asset.delays = data.delays

    for (let i = 0; i < data.arrayBufferFrames.length; i++) {
      const tex = this.GenerateTexture()
      const frameImageData = new ImageData(new Uint8ClampedArray(data.arrayBufferFrames[i]), data.width, data.height)
      asset.textures.push({ name: tex.name, imageData: frameImageData })
      this.UpdateGIFTex(frameImageData, tex.name)
    }

    asset.pending = false
    return true
  }
}
