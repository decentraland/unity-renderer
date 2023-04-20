import Html from './Html'
import { isChrome } from 'lib/browser/isChrome'

export type GlobalAudioStream = {
  setGainVolume(volume: number): void
  getDestinationStream(): MediaStream
  getAudioContext(): AudioContext
  getGainNode(): GainNode
  play(): void
}

let globalAudioStream: undefined | GlobalAudioStream = undefined

export async function getGlobalAudioStream() {
  if (!globalAudioStream) {
    globalAudioStream = await createAudioStream()
  }

  return globalAudioStream
}

export async function createAudioStream(): Promise<GlobalAudioStream> {
  const parentElement = Html.loopbackAudioElement()

  const audioContext = new AudioContext()
  const destination = audioContext.createMediaStreamDestination()
  const destinationStream = isChrome() ? await startLoopback(destination.stream) : destination.stream

  const gainNode = audioContext.createGain()
  gainNode.connect(destination)
  gainNode.gain.value = 1

  if (!parentElement) {
    throw new Error('Cannot create global audio stream: no parent element')
  }

  parentElement.srcObject = destinationStream

  function getGainNode() {
    return gainNode
  }

  function setGainVolume(volume: number) {
    gainNode.gain.value = volume
  }

  function getDestinationStream() {
    return destinationStream
  }

  function getAudioContext() {
    return audioContext
  }

  function play() {
    parentElement!.play().catch(console.error)
  }

  return {
    getGainNode,
    setGainVolume,
    getDestinationStream,
    getAudioContext,
    play
  }
}

const offerOptions = {
  offerVideo: false,
  offerAudio: true,
  offerToReceiveAudio: false,
  offerToReceiveVideo: false
}

export async function startLoopback(stream: MediaStream) {
  const loopbackStream = new MediaStream()
  const rtcConnection = new RTCPeerConnection()
  const rtcLoopbackConnection = new RTCPeerConnection()

  rtcConnection.onicecandidate = (e) =>
    e.candidate && rtcLoopbackConnection.addIceCandidate(new RTCIceCandidate(e.candidate))

  rtcLoopbackConnection.onicecandidate = (e) =>
    e.candidate && rtcConnection.addIceCandidate(new RTCIceCandidate(e.candidate))

  rtcLoopbackConnection.ontrack = (e) => e.streams[0].getTracks().forEach((track) => loopbackStream.addTrack(track))

  // setup the loopback
  stream.getTracks().forEach((track) => rtcConnection.addTrack(track, stream))

  const offer = await rtcConnection.createOffer(offerOptions)
  await rtcConnection.setLocalDescription(offer)

  await rtcLoopbackConnection.setRemoteDescription(offer)
  const answer = await rtcLoopbackConnection.createAnswer()
  await rtcLoopbackConnection.setLocalDescription(answer)

  await rtcConnection.setRemoteDescription(answer)

  return loopbackStream
}
