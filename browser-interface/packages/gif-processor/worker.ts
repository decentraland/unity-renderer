import { processGif } from './processors/gifuct-jsProcessor'
import { ProcessorMessage, WorkerMessageData } from './types'

declare const self: any

{
  const payloads: ProcessorMessage[] = []
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
      promise.catch((error) => console.log(error))
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
      const gifData = await processGif(buffer)

      self.postMessage(
        {
          success: true,
          arrayBufferFrames: gifData.imagesData,
          width: gifData.width,
          height: gifData.height,
          delays: gifData.frameDelays,
          url: e.data.url,
          id: e.data.id
        } as WorkerMessageData,
        gifData.imagesData
      )
    } catch (err) {
      abortController = null
      self.postMessage({
        success: false,
        id: e.data.id
      } as Partial<WorkerMessageData>)
    }
  }
}
