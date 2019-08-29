import { engine, Entity, Transform, Vector3, Color3, BoxShape, Material, Quaternion } from 'decentraland-ecs'

const box = new BoxShape()
const redMaterial = new Material()
redMaterial.albedoColor = Color3.FromHexString('#FF0000')
const greenMaterial = new Material()
greenMaterial.albedoColor = Color3.FromHexString('#00FF00')
const blueMaterial = new Material()
blueMaterial.albedoColor = Color3.FromHexString('#0000FF')

function spawnAxis() {
  const axis = new Entity()
  engine.addEntity(axis)

  const xAxis = new Entity()
  const yAxis = new Entity()
  const zAxis = new Entity()

  xAxis.addComponentOrReplace(box)
  xAxis.addComponentOrReplace(redMaterial)
  xAxis.addComponentOrReplace(
    new Transform({
      position: new Vector3(0.5, 0, 0),
      scale: new Vector3(1, 0.02, 0.02)
    })
  )

  yAxis.addComponentOrReplace(box)
  yAxis.addComponentOrReplace(greenMaterial)
  yAxis.addComponentOrReplace(
    new Transform({
      position: new Vector3(0, 0.5, 0),
      scale: new Vector3(0.02, 1, 0.02)
    })
  )

  zAxis.addComponentOrReplace(box)
  zAxis.addComponentOrReplace(blueMaterial)
  zAxis.addComponentOrReplace(
    new Transform({
      position: new Vector3(0, 0, 0.5),
      scale: new Vector3(0.02, 0.02, 1)
    })
  )

  engine.addEntity(xAxis)
  engine.addEntity(yAxis)
  engine.addEntity(zAxis)

  xAxis.setParent(axis)
  yAxis.setParent(axis)
  zAxis.setParent(axis)

  return axis
}

function spawnCube(position: Vector3, rotation: Vector3) {
  const ent = new Entity()
  const box = new BoxShape()
  const rot = Quaternion.Euler(rotation.x, rotation.y, rotation.z)
  const axis = spawnAxis()
  ent.addComponentOrReplace(
    new Transform({
      position,
      rotation: rot
    })
  )
  ent.addComponentOrReplace(box)
  engine.addEntity(ent)
  axis.setParent(ent)
}

spawnCube(new Vector3(0, 5, 0), new Vector3(50, 0, 0))
spawnCube(new Vector3(1.5, 5, 0), new Vector3(0, 50, 0))
spawnCube(new Vector3(3, 5, 0), new Vector3(0, 0, 50))
