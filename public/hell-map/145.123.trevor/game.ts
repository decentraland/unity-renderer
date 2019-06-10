import {
  Entity,
  GLTFShape,
  engine,
  Vector3,
  Transform,
  AnimationState,
  Animator,
  Component,
  ISystem,
  Quaternion,
  AudioClip,
  AudioSource,
  Material,
  PlaneShape,
  Color3
} from 'decentraland-ecs/src'

const colors = ['#1dccc7', '#ffce00', '#9076ff', '#fe3e3e', '#3efe94', '#3d30ec', '#6699cc']

///////////////////////////////
// Custom components

@Component('tileFlag')
export class TileFlag { }

@Component('beat')
export class Beat {
  interval: number
  timer: number
  constructor(interval: number = 0.5) {
    this.interval = interval
    this.timer = interval
  }
}

///////////////////////////
// Entity groups

const tiles = engine.getComponentGroup(TileFlag)

///////////////////////////
// Systems

export class ChangeColor implements ISystem {
  update(dt: number) {
    let beat = beatKeeper.getComponent(Beat)
    beat.timer -= dt
    if (beat.timer < 0) {
      beat.timer = beat.interval
      for (let tile of tiles.entities) {
        const colorNum = Math.floor(Math.random() * colors.length)
        tile.addComponent(tileMaterials[colorNum])
      }
    }
  }
}

engine.addSystem(new ChangeColor())

///////////////////////////
// INITIAL ENTITIES

// Create materials
let tileMaterials: Material[] = []
for (let i = 0; i < colors.length; i++) {
  let material = new Material()
  material.albedoColor = Color3.FromHexString(colors[i])
  tileMaterials.push(material)
}

// Add Tiles
// tslint:disable-next-line:semicolon
;[0, 1, 2, 3, 4].forEach(x => {
  // tslint:disable-next-line:semicolon
  ;[0, 1, 2, 3, 4].forEach(z => {
    const tile = new Entity()
    tile.addComponent(new PlaneShape())
    tile.addComponent(
      new Transform({
        position: new Vector3(x * 2 + 1, 0, z * 2 + 1),
        rotation: Quaternion.Euler(90, 0, 0),
        scale: new Vector3(2, 2, 2)
      })
    )
    tile.addComponent(new TileFlag())
    const colorNum = Math.floor(Math.random() * colors.length)
    tile.addComponent(tileMaterials[colorNum])
    engine.addEntity(tile)
  })
})

// Add dancing Trevor
const trevor = new Entity()
trevor.addComponent(new GLTFShape('models/Trevor.glb'))
const clipDance = new AnimationState('Armature_Idle')
const animator = new Animator()
animator.addClip(clipDance)
trevor.addComponent(animator)
clipDance.play()
trevor.addComponent(
  new Transform({
    position: new Vector3(5, 0.1, 5),
    rotation: Quaternion.Euler(0, -90, 0),
    scale: new Vector3(1.5, 1.5, 1.5)
  })
)

const audioClip = new AudioClip('sounds/Vexento.ogg')
audioClip.loop = true
const audioSource = new AudioSource(audioClip)
trevor.addComponent(audioSource)

engine.addEntity(trevor)

// Singleton to keep track of the beat
let beatKeeper = new Entity()
beatKeeper.addComponent(new Beat(0.5))

audioSource.playing = true
