import {
  Entity,
  engine,
  Transform,
  Vector3,
  PlaneShape,
  Quaternion,
  VideoTexture,
  VideoClip,
  BasicMaterial,
  OnPointerDown,
  BoxShape,
  ActionButton
} from 'decentraland-ecs/src'

/*
Plane with video live streaming

Plane with video playing from local file.
  Cube to set the video to X second
  Cube to set video to loop
  Cube toggle video play rate between x1 x4
  Cube to restart video
*/

const streamEntity = new Entity()
streamEntity.addComponent(new PlaneShape())
streamEntity.addComponent(
  new Transform({
    position: new Vector3(8, 4.5, 15),
    scale: new Vector3(16, 9, 1),
    rotation: Quaternion.Euler(0, 180, 0)
  })
)

const streamVideo = new VideoTexture(new VideoClip('http://rt-arab.secure.footprint.net:80/1104.m3u8?streamType=live'))

const streamMaterial = new BasicMaterial()
streamMaterial.texture = streamVideo

streamEntity.addComponent(streamMaterial)
streamEntity.addComponent(
  new OnPointerDown(
    () => {
      streamVideo.playing = !streamVideo.playing
    },
    { button: ActionButton.POINTER, hoverText: 'Play Stream' }
  )
)

engine.addEntity(streamEntity)

const localEntity = new Entity()
localEntity.addComponent(new PlaneShape())
localEntity.addComponent(
  new Transform({
    position: new Vector3(15, 4.5, 8),
    scale: new Vector3(16, 9, 1),
    rotation: Quaternion.Euler(0, 270, 0)
  })
)

const localVideo = new VideoTexture(new VideoClip('video1.mp4'))

const localMaterial = new BasicMaterial()
localMaterial.texture = localVideo

localEntity.addComponent(localMaterial)

localEntity.addComponent(
  new OnPointerDown(
    () => {
      localVideo.playing = !localVideo.playing
    },
    { button: ActionButton.POINTER, hoverText: 'Play/Pause Local Video' }
  )
)

const seekEntity = new Entity()
seekEntity.addComponent(new BoxShape())
seekEntity.addComponent(new Transform({ position: new Vector3(14.5, 1, 10) }))
seekEntity.addComponent(
  new OnPointerDown(
    () => {
      localVideo.seekTime(15)
    },
    { button: ActionButton.POINTER, hoverText: 'Seek second: 15' }
  )
)

const loopEntity = new Entity()
loopEntity.addComponent(new BoxShape())
loopEntity.addComponent(new Transform({ position: new Vector3(14.5, 1, 8) }))
const loopPointerEvent = new OnPointerDown(
  () => {
    localVideo.loop = !localVideo.loop
    loopPointerEvent.hoverText = 'Loop: ' + String(localVideo.loop)
  },
  { button: ActionButton.POINTER, hoverText: 'Loop: false' }
)
loopEntity.addComponent(loopPointerEvent)

const playbackRateEntity = new Entity()
playbackRateEntity.addComponent(new BoxShape())
playbackRateEntity.addComponent(new Transform({ position: new Vector3(14.5, 1, 6) }))
const playbackPointerEvent = new OnPointerDown(
  () => {
    localVideo.playbackRate = localVideo.playbackRate == 1 ? 4 : 1
    playbackPointerEvent.hoverText = 'Playback Rate: ' + localVideo.playbackRate
  },
  { button: ActionButton.POINTER, hoverText: 'Playback Rate: 1' }
)
playbackRateEntity.addComponent(playbackPointerEvent)

const resetEntity = new Entity()
resetEntity.addComponent(new BoxShape())
resetEntity.addComponent(new Transform({ position: new Vector3(14.5, 1, 4) }))
resetEntity.addComponent(
  new OnPointerDown(
    () => {
      localVideo.reset()
    },
    { button: ActionButton.POINTER, hoverText: 'Restart video' }
  )
)

engine.addEntity(localEntity)
engine.addEntity(seekEntity)
engine.addEntity(loopEntity)
engine.addEntity(playbackRateEntity)
engine.addEntity(resetEntity)
