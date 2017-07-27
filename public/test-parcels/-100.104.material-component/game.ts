import { engine, Material, BoxShape, Entity, Transform, Color3 } from 'decentraland-ecs/src'

const box = new BoxShape()
const niceMaterial = new Material()
niceMaterial.albedoColor = Color3.FromHexString('#FF0000')
niceMaterial.metallic = 0.9
niceMaterial.roughness = 0.1

function spawn() {
  const ent = new Entity()
  const transform = new Transform()
  transform.position.x = Math.random() * 8
  transform.position.y = Math.random() * 8
  transform.position.z = Math.random() * 8

  ent.set(box)
  ent.set(niceMaterial)
  ent.set(transform)
  engine.addEntity(ent)
}

for (let i = 0; i < 10; i++) {
  spawn()
}
