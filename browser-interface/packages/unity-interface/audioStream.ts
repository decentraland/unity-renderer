/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////
import { Entity, IEngine } from '@dcl/ecs/dist-cjs'
import { AudioEvent as defineAudioEvent, AudioState } from '@dcl/ecs/dist-cjs/components'
import { SceneWorker } from '../shared/world/SceneWorker'
import { getSceneWorkerBySceneID, getSceneWorkerBySceneNumber } from '../shared/world/parcelSceneManager'

type AudioEvents = HTMLMediaElementEventMap
type GlobalProps = {
  playToken: number
  entityId: Entity | undefined
  engine: IEngine | undefined
}

let globalProps: GlobalProps = {
  playToken: 0,
  entityId: undefined,
  engine: undefined
}

function setGlobalProps(props: GlobalProps) {
  globalProps = props
}

// https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement#events
const AUDIO_EVENTS: (keyof AudioEvents)[] = [
  'loadeddata',
  'error',
  'seeking',
  'loadstart',
  'waiting',
  'playing',
  'pause'
]

const audioStreamSource = new Audio()
AUDIO_EVENTS.forEach(($) => audioStreamSource.addEventListener($, listen))

function listen(ev: AudioEvents[keyof AudioEvents]) {
  const { entityId, engine } = globalProps

  if (!entityId || !engine) return

  const AudioEvent = defineAudioEvent(engine)
  const state = getAudioEvent(ev.type)
  const timestamp = Date.now()

  AudioEvent.addValue(entityId, { state, timestamp })
}

function getAudioEvent(type: string): AudioState {
  switch (type) {
    case 'loadeddata':
      return AudioState.AS_READY
    case 'error':
      return AudioState.AS_ERROR
    case 'seeking':
      return AudioState.AS_SEEKING
    case 'loadstart':
      return AudioState.AS_LOADING
    case 'waiting':
      return AudioState.AS_BUFFERING
    case 'playing':
      return AudioState.AS_PLAYING
    case 'pause':
      return AudioState.AS_PAUSED
    default:
      return AudioState.AS_NONE
  }
}

function getScene(id: string | number): SceneWorker | undefined {
  if (typeof id === 'string') return getSceneWorkerBySceneID(id)
  if (typeof id === 'number') return getSceneWorkerBySceneNumber(id)
  return undefined
}

function getEngineFromScene(scene?: SceneWorker): IEngine | undefined {
  return scene?.rpcContext.internalEngine?.engine
}

export async function setAudioStream(url: string, play: boolean, volume: number) {
  const isSameSrc =
    audioStreamSource.src.length > 1 && (encodeURI(url) === audioStreamSource.src || url === audioStreamSource.src)
  const playSrc = play && (!isSameSrc || (isSameSrc && audioStreamSource.paused))

  audioStreamSource.volume = volume

  if (play && !isSameSrc) {
    audioStreamSource.src = url
  } else if (!play && isSameSrc) {
    globalProps.playToken++
    audioStreamSource.pause()
  }

  if (playSrc) {
    playIntent()
  }
}

export async function setAudioStreamForEntity(
  url: string,
  play: boolean,
  volume: number,
  sceneId: string | number,
  entityId: Entity
) {
  void setAudioStream(url, play, volume)

  const scene = getScene(sceneId)
  const engine = getEngineFromScene(scene)

  setGlobalProps({ ...globalProps, engine, entityId })
}

export async function killAudioStream(sceneId: number, entityId: Entity) {
  void setAudioStream(audioStreamSource.src, false, audioStreamSource.volume)

  const scene = getScene(sceneId)
  const engine = getEngineFromScene(scene)

  if (engine) {
    const AudioEvent = defineAudioEvent(engine)
    AudioEvent.addValue(entityId, { state: AudioState.AS_NONE, timestamp: Date.now() })
  }

  setGlobalProps({ ...globalProps, engine: undefined, entityId: undefined })
}

// audioStreamSource play might be requested without user interaction
// i.e: spawning in world without clicking on the canvas
// so me want to keep retrying on play exception until audio starts playing
function playIntent() {
  function tryPlay(token: number) {
    if (globalProps.playToken !== token) return

    audioStreamSource.play().catch((_) => {
      setTimeout(() => tryPlay(token), 500)
    })
  }
  globalProps.playToken++
  tryPlay(globalProps.playToken)
}
