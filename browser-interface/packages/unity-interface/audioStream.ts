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

export async function setAudioStreamForEntity(url: string, play: boolean, volume: number, sceneNumber: number, entityId: number) {
  setAudioStream(url, play, volume)

  // TODO: "getSceneWorkerBySceneNumber().rpcContext.internalEngine" to update the AudioEvent component with append
}

export async function killAudioStream(sceneNumber: number, entityId: number) {
  // TODO: remove entity from internalEngine...
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
