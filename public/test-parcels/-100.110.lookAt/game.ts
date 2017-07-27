import { engine, Entity, Transform, Vector3, BoxShape, Material, Color3 } from 'decentraland-ecs'

const box = new BoxShape()
const redMaterial = new Material()
redMaterial.albedoColor = Color3.FromHexString('#FF0000')
const greenMaterial = new Material()
greenMaterial.albedoColor = Color3.FromHexString('#00FF00')
const blueMaterial = new Material()
blueMaterial.albedoColor = Color3.FromHexString('#0000FF')

function spawnAxis(pos: Vector3) {
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
    position: pos
  })
  axis.set(t)
  t.lookAt(new Vector3(5, 5, 5))

  engine.addEntity(axis)
  engine.addEntity(xAxis)
  engine.addEntity(yAxis)
  engine.addEntity(zAxis)

  return axis
}

spawnAxis(new Vector3(1.5, 1.5, 1.5))
spawnAxis(new Vector3(1.5, 1.5, 8.5))
spawnAxis(new Vector3(1.5, 8.5, 1.5))
spawnAxis(new Vector3(1.5, 8.5, 8.5))
spawnAxis(new Vector3(8.5, 1.5, 8.5))
spawnAxis(new Vector3(8.5, 1.5, 1.5))
spawnAxis(new Vector3(8.5, 8.5, 1.5))
spawnAxis(new Vector3(8.5, 8.5, 8.5))
