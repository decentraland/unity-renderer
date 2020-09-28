import { VoiceChatWorkerResponse, RequestTopic, ResponseTopic } from './types'
import { Resampler } from './resampler'
import { OPUS_BITS_PER_SECOND, OPUS_FRAME_SIZE_MS } from './constants'
declare var self: WorkerGlobalScope & any

declare function postMessage(message: any, transferables: any[]): void

declare const libopus: any

self.LIBOPUS_WASM_URL = 'libopus.wasm'

importScripts('libopus.wasm.js')

type CodecWorklet = {
  working: boolean
  lastWorkTime: number
  destroy: () => any
}

type EncoderWorklet = {
  encoder: any
} & CodecWorklet

type DecoderWorklet = {
  decoder: any
} & CodecWorklet

function getSampleRate(e: MessageEvent) {
  return e.data.sampleRate ? e.data.sampleRate : 24000
}

const encoderWorklets: Record<string, EncoderWorklet> = {}
const decoderWorklets: Record<string, DecoderWorklet> = {}

function startWorklet<T extends CodecWorklet, O extends Uint8Array | Float32Array>(
  streamId: string,
  worklet: T,
  outputFunction: (worklet: T) => O,
  messageBuilder: (output: O, streamId: string) => VoiceChatWorkerResponse
) {
  worklet.working = true

  function doWork() {
    worklet.lastWorkTime = Date.now()

    let output = outputFunction(worklet)

    if (output) {
      if (output instanceof Uint8Array) {
        output = Uint8Array.from(output) as O
      } else {
        output = Float32Array.from(output) as O
      }

      postMessage(messageBuilder(output, streamId), [output.buffer])
      setTimeout(doWork, 0)
    } else {
      worklet.working = false
    }
  }

  setTimeout(doWork, 0)
}

onmessage = function (e) {
  if (e.data.topic === RequestTopic.ENCODE) {
    processEncodeMessage(e)
  }

  if (e.data.topic === RequestTopic.DECODE) {
    processDecodeMessage(e)
  }

  if (e.data.topic === RequestTopic.DESTROY_DECODER) {
    const { streamId } = e.data
    destroyWorklet(decoderWorklets, streamId)
  }

  if (e.data.topic === RequestTopic.DESTROY_ENCODER) {
    const { streamId } = e.data
    destroyWorklet(encoderWorklets, streamId)
  }
}

function processDecodeMessage(e: MessageEvent) {
  const sampleRate = getSampleRate(e)
  const decoderWorklet = (decoderWorklets[e.data.streamId] = decoderWorklets[e.data.streamId] || {
    working: false,
    decoder: new libopus.Decoder(1, sampleRate),
    lastWorkTime: Date.now(),
    destroy: function () {
      this.decoder.destroy()
    }
  })

  decoderWorklet.decoder.input(e.data.encoded)

  if (!decoderWorklet.working) {
    startWorklet(
      e.data.streamId,
      decoderWorklet,
      (worklet) => worklet.decoder.output(),
      (output, streamId) => ({
        topic: ResponseTopic.DECODE,
        streamId,
        samples: toFloat32Samples(output)
      })
    )
  }
}

function processEncodeMessage(e: MessageEvent) {
  const sampleRate = getSampleRate(e)
  const encoderWorklet = (encoderWorklets[e.data.streamId] = encoderWorklets[e.data.streamId] || {
    working: false,
    encoder: new libopus.Encoder(1, sampleRate, OPUS_BITS_PER_SECOND, OPUS_FRAME_SIZE_MS, true),
    lastWorkTime: Date.now(),
    destroy: function () {
      this.encoder.destroy()
    }
  })

  const samples = toInt16Samples(resampleIfNecessary(e.data.samples, e.data.sampleRate, e.data.inputSampleRate))

  encoderWorklet.encoder.input(samples)

  if (!encoderWorklet.working) {
    startWorklet(
      e.data.streamId,
      encoderWorklet,
      (worklet) => worklet.encoder.output(),
      (output, streamId) => ({ topic: ResponseTopic.ENCODE, streamId: streamId, encoded: output })
    )
  }
}

function resampleIfNecessary(floatSamples: Float32Array, targetSampleRate: number, inputSampleRate?: number) {
  if (inputSampleRate && inputSampleRate !== targetSampleRate) {
    const resampler = new Resampler(inputSampleRate, targetSampleRate, 1)
    return resampler.resample(floatSamples)
  } else {
    return floatSamples
  }
}
function toInt16Samples(floatSamples: Float32Array) {
  return Int16Array.from(floatSamples, (floatSample) => {
    let val = Math.floor(32767 * floatSample)
    val = Math.min(32767, val)
    val = Math.max(-32768, val)
    return val
  })
}

function toFloat32Samples(intSamples: Int16Array) {
  return Float32Array.from(intSamples, (intSample) => {
    let floatValue = intSample >= 0 ? intSample / 32767 : intSample / 32768
    return Math.fround(floatValue)
  })
}

function destroyWorklet(worklets: Record<string, CodecWorklet>, workletId: string) {
  worklets[workletId]?.destroy()
  delete worklets[workletId]
}
