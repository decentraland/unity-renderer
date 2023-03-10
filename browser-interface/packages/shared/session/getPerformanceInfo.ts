import mitt from 'mitt'
import { getAndClearOccurenceCounters, incrementCounter } from 'shared/analytics/occurences'
import { getExplorerVersion } from 'shared/meta/version'

const pingResponseTimes: number[] = []
const pingResponsePercentages: number[] = []
let kernelToRendererMessageCounter = 0
let rendererToKernelMessageCounter = 0
let receivedCommsMessagesCounter = 0
let sentCommsMessagesCounter = 0
let kernelToRendererMessageNativeCounter = 0
let lastReport = 0
let commsProtocol = ''

export function overrideCommsProtocol(newProtocol: string) {
  commsProtocol = newProtocol
}

export function measurePingTime(ping: number) {
  pingResponseTimes.push(ping)
}
export function measurePingTimePercentages(percent: number) {
  pingResponsePercentages.push(percent)
}

export function incrementMessageFromRendererToKernel() {
  rendererToKernelMessageCounter++
}

export function incrementMessageFromKernelToRenderer() {
  kernelToRendererMessageCounter++
}

export function incrementMessageFromKernelToRendererNative() {
  kernelToRendererMessageNativeCounter++
}

export function incrementCommsMessageReceived() {
  receivedCommsMessagesCounter++
}

export const commsPerfObservable = mitt<any>()

export function incrementCommsMessageReceivedByName(event: string) {
  commsPerfObservable.emit(event, { value: 1 })
  incrementCounter(`commMessage:${event}`)
  // NOTE:          ^^^^^^^^^^^ do NOT fix that typo
}

export function incrementAvatarSceneMessages(value: number) {
  commsPerfObservable.emit('avatar-renderer', { value })
}

export function incrementCommsMessageSent(_bytes: number) {
  sentCommsMessagesCounter++
}

export function getPerformanceInfo(data: {
  samples: string
  fpsIsCapped: boolean
  hiccupsInThousandFrames: number
  hiccupsTime: number
  totalTime: number
  estimatedAllocatedMemory: number
  estimatedTotalMemory: number

  // summarizes all frames
  gltfInProgress: number
  // summarizes all frames
  gltfFailed: number
  // summarizes all frames
  gltfCancelled: number
  // summarizes all frames
  gltfLoaded: number
  // summarizes all frames
  abInProgress: number
  // summarizes all frames
  abFailed: number
  // summarizes all frames
  abCancelled: number
  // summarizes all frames
  abLoaded: number
  // summarizes all frames
  gltfTexturesLoaded: number
  // summarizes all frames
  abTexturesLoaded: number
  // summarizes all frames
  promiseTexturesLoaded: number
  // summarizes all frames
  enqueuedMessages: number
  // summarizes all frames
  processedMessages: number

  // only samples last frame
  playerCount: number
  // only samples last frame
  loadRadius: number
  // only samples last frame
  sceneScores: Record<string, number>
  // only samples last frame
  drawCalls: number
  // only samples last frame
  memoryReserved: number
  // only samples last frame
  memoryUsage: number

  // summarizes all frames
  totalGCAlloc: number
}) {
  const entries: number[] = []
  const length = data.samples.length
  let sumTotalSamples = 0

  for (let i = 0; i < length; i++) {
    entries[i] = data.samples.charCodeAt(i)
    sumTotalSamples += entries[i]
  }

  const sorted = entries.sort((a, b) => a - b)

  const runtime = performance.now()
  const deltaTime = runtime - lastReport
  lastReport = runtime

  const memory = (performance as any).memory

  const jsHeapSizeLimit = memory?.jsHeapSizeLimit
  const totalJSHeapSize = memory?.totalJSHeapSize
  const usedJSHeapSize = memory?.usedJSHeapSize

  const isHidden = (globalThis as any).document?.hidden

  const explorerVersion = getExplorerVersion()

  const ret = {
    runtime,
    idle: isHidden,
    fps: (1000 * length) / sumTotalSamples,
    avg: sumTotalSamples / length,
    total: sumTotalSamples,
    len: length,
    min: sorted[0],
    p1: sorted[Math.ceil(length * 0.01)],
    p5: sorted[Math.ceil(length * 0.05)],
    p10: sorted[Math.ceil(length * 0.1)],
    p20: sorted[Math.ceil(length * 0.2)],
    p50: sorted[Math.ceil(length * 0.5)],
    p75: sorted[Math.ceil(length * 0.75)],
    p80: sorted[Math.ceil(length * 0.8)],
    p90: sorted[Math.ceil(length * 0.9)],
    p95: sorted[Math.ceil(length * 0.95)],
    p99: sorted[Math.ceil(length * 0.99)],
    max: sorted[length - 1],
    samples: JSON.stringify(entries),
    // chrome memory
    jsHeapSizeLimit,
    totalJSHeapSize,
    usedJSHeapSize,
    // unity memory
    estimatedAllocatedMemory: data.estimatedAllocatedMemory,
    estimatedTotalMemory: data.estimatedTotalMemory,
    estimatedMemoryPercent: data.estimatedTotalMemory
      ? data.estimatedAllocatedMemory / data.estimatedTotalMemory
      : null,
    // flags
    capped: data.fpsIsCapped,
    // hiccups
    hiccupsInThousandFrames: data.hiccupsInThousandFrames,
    hiccupsTime: data.hiccupsTime,
    totalTime: data.totalTime,
    // counters
    kernelToRendererMessageCounter,
    rendererToKernelMessageCounter,
    receivedCommsMessagesCounter,
    sentCommsMessagesCounter,
    kernelToRendererMessageNativeCounter,

    // reserved
    rendererAllocatedSize: 0,

    // delta time between performance reports
    deltaTime,

    // versions
    explorerVersion,
    kernelVersion: explorerVersion, // mantain compatibility
    rendererVersion: explorerVersion, // mantain compatibility

    // detailed profiling
    gltfInProgress: data.gltfInProgress,
    gltfFailed: data.gltfFailed,
    gltfCancelled: data.gltfCancelled,
    gltfLoaded: data.gltfLoaded,
    abInProgress: data.abInProgress,
    abFailed: data.abFailed,
    abCancelled: data.abCancelled,
    abLoaded: data.abLoaded,
    gltfTexturesLoaded: data.gltfTexturesLoaded,
    abTexturesLoaded: data.abTexturesLoaded,
    promiseTexturesLoaded: data.promiseTexturesLoaded,
    enqueuedMessages: data.enqueuedMessages,
    processedMessages: data.processedMessages,
    playerCount: data.playerCount,
    loadRadius: data.loadRadius,

    drawCalls: data.drawCalls,
    memoryReserved: data.memoryReserved,
    memoryUsage: data.memoryUsage,
    totalGCAlloc: data.totalGCAlloc,

    // replace sceneScores by the values only
    sceneScores: (data.sceneScores && Object.values(data.sceneScores)) || null,

    pingResponseTimes: pingResponseTimes.slice(),
    pingResponsePercentages: pingResponsePercentages.slice(),

    // misc metric counters
    metrics: getAndClearOccurenceCounters(),

    // Comms protocol
    commsProtocol: commsProtocol
  }

  pingResponseTimes.length = 0
  pingResponsePercentages.length = 0

  sentCommsMessagesCounter = 0
  receivedCommsMessagesCounter = 0
  kernelToRendererMessageCounter = 0
  rendererToKernelMessageCounter = 0
  kernelToRendererMessageNativeCounter = 0

  return ret
}
