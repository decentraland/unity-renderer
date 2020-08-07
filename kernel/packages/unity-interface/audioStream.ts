import { defaultLogger } from 'shared/logger'
import { teleportObservable } from 'shared/world/positionThings'

/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////

const audioStreamSource = new Audio()
teleportObservable.add(() => {
  audioStreamSource.pause()
})

export async function setAudioStream(url: string, play: boolean, volume: number) {
  const isSameSrc = audioStreamSource.src.length > 1 && url.includes(audioStreamSource.src)
  const playSrc = play && (!isSameSrc || (isSameSrc && audioStreamSource.paused))

  audioStreamSource.volume = volume

  if (play && !isSameSrc) {
    audioStreamSource.src = url
  } else if (!play && isSameSrc) {
    audioStreamSource.pause()
  }

  if (playSrc) {
    try {
      await audioStreamSource.play()
    } catch (err) {
      defaultLogger.log('setAudioStream: failed to play' + err)
    }
  }
}
