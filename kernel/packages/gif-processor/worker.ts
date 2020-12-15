import defaultLogger from 'shared/logger'
import { parseGIF, decompressFrames } from 'gifuct-js'
import { ProcessorMessage, WorkerMessageData } from './types'

declare const self: any

const gifCanvas = new OffscreenCanvas(1, 1)
const gifCanvasCtx = gifCanvas.getContext('2d')
const gifPatchCanvas = new OffscreenCanvas(1, 1)
const gifPatchCanvasCtx = gifPatchCanvas.getContext('2d')
const resizedCanvas = new OffscreenCanvas(1, 1)
const resizedCanvasCtx = gifCanvas.getContext('2d')
const maxGIFDimension = 512

let frameImageData: any = undefined

{
  let payloads: ProcessorMessage[] = new Array()
  let payloadInProcess: ProcessorMessage | null = null
  let abortController: AbortController | null

  self.onmessage = (e: ProcessorMessage) => {
    if (e.data.type === 'FETCH') {
      EnqueuePayload(e)
    } else if (e.data.type === 'CANCEL') {
      CancelPayload(e)
    }
  }

  function EnqueuePayload(e: ProcessorMessage) {
    payloads.push(e)
    if (payloads.length === 1) {
      const promise = ConsumePayload()
      promise.catch((error) => defaultLogger.log(error))
    }
  }

  function CancelPayload(e: ProcessorMessage) {
    const isDownloading = abortController && payloadInProcess && payloadInProcess.data.id === e.data.id
    if (isDownloading) {
      abortController!.abort()
      return
    }

    for (let i = 0; i < payloads.length; i++) {
      if (payloads[i].data.id === e.data.id) {
        payloads.slice(i, 0)
        return
      }
    }
  }

  async function ConsumePayload() {
    while (payloads.length > 0) {
      payloadInProcess = payloads[0]
      await DownloadAndProcessGIF(payloadInProcess)
      payloadInProcess = null
      payloads.splice(0, 1)
    }
  }

  async function DownloadAndProcessGIF(e: ProcessorMessage) {
    abortController = new AbortController()
    const signal = abortController.signal

    try {
      const imageFetch = fetch(e.data.url, { signal })
      const response = await imageFetch
      abortController = null

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
        let scalingFactor =
          gifCanvas.width > gifCanvas.height ? gifCanvas.width / maxGIFDimension : gifCanvas.height / maxGIFDimension
        resizedCanvas.width = gifCanvas.width / scalingFactor
        finalWidth = resizedCanvas.width

        resizedCanvas.height = gifCanvas.height / scalingFactor
        finalHeight = resizedCanvas.height
      }

      for (const key in decompressedFrames) {
        frameDelays.push(decompressedFrames[key].delay)

        const processedImageData = GenerateFinalImageData(decompressedFrames[key], hasToBeResized)
        if (processedImageData) framesAsArrayBuffer.push(processedImageData.data.buffer)
      }

      self.postMessage(
        {
          success: true,
          arrayBufferFrames: framesAsArrayBuffer,
          width: finalWidth,
          height: finalHeight,
          delays: frameDelays,
          url: e.data.url,
          id: e.data.id
        } as WorkerMessageData,
        framesAsArrayBuffer
      )
    } catch (err) {
      abortController = null
      self.postMessage({
        success: false,
        id: e.data.id
      } as Partial<WorkerMessageData>)
    }
  }

  function GenerateFinalImageData(frame: any, hasToBeResized: boolean): ImageData | undefined {
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
      resizedCanvasCtx?.drawImage(
        gifCanvas,
        0,
        0,
        gifCanvas.width,
        gifCanvas.height,
        0,
        0,
        resizedCanvas.width,
        resizedCanvas.height
      )

      finalImageData = resizedCanvasCtx?.getImageData(0, 0, resizedCanvas.width, resizedCanvas.height)
    }

    return finalImageData
  }
}
