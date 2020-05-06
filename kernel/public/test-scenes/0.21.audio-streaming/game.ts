import { Entity, engine, Transform, Vector3, OnClick, BoxShape, AudioStream } from 'decentraland-ecs/src'

const cube = new Entity()
cube.addComponent(new BoxShape())
cube.addComponent(new Transform({ position: new Vector3(8, 1, 8) }))

const audioStream = new AudioStream('https://radiotopfm2.turadioonline.xyz/stream')
audioStream.playing = false
cube.addComponent(audioStream)

cube.addComponent(
  new OnClick(() => {
    audioStream.playing = !audioStream.playing
  })
)

engine.addEntity(cube)
