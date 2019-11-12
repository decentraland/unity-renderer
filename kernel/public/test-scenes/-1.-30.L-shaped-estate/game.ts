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
  const ent = new Entity()
  ent.addComponentOrReplace(
    new Transform({
      position: new Vector3(x, y, z)
    })
  )
  color && ent.addComponentOrReplace(color)
  ent.addComponentOrReplace(box)
  engine.addEntity(ent)
  ent.setParent(parent)
  return ent
}

function makeSphere(parent: Entity, x: number, y: number, z: number, color?: Material) {
  const ent = new Entity()
  ent.addComponentOrReplace(
    new Transform({
      position: new Vector3(x, y, z)
    })
  )
  ent.addComponentOrReplace(sphere)
  color && ent.addComponentOrReplace(color)
  engine.addEntity(ent)
  ent.setParent(parent)
  return ent
}

const root = new Entity()
engine.addEntity(root)
root.addComponentOrReplace(
  new Transform({
    position: new Vector3(0, 0, 10),
    scale: new Vector3(1, 1, -1)
  })
)

makeBox(root, 0.5, 0.5, 0.5, materialA)
makeBox(root, 9.5, 0.5, 9.5, materialB)
makeBox(root, 9.5, 0.5, 0.5, materialC)
makeBox(root, 0.5, 0.5, 9.5, materialD)
makeBox(root, 4, 0.5, 3, materialA).getComponent(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)

// <sphere position="3 1.25 5" radius="1.25" color="#EF2D5E"></sphere>
makeSphere(root, 3, 1.25, 5, materialB)
  .getComponent(Transform)
  .scale.set(1.25, 1.25, 1.25)

{
  const newRoot = new Entity()
  engine.addEntity(newRoot)
  newRoot.setParent(root)
  newRoot.getComponentOrCreate(Transform).position.set(0, 1, 10)
  makeBox(newRoot, 0.5, 0.5, 0.5, materialA)
  makeBox(newRoot, 9.5, 0.5, 9.5, materialB)
  makeBox(newRoot, 9.5, 0.5, 0.5, materialC)
  makeBox(newRoot, 0.5, 0.5, 9.5, materialD)
  makeBox(newRoot, 4, 0.5, 3, materialA).getComponent(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)
}

{
  const newRoot = new Entity()
  engine.addEntity(newRoot)
  newRoot.setParent(root)
  newRoot.getComponentOrCreate(Transform).position.set(-10, 2, 10)

  makeBox(newRoot, 0.5, 0.5, 0.5, materialA)
  makeBox(newRoot, 9.5, 0.5, 9.5, materialB)
  makeBox(newRoot, 9.5, 0.5, 0.5, materialC)
  makeBox(newRoot, 0.5, 0.5, 9.5, materialD)
  makeBox(newRoot, 4, 0.5, 3, materialA).getComponent(Transform).rotation.eulerAngles = new Vector3(0, 45, 0)
}
