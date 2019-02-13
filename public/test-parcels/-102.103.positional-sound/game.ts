import {
  Component,
  AudioSource,
  AudioClip,
  Entity,
  engine,
  Transform,
  Vector3,
  GLTFShape,
  log,
  OnPointerDown,
  Input,
  PlaneShape,
  Billboard,
  Material,
  Color3,
  Gizmos
} from '../../../packages/decentraland-ecs/src'

const g = new Gizmos()

@Component('alternatingNotes')
export class AlternatingNotes {
  notes: AudioSource[]
  nextNote: number
  constructor(notes: AudioSource[]) {
    this.notes = notes
    this.nextNote = 0
  }
}

// Create Audio objects
let AHighClip = new AudioClip('sounds/kalimbaAHigh.wav')
let sourceAHigh = new AudioSource(AHighClip)
sourceAHigh.loop = false
let AHigh = new Entity()
AHigh.add(sourceAHigh)
engine.addEntity(AHigh)

let ALowClip = new AudioClip('sounds/kalimbaALow.wav')
let sourceALow = new AudioSource(ALowClip)
sourceALow.loop = false
let ALow = new Entity()
ALow.add(sourceALow)
engine.addEntity(ALow)

let CHighClip = new AudioClip('sounds/kalimbaCHigh.wav')
let sourceCHigh = new AudioSource(CHighClip)
sourceCHigh.loop = false
let CHigh = new Entity()
CHigh.add(sourceCHigh)
engine.addEntity(CHigh)

let CLowClip = new AudioClip('sounds/kalimbaCLow.wav')
let sourceCLow = new AudioSource(CLowClip)
sourceCLow.loop = false
let CLow = new Entity()
CLow.add(sourceCLow)
engine.addEntity(CLow)

let EHighClip = new AudioClip('sounds/kalimbaEHigh.wav')
let sourceEHigh = new AudioSource(EHighClip)
sourceEHigh.loop = false
let EHigh = new Entity()
EHigh.add(sourceEHigh)
engine.addEntity(EHigh)

let GLowClip = new AudioClip('sounds/kalimbaGLow.wav')
let sourceGLow = new AudioSource(GLowClip)
sourceGLow.loop = false
let GLow = new Entity()
GLow.add(sourceGLow)
engine.addEntity(GLow)

// fruits

const shape = new GLTFShape('models/2.glb')

let fruit1 = new Entity()
fruit1.add(
  new Transform({
    position: new Vector3(1.3 + 5, 2.3, 0.2 + 5)
  })
)
fruit1.add(g)
fruit1.add(new AlternatingNotes([sourceAHigh, sourceALow]))
fruit1.add(shape)
fruit1.add(
  new OnPointerDown(e => {
    log('fuit1 ', e)
    playNote(fruit1)
  })
)
engine.addEntity(fruit1)
AHigh.setParent(fruit1)
ALow.setParent(fruit1)

let fruit2 = new Entity()
fruit2.add(
  new Transform({
    position: new Vector3(-1.2 + 5, 2, -0.2 + 5)
  })
)
fruit2.add(g)
fruit2.add(new AlternatingNotes([sourceEHigh, sourceGLow]))
fruit2.add(shape)
fruit2.add(
  new OnPointerDown(e => {
    log('fuit2', e)
    playNote(fruit2)
  })
)
engine.addEntity(fruit2)
EHigh.setParent(fruit2)
GLow.setParent(fruit2)

let fruit3 = new Entity()
fruit3.add(
  new Transform({
    position: new Vector3(0 + 5, 1.7, 2 + 5)
  })
)
fruit3.add(g)
fruit3.add(new AlternatingNotes([sourceCHigh, sourceCLow]))
fruit3.add(shape)
fruit3.add(
  new OnPointerDown(e => {
    log('fuit3', e)
    playNote(fruit3)
  })
)
engine.addEntity(fruit3)
CHigh.setParent(fruit3)
CLow.setParent(fruit3)

function playNote(fruit: Entity) {
  let notes = fruit.get(AlternatingNotes)
  let playedNote = notes.notes[notes.nextNote]
  playedNote.playOnce()
  notes.nextNote += 1
  if (notes.nextNote > notes.notes.length - 1) {
    notes.nextNote = 0
  }
}

Input.instance.subscribe('BUTTON_A_DOWN', evt => {
  log(evt)
  if (evt.hit) {
    spawnClick(evt.hit.hitPoint)
    spawnClick2(evt.origin.add(evt.direction.scale(evt.hit.length)))
  }
})

const clickShape = new PlaneShape()
const billboard = new Billboard()
const material = new Material()
material.albedoColor = Color3.Red()
const material2 = new Material()
material2.albedoColor = Color3.Green()

function spawnClick(position: Vector3) {
  const ent = new Entity()
  ent.set(new Transform({ position, scale: new Vector3(0.03, 0.03, 0.03) }))
  ent.set(clickShape)
  ent.set(billboard)
  ent.set(material)
  engine.addEntity(ent)
  return ent
}

function spawnClick2(position: Vector3) {
  const ent = new Entity()
  ent.set(new Transform({ position, scale: new Vector3(0.03, 0.03, 0.03) }))
  ent.set(clickShape)
  ent.set(billboard)
  ent.set(material2)
  engine.addEntity(ent)
  return ent
}
