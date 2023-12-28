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

function loopbackAudioElement() {
  const audio = document.getElementById('voice-chat-audio')
  if (!audio) {
    console.error('There is no #voice-chat-audio element to use with VoiceChat. Returning a `new Audio()`')
    const audio = new Audio()
    document.body.append(audio)
    return audio
  }
  return audio as HTMLAudioElement | undefined
}

export async function createAudioStream(): Promise<GlobalAudioStream> {
  const parentElement = loopbackAudioElement()
  if (!parentElement) {
    throw new Error('Cannot create global audio stream: no parent element')
  }

  const audioContext = new AudioContext()
  const destination = audioContext.createMediaStreamDestination()

  const gainNode = new GainNode(audioContext)
  gainNode.connect(destination)
  gainNode.gain.value = 1

  parentElement.srcObject = destination.stream

  function getGainNode() {
    return gainNode
  }

  function setGainVolume(volume: number) {
    gainNode.gain.value = volume
  }

  function getDestinationStream() {
    return destination.stream
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
