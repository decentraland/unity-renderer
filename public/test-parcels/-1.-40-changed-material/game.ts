import { Transform, Vector3, engine, Material, Entity, SphereShape, Color3 } from 'decentraland-ecs/src'

const mat0 = new Material()
mat0.albedoColor = Color3.FromHexString('#0000ff')

const mat1 = new Material()
mat1.albedoColor = Color3.FromHexString('#cccccc')

const ent = new Entity()
ent.addComponentOrReplace(new SphereShape())
ent.addComponentOrReplace(
  new Transform({
    position: new Vector3(2, 1, 2)
  })
)

class TestSystem {
  currentMaterial = mat0

  update() {
    if (this.currentMaterial === mat0) {
      this.currentMaterial = mat1
    }

    ent.addComponentOrReplace(this.currentMaterial)
  }
}

engine.addEntity(ent)

engine.addSystem(new TestSystem())
