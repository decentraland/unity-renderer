import { Vector3, Entity, Transform, BoxShape, engine, SphereShape, Material, Color3 } from 'decentraland-ecs/src'

const materialA = new Material()
const materialB = new Material()
const materialC = new Material()
const materialD = new Material()

materialA.albedoColor = Color3.FromHexString('#4CC3D9')
materialB.albedoColor = Color3.FromHexString('#EF2D5E')
materialC.albedoColor = Color3.FromHexString('#FFC65D')
materialD.albedoColor = Color3.FromHexString('#7BC8A4')

const box = new BoxShape()
const sphere = new SphereShape()

function makeBox(parent: Entity, x: number, y: number, z: number, color?: Material) {
  const ent = new Entity(parent)
  ent.set(
    new Transform({
      position: new Vector3(x, y, z)
    })
  )
  color && ent.set(color)
  ent.set(box)
  engine.addEntity(ent)
  return ent
}

function makeSphere(parent: Entity, x: number, y: number, z: number, color?: Material) {
  const ent = new Entity(parent)
  ent.set(
    new Transform({
      position: new Vector3(x, y, z)
    })
  )
  ent.set(sphere)
  color && ent.set(color)
  engine.addEntity(ent)
  return ent
}

const root = new Entity()
engine.addEntity(root)
root.set(
  new Transform({
    position: new Vector3(0, 0, 10),
    scale: new Vector3(1, 1, -1)
  })
)

makeBox(root, 0.5, 0.5, 0.5, materialA)
makeBox(root, 9.5, 0.5, 9.5, materialB)
makeBox(root, 9.5, 0.5, 0.5, materialC)
makeBox(root, 0.5, 0.5, 9.5, materialD)
makeBox(root, 4, 0.5, 3, materialA).get(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)

// <sphere position="3 1.25 5" radius="1.25" color="#EF2D5E"></sphere>
makeSphere(root, 3, 1.25, 5, materialB)
  .get(Transform)
  .scale.set(1.25, 1.25, 1.25)

{
  const newRoot = new Entity(root)
  engine.addEntity(newRoot)
  newRoot.getOrCreate(Transform).position.set(0, 1, 10)
  makeBox(newRoot, 0.5, 0.5, 0.5, materialA)
  makeBox(newRoot, 9.5, 0.5, 9.5, materialB)
  makeBox(newRoot, 9.5, 0.5, 0.5, materialC)
  makeBox(newRoot, 0.5, 0.5, 9.5, materialD)
  makeBox(newRoot, 4, 0.5, 3, materialA).get(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)
}

{
  const newRoot = new Entity(root)
  engine.addEntity(newRoot)
  newRoot.getOrCreate(Transform).position.set(-10, 2, 10)

  makeBox(newRoot, 0.5, 0.5, 0.5, materialA)
  makeBox(newRoot, 9.5, 0.5, 9.5, materialB)
  makeBox(newRoot, 9.5, 0.5, 0.5, materialC)
  makeBox(newRoot, 0.5, 0.5, 9.5, materialD)
  makeBox(newRoot, 4, 0.5, 3, materialA).get(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)
}
