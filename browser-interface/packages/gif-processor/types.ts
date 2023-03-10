export type GifAsset = {
  id: string
  url: string
  textures: { name: any; imageData: ImageData }[]
  delays: number[]
  width: number
  height: number
  pending: boolean
  worker: any
}

export type WorkerMessage = {
  data: WorkerMessageData
}

export type WorkerMessageData = {
  arrayBufferFrames: Array<ArrayBufferLike> | undefined
  width: number
  height: number
  delays: number[]
  url: string
  id: string
  success: boolean
}

export type ProcessorMessage = {
  data: ProcessorMessageData
}

export type ProcessorMessageData = {
  url: string
  id: string
  type: ProcessorMessageType
}

type ProcessorMessageType = 'FETCH' | 'CANCEL'

export type GifData = {
  width: number
  height: number
  frameDelays: number[]
  imagesData: ArrayBufferLike[]
}
