import { RingBuffer } from 'atomicHelpers/RingBuffer'
import { OPUS_SAMPLES_PER_FRAME, VOICE_CHAT_SAMPLE_RATE } from './constants'
import { InputWorkletRequestTopic, OutputWorkletRequestTopic } from './types'

export interface AudioWorkletProcessor {
  readonly port: MessagePort
  process(inputs: Float32Array[][], outputs: Float32Array[][], parameters: Record<string, Float32Array>): boolean
}

declare var AudioWorkletProcessor: {
  prototype: AudioWorkletProcessor
  new (options?: AudioWorkletNodeOptions): AudioWorkletProcessor
}

declare function registerProcessor(
  name: string,
  processorCtor: (new (options?: AudioWorkletNodeOptions) => AudioWorkletProcessor) & {
    parameterDescriptors?: AudioParamDescriptor[]
  }
): void

enum InputProcessorStatus {
  RECORDING,
  PAUSE_REQUESTED,
  PAUSED
}

class InputProcessor extends AudioWorkletProcessor {
  status: InputProcessorStatus = InputProcessorStatus.PAUSED
  inputSamplesCount: number = 0

  constructor(...args: any[]) {
    super(...args)

    this.port.onmessage = (e) => {
      if (e.data.topic === InputWorkletRequestTopic.PAUSE) {
        this.status = InputProcessorStatus.PAUSE_REQUESTED
      }

      if (e.data.topic === InputWorkletRequestTopic.RESUME) {
        this.status = InputProcessorStatus.RECORDING
        this.notify(InputWorkletRequestTopic.ON_RECORDING)
      }
    }
  }

  process(inputs: Float32Array[][], outputs: Float32Array[][], parameters: Record<string, Float32Array>) {
    if (this.status === InputProcessorStatus.PAUSED) return true
    let data = inputs[0][0]

    if (this.status === InputProcessorStatus.PAUSE_REQUESTED) {
      // We try to use as many samples as we can that would complete some frames
      const samplesToUse =
        Math.floor(data.length / OPUS_SAMPLES_PER_FRAME) * OPUS_SAMPLES_PER_FRAME +
        OPUS_SAMPLES_PER_FRAME -
        (this.inputSamplesCount % OPUS_SAMPLES_PER_FRAME)

      // If we still don't have enough samples to complete a frame, we cannot pause recording yet.
      if (samplesToUse <= data.length) {
        data = data.slice(0, samplesToUse)
        this.status = InputProcessorStatus.PAUSED
        this.notify(InputWorkletRequestTopic.ON_PAUSED)
      }
    }

    this.inputSamplesCount += data.length
    this.sendDataToEncode(data)

    return true
  }

  notify(notification: InputWorkletRequestTopic) {
    this.port.postMessage({ topic: notification })
  }

  private sendDataToEncode(data: Float32Array) {
    this.port.postMessage({ topic: InputWorkletRequestTopic.ENCODE, samples: data }, [data.buffer])
  }
}

class OutputProcessor extends AudioWorkletProcessor {
  buffer: RingBuffer<Float32Array>
  playing: boolean = false
  bufferLength: number
  sampleRate: number
  readStartSamplesCount: number

  constructor(options?: AudioWorkletNodeOptions) {
    super(options)
    this.bufferLength = options?.processorOptions.channelBufferSize ?? 2.0
    this.sampleRate = options?.processorOptions.sampleRate ?? VOICE_CHAT_SAMPLE_RATE
    this.buffer = new RingBuffer(Math.floor(this.bufferLength * this.sampleRate), Float32Array)
    this.readStartSamplesCount = (options?.processorOptions.readStartLength ?? 0.2) * this.sampleRate

    this.port.onmessage = (e) => {
      if (e.data.topic === OutputWorkletRequestTopic.WRITE_SAMPLES) {
        this.buffer.write(e.data.samples)
      }
    }
  }

  process(inputs: Float32Array[][], outputs: Float32Array[][], parameters: Record<string, Float32Array>) {
    const data = outputs[0][0]

    data.fill(0)
    const wasPlaying = this.playing
    const minReadCount = wasPlaying ? data.length - 1 : this.readStartSamplesCount
    if (this.buffer.readAvailableCount() > minReadCount) {
      data.set(this.buffer.read(data.length))
      if (!wasPlaying) {
        this.changePlayingStatus(true)
      }
    } else {
      if (wasPlaying) {
        this.changePlayingStatus(false)
      }
    }
    return true
  }

  changePlayingStatus(playing: boolean) {
    this.playing = playing
    this.port.postMessage({ topic: OutputWorkletRequestTopic.STREAM_PLAYING, playing })
  }
}

registerProcessor('inputProcessor', InputProcessor)

registerProcessor('outputProcessor', OutputProcessor)
