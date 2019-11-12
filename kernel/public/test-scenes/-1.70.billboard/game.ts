import { Entity, BoxShape, engine, Vector3, Transform, Material, Color3, Billboard } from 'decentraland-ecs/src'

export function makeBox(position: Vector3, scale: Vector3, color: string, x: boolean, y: boolean, z: boolean) {
  const ent = new Entity()
  const box = new BoxShape()
  const mat = new Material()
  ent.addComponentOrReplace(new Billboard(x, y, z))
  mat.albedoColor = Color3.FromHexString(color)
  ent.addComponentOrReplace(box)
  ent.addComponentOrReplace(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeBox(new Vector3(1, 1.6, 0), new Vector3(0.5, 0.5, 0.5), '#ff0000', true, false, false)
makeBox(new Vector3(2, 1.6, 0), new Vector3(0.5, 0.5, 0.5), '#00ff00', false, true, false)
makeBox(new Vector3(3, 1.6, 0), new Vector3(0.5, 0.5, 0.5), '#0000ff', false, false, true)
makeBox(new Vector3(1, 1.6, 5), new Vector3(0.5, 0.5, 0.5), '#ffff00', true, true, false)
makeBox(new Vector3(2, 1.6, 5), new Vector3(0.5, 0.5, 0.5), '#00ffff', false, true, true)
makeBox(new Vector3(3, 1.6, 5), new Vector3(0.5, 0.5, 0.5), '#ff00ff', true, false, true)
makeBox(new Vector3(5, 1.6, 3), new Vector3(0.5, 0.5, 0.5), '#ffffff', true, true, true)
