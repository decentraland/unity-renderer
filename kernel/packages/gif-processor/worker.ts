import defaultLogger from 'shared/logger'
import { parseGIF, decompressFrames } from 'gifuct-js'

declare const self: any

const gifCanvas = new OffscreenCanvas(1,1)
const gifCanvasCtx = gifCanvas.getContext('2d')
const gifPatchCanvas = new OffscreenCanvas(1,1)
const gifPatchCanvasCtx = gifPatchCanvas.getContext('2d')
const resizedCanvas = new OffscreenCanvas(1,1)
const resizedCanvasCtx = gifCanvas.getContext('2d')
const maxGIFDimension = 512

let frameImageData: any = undefined

{
  let payloads: any[] = new Array()

  self.onmessage = (e: any) => {
    EnqueuePayload(e)
  }

  function EnqueuePayload(e: any) {
    payloads.push(e)
    if (payloads.length === 1) {
      const promise = ConsumePayload()
      promise.catch((error) => defaultLogger.log(error))
    }
  }

  async function ConsumePayload() {
    while (payloads.length > 0) {
      await DownloadAndProcessGIF(payloads[0])
      payloads.splice(0, 1)
    }
  }

  async function DownloadAndProcessGIF(e: any) {
    const imageFetch = fetch(e.data.src)
    const response = await imageFetch
    const buffer = await response.arrayBuffer()
    const parsedGif = await parseGIF(buffer)
    const decompressedFrames = decompressFrames(parsedGif, true)
    const frameDelays = new Array()
    const framesAsArrayBuffer = new Array()
    let hasToBeResized = false

    frameImageData = undefined

    gifCanvas.width = decompressedFrames[0].dims.width
    let finalWidth = gifCanvas.width

    gifCanvas.height = decompressedFrames[0].dims.height
    let finalHeight = gifCanvas.height

    hasToBeResized = gifCanvas.width > maxGIFDimension || gifCanvas.height > maxGIFDimension
    if (hasToBeResized) {
      let scalingFactor = (gifCanvas.width > gifCanvas.height) ? (gifCanvas.width / maxGIFDimension) : (gifCanvas.height / maxGIFDimension)
      resizedCanvas.width = gifCanvas.width / scalingFactor
      finalWidth = resizedCanvas.width

      resizedCanvas.height = gifCanvas.height / scalingFactor
      finalHeight = resizedCanvas.height
    }

    for (const key in decompressedFrames) {
      frameDelays.push(decompressedFrames[key].delay)

      const processedImageData = GenerateFinalImageData(decompressedFrames[key], hasToBeResized)
      framesAsArrayBuffer.push(processedImageData.data.buffer)
    }

    self.postMessage({
      arrayBufferFrames: framesAsArrayBuffer,
      width: finalWidth,
      height: finalHeight,
      delays: frameDelays,
      id: e.data.id,
    }, framesAsArrayBuffer)
  }

  function GenerateFinalImageData(frame: any, hasToBeResized: boolean): any {
    if (!frameImageData || frame.dims.width !== frameImageData.width || frame.dims.height !== frameImageData.height) {
      gifPatchCanvas.width = frame.dims.width
      gifPatchCanvas.height = frame.dims.height

      frameImageData = gifPatchCanvasCtx?.createImageData(frame.dims.width, frame.dims.height)
    }

    if (frameImageData) {
      frameImageData.data.set(frame.patch)
      gifPatchCanvasCtx?.putImageData(frameImageData, 0, 0)

      // We have to flip it vertically or it's rendered upside down
      gifCanvasCtx?.scale(1, -1)
      gifCanvasCtx?.drawImage(gifPatchCanvas, frame.dims.left, -(gifCanvas.height - frame.dims.top))
    }

    let finalImageData = gifCanvasCtx?.getImageData(0, 0, gifCanvas.width, gifCanvas.height)

    // Reset the canvas scale/transformation (otherwise the resizing breaks)
    gifCanvasCtx?.setTransform(1, 0, 0, 1, 0, 0)

    if (finalImageData && hasToBeResized) {
      resizedCanvasCtx?.drawImage(gifCanvas, 0, 0, gifCanvas.width, gifCanvas.height, 0, 0, resizedCanvas.width, resizedCanvas.height)

      finalImageData = resizedCanvasCtx?.getImageData(0, 0, resizedCanvas.width, resizedCanvas.height)
    }

    return finalImageData
  }
}
