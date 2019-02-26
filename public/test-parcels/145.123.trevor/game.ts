import { Entity, GLTFShape, engine, Vector3, Transform, AnimationClip, Animator, 
  Component, ISystem, Quaternion, AudioClip, AudioSource, Material, PlaneShape, Color3 } from 'decentraland-ecs/src'

const colors = ["#1dccc7", "#ffce00", "#9076ff", "#fe3e3e", "#3efe94", "#3d30ec", "#6699cc"]

///////////////////////////////
// Custom components

@Component('tileFlag')
export class TileFlag {
}

@Component('beat')
export class Beat {
  interval: number
  timer: number
  constructor(interval: number = 0.5){
    this.interval = interval
    this.timer = interval
  }
}


///////////////////////////
// Entity groups

const tiles = engine.getComponentGroup(TileFlag)

///////////////////////////
// Systems

export class changeColor implements ISystem {
  update(dt: number) {
    let beat = beatKeeper.get(Beat)
    beat.timer -= dt
    if (beat.timer < 0){
      beat.timer = beat.interval
      for (let tile of tiles.entities) {
        const colorNum = Math.floor(Math.random() * colors.length)
        tile.add(tileMaterials[colorNum])
      }
    }
  }
}


engine.addSystem(new changeColor)

///////////////////////////
// INITIAL ENTITIES


// Create materials
let tileMaterials : Material[] = []
for (let i = 0; i < colors.length; i ++){
  let material = new Material()
  material.albedoColor = Color3.FromHexString(colors[i])
  tileMaterials.push(material)
}

// Add Tiles
[0, 1, 2, 3, 4].forEach(x => {
  [0, 1, 2, 3, 4].forEach(z => {
    const tile = new Entity()
    tile.add(new PlaneShape())
    tile.add(new Transform({
      position: new Vector3((x * 2) + 1, 0, (z * 2) + 1),
      rotation: Quaternion.Euler(90, 0, 0),
      scale: new Vector3(2, 2, 2)
    }))
    tile.add(new TileFlag())
    const colorNum = Math.floor(Math.random() * colors.length)
    tile.add(tileMaterials[colorNum])
    engine.addEntity(tile)
  })
})


// Add dancing Trevor
const trevor = new Entity()
trevor.add(new GLTFShape("models/Trevor.glb"))
const clipDance = new AnimationClip("Armature_Idle")
const animator = new Animator()
animator.addClip(clipDance)
trevor.add(animator)
clipDance.play()
trevor.add(new Transform({
  position: new Vector3(5, 0.1, 5),
  rotation: Quaternion.Euler(0, -90, 0),
  scale: new Vector3(1.5, 1.5, 1.5)
}))

const audioClip = new AudioClip("sounds/Vexento.ogg")
audioClip.loop = true
const audioSource = new AudioSource(audioClip)
trevor.add(audioSource)

engine.addEntity(trevor)

// Singleton to keep track of the beat
let beatKeeper = new Entity()
beatKeeper.add(new Beat(0.5))

audioSource.playing = true

