/// <reference types="offscreencanvas" />
/// <reference lib="dom" />

import { GifData } from '../types'
import { parseGIF, decompressFrames, ParsedFrame } from 'gifuct-js'

const composedFrameCanvas = new OffscreenCanvas(1, 1)
const composedFrameCtx: OffscreenCanvasRenderingContext2D | null = composedFrameCanvas.getContext('2d')
const frameCanvas = new OffscreenCanvas(1, 1)
const frameCanvasCtx: OffscreenCanvasRenderingContext2D | null = frameCanvas.getContext('2d')
const resizedFrameCanvas = new OffscreenCanvas(1, 1)
const resizedFrameCtx: OffscreenCanvasRenderingContext2D | null = composedFrameCanvas.getContext('2d')

const maxGIFDimension = 512

let prevFrameImageData: ImageData

enum FrameDisposalMethod {
  NOT_SPECIFIED = 0,
  NO_DISPOSAL = 1,
  RESTORE_TO_BACKGROUND_COLOR = 2,
  RESTORE_TO_PREVIOUS = 3
}

export async function processGif(buffer: ArrayBuffer): Promise<GifData> {
  const parsedGif = await parseGIF(buffer)
  const decompressedFrames = decompressFrames(parsedGif, true)

  const frameDelays: number[] = []
  const framesImageData: ArrayBufferLike[] = []

  const gifDims = decompressedFrames[0].dims

  composedFrameCanvas.width = gifDims.width
  composedFrameCanvas.height = gifDims.height

  // calculate if images has to be resized
  const { width, height, shouldResize } = getResizingInfo(gifDims)

  resizedFrameCanvas.width = width
  resizedFrameCanvas.height = height

  // iterate frame by frame to genereate gif images
  let isFirstFrame = true
  for (const frame of decompressedFrames) {
    frameDelays.push(frame.delay)

    const processedImageData = generateFinalImageData(frame, shouldResize, isFirstFrame)
    if (processedImageData) {
      framesImageData.push(processedImageData.data.buffer)
    }
    isFirstFrame = false
  }

  return {
    imagesData: framesImageData,
    width: width,
    height: height,
    frameDelays: frameDelays
  }
}

function getResizingInfo(gifDims: { width: number; height: number }): {
  width: number
  height: number
  shouldResize: boolean
} {
  let width = gifDims.width
  let height = gifDims.height
  const shouldResize = width > maxGIFDimension || height > maxGIFDimension

  if (shouldResize) {
    const scalingFactor = width > height ? width / maxGIFDimension : height / maxGIFDimension
    width = width / scalingFactor
    height = height / scalingFactor
  }

  return { width, height, shouldResize }
}

function generateFinalImageData(
  frame: ParsedFrame,
  hasToBeResized: boolean,
  isFirstFrame: boolean
): ImageData | undefined {
  if (!frameCanvasCtx || !composedFrameCtx) {
    return undefined
  }

  // if is not the first frame we set the previous image as frame's background
  // since some gifs use frames as image difference to optimize size
  if (!isFirstFrame) {
    restorePrevFrame()
  }

  // we set the image canvas for this frame, which could mean just the image difference
  // between previous frame
  setFrameCanvas(frame)

  // we compose the image using the previous frame with the new one
  let finalImageData = setComposedFrameCanvas(frameCanvas, frame)

  // depending on the frame disposal method we store the "previous frame image"
  // to be used as a the background of the next frame
  if (
    frame.disposalType !== FrameDisposalMethod.RESTORE_TO_BACKGROUND_COLOR &&
    frame.disposalType !== FrameDisposalMethod.RESTORE_TO_PREVIOUS
  ) {
    setPrevFrameImage(finalImageData!)
  }

  // reset all transformation done to the canvas
  composedFrameCtx.setTransform(1, 0, 0, 1, 0, 0)

  // scale the image if needed
  if (finalImageData && hasToBeResized) {
    finalImageData = resizeImage(composedFrameCanvas, resizedFrameCanvas.width, resizedFrameCanvas.height)
  }

  return finalImageData
}

function setFrameCanvas(frame: ParsedFrame): ImageData | undefined {
  if (!frameCanvasCtx) {
    return undefined
  }
  frameCanvas.width = frame.dims.width
  frameCanvas.height = frame.dims.height
  const frameImageData = frameCanvasCtx.createImageData(frame.dims.width, frame.dims.height)
  frameImageData.data.set(frame.patch)
  frameCanvasCtx.putImageData(frameImageData, 0, 0)
  return frameImageData
}

function setComposedFrameCanvas(image: CanvasImageSource | OffscreenCanvas, frame: ParsedFrame): ImageData | undefined {
  if (!composedFrameCtx) {
    return undefined
  }
  // We have to flip it vertically or it's rendered upside down
  composedFrameCtx.scale(1, -1)
  composedFrameCtx.drawImage(image, frame.dims.left, -(composedFrameCanvas.height - frame.dims.top))
  return composedFrameCtx.getImageData(0, 0, composedFrameCanvas.width, composedFrameCanvas.height)
}

function setPrevFrameImage(imageData: ImageData) {
  prevFrameImageData = imageData
}

function restorePrevFrame() {
  if (!composedFrameCtx) {
    return undefined
  }
  composedFrameCtx.putImageData(prevFrameImageData, 0, 0)
}

function resizeImage(canvas: OffscreenCanvas, width: number, height: number): ImageData | undefined {
  if (!composedFrameCtx || !resizedFrameCtx) {
    return undefined
  }
  // Reset the canvas scale/transformation (otherwise the resizing breaks)
  composedFrameCtx.setTransform(1, 0, 0, 1, 0, 0)
  resizedFrameCtx.drawImage(canvas, 0, 0, canvas.width, canvas.height, 0, 0, width, height)
  return resizedFrameCtx.getImageData(0, 0, width, height)
}
