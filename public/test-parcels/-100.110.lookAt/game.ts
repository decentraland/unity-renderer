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

  const t = new Transform({
    position: pos
  })
  axis.addComponentOrReplace(t)
  t.lookAt(new Vector3(5, 5, 5))

  engine.addEntity(axis)
  engine.addEntity(xAxis)
  engine.addEntity(yAxis)
  engine.addEntity(zAxis)

  xAxis.setParent(axis)
  yAxis.setParent(axis)
  zAxis.setParent(axis)

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
