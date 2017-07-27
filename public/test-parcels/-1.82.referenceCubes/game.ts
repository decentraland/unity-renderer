import { engine, Entity, Transform, Vector3, Color3, BoxShape, Material, Quaternion } from 'decentraland-ecs'

const box = new BoxShape()
const redMaterial = new Material()
redMaterial.albedoColor = Color3.FromHexString('#FF0000')
const greenMaterial = new Material()
greenMaterial.albedoColor = Color3.FromHexString('#00FF00')
const blueMaterial = new Material()
blueMaterial.albedoColor = Color3.FromHexString('#0000FF')

function spawnAxis(pos: Vector3, rot: Quaternion) {
  const axis = new Entity()
  const xAxis = new Entity(axis)
  const yAxis = new Entity(axis)
  const zAxis = new Entity(axis)

  xAxis.set(box)
  xAxis.set(redMaterial)
  xAxis.set(
    new Transform({
      position: new Vector3(0.5, 0, 0),
      scale: new Vector3(1, 0.02, 0.02)
    })
  )

  yAxis.set(box)
  yAxis.set(greenMaterial)
  yAxis.set(
    new Transform({
      position: new Vector3(0, 0.5, 0),
      scale: new Vector3(0.02, 1, 0.02)
    })
  )

  zAxis.set(box)
  zAxis.set(blueMaterial)
  zAxis.set(
    new Transform({
      position: new Vector3(0, 0, 0.5),
      scale: new Vector3(0.02, 0.02, 1)
    })
  )

  const t = new Transform({
    position: pos,
    rotation: rot
  })
  axis.set(t)

  engine.addEntity(axis)
  engine.addEntity(xAxis)
  engine.addEntity(yAxis)
  engine.addEntity(zAxis)

  return axis
}

function spawnCube(position: Vector3, rotation: Vector3) {
  const ent = new Entity()
  const box = new BoxShape()
  const rot = Quaternion.Euler(rotation.x, rotation.y, rotation.z)
  const axis = spawnAxis(position, rot)
  ent.set(
    new Transform({
      position,
      rotation: rot
    })
  )
  ent.set(box)
  axis.setParent(ent)
  engine.addEntity(ent)
}

spawnCube(new Vector3(0, 5, 0), new Vector3(50, 0, 0))
spawnCube(new Vector3(1.5, 5, 0), new Vector3(0, 50, 0))
spawnCube(new Vector3(3, 5, 0), new Vector3(0, 0, 50))
