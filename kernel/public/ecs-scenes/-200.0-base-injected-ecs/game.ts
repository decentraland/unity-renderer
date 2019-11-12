const material = new Material()

material.roughness = 0.5
material.metallic = 0.5

const group = engine.getComponentGroup(Transform)

export class RotatorSystem {
  update(dt: number) {
    for (let entity of group.entities) {
      const t = entity.getComponent(Transform)
      t.rotate(Vector3.Up(), dt * 10)
    }
  }
}

export class TraslatorSystem {
  group = engine.getComponentGroup(Transform, Counter)

  update(dt: number) {
    for (let entity of this.group.entities) {
      const t = entity.getComponent(Transform)
      const counter = entity.getComponent(Counter)
      t.position.x = 5 + 3 * Math.sin(counter.number)
      t.position.z = 3 + 3 * Math.cos(counter.number)
      counter.number += dt / 10
    }
  }
}

@Component('a counter')
export class Counter {
  number = 0
}

const cube = new Entity()

cube.getComponentOrCreate(Transform)
cube.getComponentOrCreate(BoxShape)
cube.getComponentOrCreate(Counter)
cube.addComponent(material)

engine.addEntity(cube)

engine.addSystem(new RotatorSystem())
engine.addSystem(new TraslatorSystem())

log('-200,0 loaded')

executeTask(async () => {
  throw new Error('this is an error triggered from executeTask')
})
