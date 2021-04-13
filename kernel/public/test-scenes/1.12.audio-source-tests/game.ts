/* Test description:
 * One box to play shots when we click it and another box to toggle music that changes the volume constantly
 */

import { engine, Entity, Transform, Vector3, AudioSource, AudioClip, BoxShape, OnClick, ISystem } from 'decentraland-ecs/src'

// Sounds
const shotClip = new AudioClip('shot.ogg')
const shotSource = new AudioSource(shotClip)

const musicClip = new AudioClip('carnivalrides.ogg')
const musicSource = new AudioSource(musicClip)

// Trigger playOnce
const box1 = new Entity()

box1.addComponent(new BoxShape())
box1.addComponent(new Transform({
  position: new Vector3(2, 2, 8),
  scale: new Vector3(1, 1, 1),
}))

box1.addComponentOrReplace(shotSource)

box1.addComponent(
  new OnClick((): void => {
    shotSource.playOnce()
  }, {
    hoverText: "Shot"
  })
)

engine.addEntity(box1);

// Toggle music
const box2 = new Entity()

box2.addComponent(new BoxShape())
box2.addComponent(new Transform({
  position: new Vector3(8, 2, 8),
  scale: new Vector3(1, 1, 1),
}))

box2.addComponentOrReplace(musicSource)

box2.addComponent(
  new OnClick((): void => {
    if(musicSource.playing) {
      musicSource.playing = false
    } else {
      musicSource.playOnce()
    }
  }, {
    hoverText: "Toggle music"
  })
)

engine.addEntity(box2);

// Modify volume and loop
export class AudioRandomVolume implements ISystem {
  value : number = 0.0
  update() {
    this.value += 0.0872665 // 5 degrees in radians
    musicSource.volume = Math.sin(this.value)
  }
}

// Add system to engine
engine.addSystem(new AudioRandomVolume())