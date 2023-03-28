import { VoiceChatCodecWorkerMain, EncodeStream } from 'voice-chat-codec/VoiceChatCodecWorkerMain'
import { SortedLimitedQueue } from 'lib/data-structures/SortedLimitedQueue'
import defaultLogger from 'lib/logger'
import { VOICE_CHAT_SAMPLE_RATE, OPUS_FRAME_SIZE_MS } from 'voice-chat-codec/constants'
import { parse, write } from 'sdp-transform'
import { InputWorkletRequestTopic, OutputWorkletRequestTopic } from 'voice-chat-codec/types'
import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'

// eslint-disable-next-line @typescript-eslint/no-var-requires
const workletWorkerRaw = require('../../../static/voice-chat-codec/audioWorkletProcessors.js.txt')
const workletWorkerUrl = URL.createObjectURL(new Blob([workletWorkerRaw], { type: 'application/javascript' }))

export type AudioCommunicatorChannel = {
  send(data: rfc4.Voice): any
}

export type StreamPlayingListener = (streamId: string, playing: boolean) => any
export type StreamRecordingListener = (recording: boolean) => any
export type StreamRecordingErrorListener = (message: string) => any

type VoiceOutput = {
  encodedFramesQueue: SortedLimitedQueue<rfc4.Voice>
  workletNode?: AudioWorkletNode
  panNode?: PannerNode
  spatialParams: VoiceSpatialParams
  lastUpdateTime: number
  playing: boolean
  lastDecodedFrameOrder?: number
}

type OutputStats = {
  lostFrames: number
  skippedFramesNotQueued: number
  skippedFramesQueued: number
}

type VoiceInput = {
  workletNode: AudioWorkletNode
  inputStream: MediaStreamAudioSourceNode
  recordingContext: AudioContextWithInitPromise
  encodeStream: EncodeStream
}

export type VoiceCommunicatorOptions = {
  sampleRate?: number
  outputBufferLength?: number
  maxDistance?: number
  refDistance?: number
  initialListenerParams?: VoiceSpatialParams
  panningModel?: PanningModelType
  distanceModel?: DistanceModelType
  loopbackAudioElement?: HTMLAudioElement
  volume?: number
  mute?: boolean
}

export type VoiceSpatialParams = {
  position: [number, number, number]
  orientation: [number, number, number]
}

type AudioContextWithInitPromise = [AudioContext, Promise<any>]

const SELF_STREAM_ID = 'localhost'

export class VoiceCommunicator {
  private contextWithInitPromise: AudioContextWithInitPromise
  private outputGainNode: GainNode
  private outputStreamNode?: MediaStreamAudioDestinationNode
  private loopbackConnections?: { src: RTCPeerConnection; dst: RTCPeerConnection }
  private input?: VoiceInput
  private voiceChatWorkerMain: VoiceChatCodecWorkerMain
  private outputs: Record<string, VoiceOutput> = {}

  private outputStats: Record<string, OutputStats> = {}

  private streamPlayingListeners: StreamPlayingListener[] = []
  private streamRecordingListeners: StreamRecordingListener[] = []
  private streamRecordingErrorListeners: StreamRecordingErrorListener[] = []

  private readonly sampleRate: number
  private readonly outputBufferLength: number
  private readonly outputExpireTime = 60 * 1000

  private inputFramesIndex = 0

  private checkStateTimeout: any | undefined = undefined

  private get context(): AudioContext {
    return this.contextWithInitPromise[0]
  }

  constructor(private channel: AudioCommunicatorChannel, private options: VoiceCommunicatorOptions) {
    this.sampleRate = this.options.sampleRate ?? VOICE_CHAT_SAMPLE_RATE
    this.outputBufferLength = this.options.outputBufferLength ?? 2.0

    this.contextWithInitPromise = this.createContext({ sampleRate: this.sampleRate })

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

  public addStreamPlayingListener(listener: StreamPlayingListener) {
    this.streamPlayingListeners.push(listener)
  }

  public addStreamRecordingListener(listener: StreamRecordingListener) {
    this.streamRecordingListeners.push(listener)
  }

  public addStreamRecordingErrorListener(listener: StreamRecordingErrorListener) {
    this.streamRecordingErrorListeners.push(listener)
  }

  public hasInput() {
    return !!this.input
  }

  public statsFor(outputId: string) {
    if (!this.outputStats[outputId]) {
      this.outputStats[outputId] = {
        lostFrames: 0,
        skippedFramesNotQueued: 0,
        skippedFramesQueued: 0
      }
    }

    return this.outputStats[outputId]
  }

  async playEncodedAudio(src: string, relativePosition: VoiceSpatialParams, encoded: rfc4.Voice) {
    if (!this.outputs[src]) {
      await this.createOutput(src, relativePosition)
    } else {
      this.outputs[src].lastUpdateTime = Date.now()
      this.setVoiceRelativePosition(src, relativePosition)
    }

    const output = this.outputs[src]

    if (output.lastDecodedFrameOrder && output.lastDecodedFrameOrder > encoded.index) {
      this.statsFor(src).skippedFramesNotQueued += 1
      return
    }

    const discarded = output.encodedFramesQueue.queue(encoded)
    if (discarded) {
      this.statsFor(src).skippedFramesQueued += 1
    }
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

    if (panNode) {
      panNode.positionX.value = spatialParams.position[0]
      panNode.positionY.value = spatialParams.position[1]
      panNode.positionZ.value = spatialParams.position[2]
      panNode.orientationX.value = spatialParams.orientation[0]
      panNode.orientationY.value = spatialParams.orientation[1]
      panNode.orientationZ.value = spatialParams.orientation[2]
    }
  }

  setVolume(value: number) {
    this.options.volume = value
    const muted = this.options.mute ?? false
    if (!muted) {
      this.outputGainNode.gain.value = value
    }
  }

  setMute(mute: boolean) {
    this.options.mute = mute
    this.outputGainNode.gain.value = mute ? 0 : this.options.volume ?? 1
  }

  createWorkletFor(src: string) {
    const workletNode = new AudioWorkletNode(this.context, 'outputProcessor', {
      numberOfInputs: 0,
      numberOfOutputs: 1,
      processorOptions: { sampleRate: this.sampleRate, bufferLength: this.outputBufferLength }
    })

    workletNode.port.onmessage = (e) => {
      if (e.data.topic === OutputWorkletRequestTopic.STREAM_PLAYING) {
        if (this.outputs[src]) {
          this.outputs[src].playing = e.data.playing
        }
        this.streamPlayingListeners.forEach((listener) => listener(src, e.data.playing))
      }
    }

    return workletNode
  }

  async setInputStream(stream: MediaStream) {
    if (this.input) {
      this.voiceChatWorkerMain.destroyEncodeStream(SELF_STREAM_ID)
      if (this.input.recordingContext[0] !== this.context) {
        this.input.recordingContext[0].close().catch((e) => defaultLogger.error('Error closing recording context', e))
      }
    }

    try {
      this.input = await this.createInputFor(stream, this.contextWithInitPromise)
    } catch (e: any) {
      // If this fails, then it most likely it is because the sample rate of the stream is incompatible with the context's, so we create a special context for recording
      if (e.message.includes('sample-rate is currently not supported')) {
        const recordingContext = this.createContext()
        this.input = await this.createInputFor(stream, recordingContext)
      } else {
        throw e
      }
    }
  }

  checkStatusTimeout() {
    if (this.checkStateTimeout === undefined) {
      this.checkStateTimeout = setTimeout(() => {
        this.sendToInputWorklet(InputWorkletRequestTopic.CHECK_STATUS)
        this.checkStateTimeout = undefined
      }, 1200)
    }
  }

  start() {
    if (this.input) {
      this.input.workletNode.connect(this.input.recordingContext[0].destination)
      this.sendToInputWorklet(InputWorkletRequestTopic.RESUME)
      this.checkStatusTimeout()
    } else {
      this.notifyRecording(false)
    }
  }

  pause() {
    if (this.input) {
      this.sendToInputWorklet(InputWorkletRequestTopic.PAUSE)
      this.checkStatusTimeout()
    } else {
      this.notifyRecording(false)
    }
  }

  private async createOutputNodes(src: string): Promise<{ workletNode: AudioWorkletNode; panNode: PannerNode }> {
    await this.contextWithInitPromise[1]
    const workletNode = this.createWorkletFor(src)
    const panNode = this.context.createPanner()
    panNode.coneInnerAngle = 180
    panNode.coneOuterAngle = 360
    panNode.coneOuterGain = 0.9
    panNode.maxDistance = this.options.maxDistance ?? 10000
    panNode.refDistance = this.options.refDistance ?? 5
    panNode.panningModel = this.options.panningModel ?? 'equalpower'
    panNode.distanceModel = this.options.distanceModel ?? 'inverse'
    panNode.rolloffFactor = 1.0
    workletNode.connect(panNode)
    panNode.connect(this.outputGainNode)

    return { workletNode, panNode }
  }

  private sendToInputWorklet(topic: InputWorkletRequestTopic) {
    this.input?.workletNode.port.postMessage({ topic: topic })
  }

  private createRTCLoopbackConnection(currentRetryNumber: number = 0): {
    src: RTCPeerConnection
    dst: RTCPeerConnection
  } {
    const src = new RTCPeerConnection()
    const dst = new RTCPeerConnection()

    let retryNumber = currentRetryNumber

    ;(async () => {
      // When having an error, we retry in a couple of seconds. Up to 10 retries.
      src.onconnectionstatechange = (_e) => {
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
      src.close()
      dst.close()
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

  private async createOutput(src: string, relativePosition: VoiceSpatialParams) {
    this.outputs[src] = {
      encodedFramesQueue: new SortedLimitedQueue(
        Math.ceil((this.outputBufferLength * 1000) / OPUS_FRAME_SIZE_MS),
        (frameA, frameB) => frameA.index - frameB.index
      ),
      spatialParams: relativePosition,
      lastUpdateTime: Date.now(),
      playing: false
    }

    const { workletNode, panNode } = await this.createOutputNodes(src)

    this.outputs[src].workletNode = workletNode
    this.outputs[src].panNode = panNode

    const readEncodedBufferLoop = async () => {
      if (this.outputs[src]) {
        // We use three frames (120ms) as a jitter buffer. This is not mutch, but we don't want to add much latency. In the future we should maybe make this dynamic based on packet loss
        const framesToRead = this.outputs[src].playing ? 3 : 1

        const frames = await this.outputs[src].encodedFramesQueue.dequeueItemsWhenAvailable(framesToRead, 2000)

        if (frames.length > 0) {
          this.countLostFrames(src, frames)
          let stream = this.voiceChatWorkerMain.decodeStreams[src]

          if (!stream) {
            stream = this.voiceChatWorkerMain.getOrCreateDecodeStream(src, this.sampleRate)

            stream.addAudioDecodedListener((samples) => {
              this.outputs[src].lastUpdateTime = Date.now()
              this.outputs[src].workletNode?.port.postMessage(
                { topic: OutputWorkletRequestTopic.WRITE_SAMPLES, samples },
                [samples.buffer]
              )
            })
          }

          frames.forEach((it) => stream.decode(it.encodedSamples))
          this.outputs[src].lastDecodedFrameOrder = frames[frames.length - 1].index
        }

        await readEncodedBufferLoop()
      }
    }

    readEncodedBufferLoop().catch((e) => defaultLogger.log('Error while reading encoded buffer of ' + src, e))
  }

  private countLostFrames(src: string, frames: rfc4.Voice[]) {
    // We can know a frame is lost if we have a missing frame index
    let lostFrames = 0
    if (this.outputs[src].lastDecodedFrameOrder && this.outputs[src].lastDecodedFrameOrder! < frames[0].index) {
      // We count the missing frame indexes from the last decoded frame. If there are no missin frames, 0 is added
      lostFrames += frames[0].index - this.outputs[src].lastDecodedFrameOrder! - 1
    }

    for (let i = 0; i < frames.length - 1; i++) {
      // We count the missing frame indexes in the current frames to decode. If there are no missin frames, 0 is added
      lostFrames += frames[i + 1].index - frames[i].index - 1
    }

    this.statsFor(src).lostFrames += lostFrames
  }

  private async createInputFor(stream: MediaStream, context: AudioContextWithInitPromise) {
    await context[1]
    const streamSource = context[0].createMediaStreamSource(stream)
    const workletNode = new AudioWorkletNode(context[0], 'inputProcessor', {
      numberOfInputs: 1,
      numberOfOutputs: 1
    })

    streamSource.connect(workletNode)
    return {
      recordingContext: context,
      encodeStream: this.createInputEncodeStream(context[0], workletNode),
      workletNode,
      inputStream: streamSource
    }
  }

  private createInputEncodeStream(recordingContext: AudioContext, workletNode: AudioWorkletNode) {
    const encodeStream = this.voiceChatWorkerMain.getOrCreateEncodeStream(
      SELF_STREAM_ID,
      this.sampleRate,
      recordingContext.sampleRate
    )

    encodeStream.addAudioEncodedListener((data) => {
      this.inputFramesIndex += 1
      this.channel.send({
        encodedSamples: data,
        index: this.inputFramesIndex,
        codec: rfc4.Voice_VoiceCodec.VC_OPUS
      })
    })

    workletNode.port.onmessage = (e) => {
      if (e.data.topic === InputWorkletRequestTopic.ENCODE) {
        encodeStream.encode(e.data.samples)
      }

      if (e.data.topic === InputWorkletRequestTopic.ON_PAUSED) {
        this.notifyRecording(false)
        this.input?.workletNode.disconnect()
      }

      if (e.data.topic === InputWorkletRequestTopic.ON_RECORDING) {
        this.notifyRecording(true)
      }

      if (e.data.topic === InputWorkletRequestTopic.TIMEOUT) {
        this.notifyRecordingError('Something went wrong with the microphone. No message was recorded.')
      }
    }

    workletNode.onprocessorerror = (_e) => {
      this.notifyRecording(false)
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

  private notifyRecordingError(message: string) {
    this.streamRecordingErrorListeners.forEach((listener) => listener(message))
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

  private createContext(contextOptions?: AudioContextOptions): AudioContextWithInitPromise {
    const aContext = new AudioContext(contextOptions)
    if (aContext.audioWorklet) {
      const workletInitializedPromise = aContext.audioWorklet
        .addModule(workletWorkerUrl)
        .catch((e) => defaultLogger.error('Error loading worklet modules: ', e))
      return [aContext, workletInitializedPromise]
    } else {
      // TODO: trackEvent('error_initializing_worklet') to gain visibility about how many times is this issue happening
      defaultLogger.error('Error loading worklet modules: audioWorklet undefined')
      return [aContext, Promise.resolve()]
    }
  }

  private destroyOutput(outputId: string) {
    this.disconnectOutputNodes(outputId)

    this.voiceChatWorkerMain.destroyDecodeStream(outputId)

    delete this.outputs[outputId]
  }

  private disconnectOutputNodes(outputId: string) {
    const output = this.outputs[outputId]
    output.panNode?.disconnect()
    output.workletNode?.disconnect()
  }
}
