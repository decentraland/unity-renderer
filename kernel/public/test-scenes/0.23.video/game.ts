import {
  Entity,
  engine,
  Transform,
  Vector3,
  OnClick,
  PlaneShape,
  Quaternion,
  VideoTexture,
  VideoClip,
  BasicMaterial
} from 'decentraland-ecs/src'

const e = new Entity()
e.addComponent(new PlaneShape())
e.addComponent(
  new Transform({
    position: new Vector3(8, 4.5, 15),
    scale: new Vector3(16, 9, 1),
    rotation: Quaternion.Euler(0, 180, 0)
  })
)

const v = new VideoTexture(new VideoClip('http://rt-arab.secure.footprint.net:80/1104.m3u8?streamType=live'))

const mat = new BasicMaterial()
mat.texture = v

e.addComponent(mat)
e.addComponent(
  new OnClick(() => {
    v.playing = !v.playing
  })
)

engine.addEntity(e)
