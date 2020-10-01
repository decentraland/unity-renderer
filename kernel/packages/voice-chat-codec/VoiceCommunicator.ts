import { VoiceChatCodecWorkerMain, EncodeStream } from './VoiceChatCodecWorkerMain'
import { RingBuffer } from 'atomicHelpers/RingBuffer'
import { SortedLimitedQueue } from 'atomicHelpers/SortedLimitedQueue'
import { defer } from 'atomicHelpers/defer'
import defaultLogger from 'shared/logger'
import {
  VOICE_CHAT_SAMPLE_RATE,
  OPUS_FRAME_SIZE_MS,
  OUTPUT_NODE_BUFFER_SIZE,
  OPUS_SAMPLES_PER_FRAME,
  INPUT_NODE_BUFFER_SIZE
} from './constants'
import { parse, write } from 'sdp-transform'

export type AudioCommunicatorChannel = {
  send(data: Uint8Array): any
}

export type StreamPlayingListener = (streamId: string, playing: boolean) => any
export type StreamRecordingListener = (recording: boolean) => any

type EncodedFrame = {
  order: number
  frame: Uint8Array
}

type VoiceOutput = {
  encodedFramesQueue: SortedLimitedQueue<EncodedFrame>
  decodedBuffer: RingBuffer<Float32Array>
  scriptProcessor: ScriptProcessorNode
  panNode: PannerNode
  spatialParams: VoiceSpatialParams
  playing: boolean
  lastUpdateTime: number
}

type VoiceInput = {
  encodeInputProcessor: ScriptProcessorNode
  inputStream: MediaStreamAudioSourceNode
  recordingContext: AudioContext
  encodeStream: EncodeStream
}

export type VoiceCommunicatorOptions = {
  sampleRate?: number
  channelBufferSize?: number
  maxDistance?: number
  refDistance?: number
  initialListenerParams?: VoiceSpatialParams
  panningModel?: PanningModelType
  distanceModel?: DistanceModelType
  loopbackAudioElement?: HTMLAudioElement
}

export type VoiceSpatialParams = {
  position: [number, number, number]
  orientation: [number, number, number]
}

export class VoiceCommunicator {
  private context: AudioContext
  private outputGainNode: GainNode
  private outputStreamNode?: MediaStreamAudioDestinationNode
  private loopbackConnections?: { src: RTCPeerConnection; dst: RTCPeerConnection }
  private input?: VoiceInput
  private voiceChatWorkerMain: VoiceChatCodecWorkerMain
  private outputs: Record<string, VoiceOutput> = {}

  private streamPlayingListeners: StreamPlayingListener[] = []
  private streamRecordingListeners: StreamRecordingListener[] = []

  private readonly sampleRate: number
  private readonly channelBufferSize: number
  private readonly outputExpireTime = 60 * 1000

  private pauseRequested: boolean = false
  private inputSamplesCount: number = 0

  constructor(
    private selfId: string,
    private channel: AudioCommunicatorChannel,
    private options: VoiceCommunicatorOptions
  ) {
    this.sampleRate = this.options.sampleRate ?? VOICE_CHAT_SAMPLE_RATE
    this.channelBufferSize = this.options.channelBufferSize ?? 2.0

    this.context = new AudioContext({ sampleRate: this.sampleRate })

    this.outputGainNode = this.context.createGain()

    if (options.loopbackAudioElement) {
      // Workaround for echo cancellation. See: https://bugs.chromium.org/p/chromium/issues/detail?id=687574#c71
      this.outputStreamNode = this.context.createMediaStreamDestination()
      this.outputGainNode.connect(this.outputStreamNode)
      this.loopbackConnections = this.createRTCLoopbackConnection()
    } else {
      this.outputGainNode.connect(this.context.destination)
    }

    if (this.options.initialListenerParams) {
      this.setListenerSpatialParams(this.options.initialListenerParams)
    }

    this.voiceChatWorkerMain = new VoiceChatCodecWorkerMain()

    this.startOutputsExpiration()
  }

  public setSelfId(selfId: string) {
    this.voiceChatWorkerMain.destroyEncodeStream(this.selfId)
    this.selfId = selfId
    if (this.input) {
      this.input.encodeStream = this.createInputEncodeStream(
        this.input.recordingContext,
        this.input.encodeInputProcessor
      )
    }
  }

  public addStreamPlayingListener(listener: StreamPlayingListener) {
    this.streamPlayingListeners.push(listener)
  }

  public addStreamRecordingListener(listener: StreamRecordingListener) {
    this.streamRecordingListeners.push(listener)
  }

  public hasInput() {
    return !!this.input
  }

  playEncodedAudio(src: string, relativePosition: VoiceSpatialParams, encoded: Uint8Array, time: number) {
    if (!this.outputs[src]) {
      this.createOutput(src, relativePosition)
    } else {
      this.outputs[src].lastUpdateTime = Date.now()
      this.setVoiceRelativePosition(src, relativePosition)
    }

    this.outputs[src].encodedFramesQueue.queue({ frame: encoded, order: time })
  }

  setListenerSpatialParams(spatialParams: VoiceSpatialParams) {
    const listener = this.context.listener
    listener.setPosition(spatialParams.position[0], spatialParams.position[1], spatialParams.position[2])
    listener.setOrientation(
      spatialParams.orientation[0],
      spatialParams.orientation[1],
      spatialParams.orientation[2],
      0,
      1,
      0
    )
  }

  updatePannerNodeParameters(src: string) {
    const panNode = this.outputs[src].panNode
    const spatialParams = this.outputs[src].spatialParams

    panNode.positionX.value = spatialParams.position[0]
    panNode.positionY.value = spatialParams.position[1]
    panNode.positionZ.value = spatialParams.position[2]
    panNode.orientationX.value = spatialParams.orientation[0]
    panNode.orientationY.value = spatialParams.orientation[1]
    panNode.orientationZ.value = spatialParams.orientation[2]
  }

  setVolume(value: number) {
    this.outputGainNode.gain.value = value
  }

  createOutputNodes(src: string): { scriptProcessor: ScriptProcessorNode; panNode: PannerNode } {
    const scriptProcessor = this.createScriptOutputFor(src)
    const panNode = this.context.createPanner()
    panNode.coneInnerAngle = 180
    panNode.coneOuterAngle = 360
    panNode.coneOuterGain = 0.9
    panNode.maxDistance = this.options.maxDistance ?? 10000
    panNode.refDistance = this.options.refDistance ?? 5
    panNode.panningModel = this.options.panningModel ?? 'equalpower'
    panNode.distanceModel = this.options.distanceModel ?? 'inverse'
    panNode.rolloffFactor = 1.0
    scriptProcessor.connect(panNode)
    panNode.connect(this.outputGainNode)

    return { scriptProcessor, panNode }
  }

  createScriptOutputFor(src: string) {
    const bufferSize = OUTPUT_NODE_BUFFER_SIZE
    const processor = this.context.createScriptProcessor(bufferSize, 0, 1)
    processor.onaudioprocess = (ev) => {
      const data = ev.outputBuffer.getChannelData(0)

      data.fill(0)
      if (this.outputs[src]) {
        const wasPlaying = this.outputs[src].playing
        const minReadCount = wasPlaying ? 0 : OUTPUT_NODE_BUFFER_SIZE - 1
        if (this.outputs[src].decodedBuffer.readAvailableCount() > minReadCount) {
          data.set(this.outputs[src].decodedBuffer.read(data.length))
          if (!wasPlaying) {
            this.changePlayingStatus(src, true)
          }
        } else {
          if (wasPlaying) {
            this.changePlayingStatus(src, false)
          }
        }
      }
    }

    return processor
  }

  changePlayingStatus(streamId: string, playing: boolean) {
    this.outputs[streamId].playing = playing
    this.outputs[streamId].lastUpdateTime = Date.now()
    // Listeners could be long running, so we defer the execution of them
    defer(() => {
      this.streamPlayingListeners.forEach((listener) => listener(streamId, playing))
    })
  }

  setInputStream(stream: MediaStream) {
    if (this.input) {
      this.voiceChatWorkerMain.destroyEncodeStream(this.selfId)
      if (this.input.recordingContext !== this.context) {
        this.input.recordingContext.close().catch((e) => defaultLogger.error('Error closing recording context', e))
      }
    }

    try {
      this.input = this.createInputFor(stream, this.context)
    } catch (e) {
      // If this fails, then it most likely it is because the sample rate of the stream is incompatible with the context's, so we create a special context for recording
      if (e.message.includes('sample-rate is currently not supported')) {
        const recordingContext = new AudioContext()
        this.input = this.createInputFor(stream, recordingContext)
      } else {
        throw e
      }
    }
  }

  start() {
    this.pauseRequested = false
    if (this.input) {
      this.input.encodeInputProcessor.connect(this.input.recordingContext.destination)
      this.input.inputStream.connect(this.input.encodeInputProcessor)
      this.notifyRecording(true)
    } else {
      this.notifyRecording(false)
    }
  }

  pause() {
    this.pauseRequested = true
  }

  private createRTCLoopbackConnection(
    currentRetryNumber: number = 0
  ): { src: RTCPeerConnection; dst: RTCPeerConnection } {
    const src = new RTCPeerConnection()
    const dst = new RTCPeerConnection()

    let retryNumber = currentRetryNumber

    ;(async () => {
      // When having an error, we retry in a couple of seconds. Up to 10 retries.
      src.onconnectionstatechange = (e) => {
        if (
          src.connectionState === 'closed' ||
          src.connectionState === 'disconnected' ||
          (src.connectionState === 'failed' && currentRetryNumber < 10)
        ) {
          // Just in case, we close connections to free resources
          this.closeLoopbackConnections()
          this.loopbackConnections = this.createRTCLoopbackConnection(retryNumber)
        } else if (src.connectionState === 'connected') {
          // We reset retry number when the connection succeeds
          retryNumber = 0
        }
      }

      src.onicecandidate = (e) => e.candidate && dst.addIceCandidate(new RTCIceCandidate(e.candidate))
      dst.onicecandidate = (e) => e.candidate && src.addIceCandidate(new RTCIceCandidate(e.candidate))

      dst.ontrack = (e) => (this.options.loopbackAudioElement!.srcObject = e.streams[0])

      this.outputStreamNode!.stream.getTracks().forEach((track) => src.addTrack(track, this.outputStreamNode!.stream))

      const offer = await src.createOffer()

      await src.setLocalDescription(offer)

      await dst.setRemoteDescription(offer)
      const answer = await dst.createAnswer()

      const answerSdp = parse(answer.sdp!)

      answerSdp.media[0].fmtp[0].config = 'ptime=5;stereo=1;sprop-stereo=1;maxaveragebitrate=256000'

      answer.sdp = write(answerSdp)

      await dst.setLocalDescription(answer)

      await src.setRemoteDescription(answer)
    })().catch((e) => {
      defaultLogger.error('Error creating loopback connection', e)
    })

    return { src, dst }
  }

  private closeLoopbackConnections() {
    if (this.loopbackConnections) {
      const { src, dst } = this.loopbackConnections

      src.close()
      dst.close()
    }
  }

  private disconnectInput() {
    this.input?.inputStream.disconnect()

    this.input?.encodeInputProcessor.disconnect()

    this.notifyRecording(false)
  }

  private createOutput(src: string, relativePosition: VoiceSpatialParams) {
    const nodes = this.createOutputNodes(src)
    this.outputs[src] = {
      encodedFramesQueue: new SortedLimitedQueue(
        Math.ceil((this.channelBufferSize * 1000) / OPUS_FRAME_SIZE_MS),
        (frameA, frameB) => frameA.order - frameB.order
      ),
      decodedBuffer: new RingBuffer(Math.floor(this.channelBufferSize * this.sampleRate), Float32Array),
      playing: false,
      spatialParams: relativePosition,
      lastUpdateTime: Date.now(),
      ...nodes
    }

    const readEncodedBufferLoop = async () => {
      if (this.outputs[src]) {
        // Leaving this buffer to fill too much causes a great deal of latency, so we leave this as 1 for now. In the future, we should adjust this based
        // on packet loss or something like that
        const framesToRead = 1

        const frames = await this.outputs[src].encodedFramesQueue.dequeueItemsWhenAvailable(framesToRead, 2000)

        if (frames.length > 0) {
          let stream = this.voiceChatWorkerMain.decodeStreams[src]

          if (!stream) {
            stream = this.voiceChatWorkerMain.getOrCreateDecodeStream(src, this.sampleRate)

            stream.addAudioDecodedListener((samples) => {
              this.outputs[src].lastUpdateTime = Date.now()
              this.outputs[src].decodedBuffer.write(samples)
            })
          }

          frames.forEach((it) => stream.decode(it.frame))
        }

        await readEncodedBufferLoop()
      }
    }

    readEncodedBufferLoop().catch((e) => defaultLogger.log('Error while reading encoded buffer of ' + src, e))
  }

  private createInputFor(stream: MediaStream, context: AudioContext) {
    const streamSource = context.createMediaStreamSource(stream)
    const inputProcessor = context.createScriptProcessor(INPUT_NODE_BUFFER_SIZE, 1, 1)
    return {
      recordingContext: context,
      encodeStream: this.createInputEncodeStream(context, inputProcessor),
      encodeInputProcessor: inputProcessor,
      inputStream: streamSource
    }
  }

  private createInputEncodeStream(recordingContext: AudioContext, encodeInputProcessor: ScriptProcessorNode) {
    const encodeStream = this.voiceChatWorkerMain.getOrCreateEncodeStream(
      this.selfId,
      this.sampleRate,
      recordingContext.sampleRate
    )

    encodeStream.addAudioEncodedListener((data) => this.channel.send(data))

    encodeInputProcessor.onaudioprocess = (e) => {
      const buffer = e.inputBuffer
      let data = buffer.getChannelData(0)

      if (this.pauseRequested) {
        // We try to use as many samples as we can that would complete some frames
        const samplesToUse =
          Math.floor(data.length / OPUS_SAMPLES_PER_FRAME) * OPUS_SAMPLES_PER_FRAME +
          OPUS_SAMPLES_PER_FRAME -
          (this.inputSamplesCount % OPUS_SAMPLES_PER_FRAME)
        data = data.slice(0, samplesToUse)
        this.disconnectInput()
        this.pauseRequested = false
      }

      encodeStream.encode(data)
      this.inputSamplesCount += data.length
    }

    return encodeStream
  }

  private setVoiceRelativePosition(src: string, spatialParams: VoiceSpatialParams) {
    this.outputs[src].spatialParams = spatialParams
    this.updatePannerNodeParameters(src)
  }

  private notifyRecording(recording: boolean) {
    this.streamRecordingListeners.forEach((listener) => listener(recording))
  }

  private startOutputsExpiration() {
    const expireOutputs = () => {
      Object.keys(this.outputs).forEach((outputId) => {
        const output = this.outputs[outputId]
        if (Date.now() - output.lastUpdateTime > this.outputExpireTime) {
          this.destroyOutput(outputId)
        }
      })

      setTimeout(expireOutputs, 2000)
    }

    setTimeout(expireOutputs, 0)
  }

  private destroyOutput(outputId: string) {
    this.disconnectOutputNodes(outputId)

    this.voiceChatWorkerMain.destroyDecodeStream(outputId)

    delete this.outputs[outputId]
  }

  private disconnectOutputNodes(outputId: string) {
    const output = this.outputs[outputId]
    output.panNode.disconnect()
    output.scriptProcessor.disconnect()
  }
}
