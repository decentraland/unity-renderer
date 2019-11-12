import { Entity, Transform, engine, BoxShape, Vector3, Quaternion } from 'decentraland-ecs/src'

const ent = new Entity()
ent.addComponentOrReplace(new BoxShape())
const initialRotation = Quaternion.Euler(0, 0, 120)
const t = new Transform({
  position: new Vector3(5, 0, 5),
  rotation: initialRotation
})

ent.addComponentOrReplace(t)

class MySystem {
  private counter: number = 0
  update(dt: number) {
    if (this.counter < 360) {
      const rot = t.rotation.eulerAngles
      const e = new Vector3(rot.x + 1, 0, 0)
      t.rotation.eulerAngles = e
      this.counter++
    }
  }
}

engine.addEntity(ent)
engine.addSystem(new MySystem())
