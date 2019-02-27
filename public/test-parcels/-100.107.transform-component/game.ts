import { engine, Transform, Entity, BoxShape, Vector3, Quaternion, ISystem } from 'decentraland-ecs/src'

const ent = new Entity()
ent.addComponentOrReplace(new BoxShape())
ent.addComponentOrReplace(
  new Transform({
    position: new Vector3(5, 3, 1),
    rotation: Quaternion.Euler(10, 0, 0),
    scale: new Vector3(2, 2, 2)
  })
)

engine.addEntity(ent)

class RotatorSystem implements ISystem {
  update(dt: number) {
    const t = ent.getComponent(Transform)
    t.rotate(Vector3.Right(), dt * 10)
    t.rotate(Vector3.Up(), dt * 10)
  }
}

class MoverSystem implements ISystem {
  forward = Vector3.Forward()
  backward = Vector3.Backward()
  direction = this.forward

  update(dt: number) {
    const t = ent.getComponent(Transform)
    t.translate(this.direction.scale(dt * 3))
    if (t.position.z >= 8) {
      this.direction = this.backward
    } else if (t.position.z <= 2) {
      this.direction = this.forward
    }
  }
}

engine.addSystem(new RotatorSystem())
engine.addSystem(new MoverSystem())
