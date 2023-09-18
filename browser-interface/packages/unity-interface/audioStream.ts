/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////

const audioStreamSource = new Audio()
let playToken: number = 0;

export async function setAudioStream(url: string, play: boolean, volume: number) {
  const isSameSrc =
    audioStreamSource.src.length > 1 && (encodeURI(url) === audioStreamSource.src || url === audioStreamSource.src)
  const playSrc = play && (!isSameSrc || (isSameSrc && audioStreamSource.paused))

  audioStreamSource.volume = volume

  if (play && !isSameSrc) {
    audioStreamSource.src = url
  } else if (!play && isSameSrc) {
    playToken++
    audioStreamSource.pause()
  }

  if (playSrc) {
    playIntent()
  }
}

// audioStreamSource play might be requested without user interaction
// i.e: spawning in world without clicking on the canvas
// so me want to keep retrying on play exception until audio starts playing
function playIntent() {
  function tryPlay(token: number) {
    if (playToken !== token)
      return

    audioStreamSource.play()
      .catch(_ => {
        setTimeout(() => tryPlay(token), 500)
      })
  }
  playToken++
  tryPlay(playToken)
}
