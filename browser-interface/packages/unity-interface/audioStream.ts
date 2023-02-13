import { defaultLogger } from 'lib/logger'

/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////

const audioStreamSource = new Audio()

export async function setAudioStream(url: string, play: boolean, volume: number) {
  const isSameSrc =
    audioStreamSource.src.length > 1 && (encodeURI(url) === audioStreamSource.src || url === audioStreamSource.src)
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
