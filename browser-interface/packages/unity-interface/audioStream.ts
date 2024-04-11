/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////
import { Entity } from '@dcl/ecs/dist-cjs'
import { MediaState } from '@dcl/ecs/dist-cjs/components'

import { audioStreamEmitter } from '../shared/world/runtime-7/engine'

type AudioEvents = HTMLMediaElementEventMap
type GlobalProps = {
  playToken: number
  entityId?: Entity
  sceneId?: string | number
}

let globalProps: GlobalProps = {
  playToken: 0,
  sceneId: undefined,
  entityId: undefined
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
  const { entityId, sceneId } = globalProps

  if (!entityId || !sceneId) return

  const state = getAudioEvent(ev.type)

  audioStreamEmitter.emit('changeState', { entityId, state, sceneId })
}

function getAudioEvent(type: string): MediaState {
  switch (type) {
    case 'loadeddata':
      return MediaState.MS_READY
    case 'error':
      return MediaState.MS_ERROR
    case 'seeking':
      return MediaState.MS_SEEKING
    case 'loadstart':
      return MediaState.MS_LOADING
    case 'waiting':
      return MediaState.MS_BUFFERING
    case 'playing':
      return MediaState.MS_PLAYING
    case 'pause':
      return MediaState.MS_PAUSED
    default:
      return MediaState.MS_NONE
  }
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
  setGlobalProps({ ...globalProps, sceneId, entityId })
  void setAudioStream(url, play, volume)
}

export async function killAudioStream(sceneId: number, entityId: Entity) {
  setGlobalProps({ ...globalProps, sceneId: undefined, entityId: undefined })
  void setAudioStream(audioStreamSource.src, false, audioStreamSource.volume)
  audioStreamEmitter.emit('changeState', { entityId, state: MediaState.MS_NONE, sceneId })
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
