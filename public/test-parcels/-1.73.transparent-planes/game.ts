import { PlaneShape, Material, Entity, engine, Transform, Billboard, Texture } from 'decentraland-ecs/src'

const tex = new Texture('img #7 @ $1.png', { hasAlpha: true })

const m1 = new Material()
m1.albedoTexture = tex
m1.alphaTexture = tex
m1.metallic = 1
m1.roughness = 1

const m2 = new Material()
m2.albedoTexture = tex
m2.hasAlpha = true
m2.metallic = 0
m2.roughness = 1

const p1 = new PlaneShape()
const billboard = new Billboard(true, true, true)

{
  const e1 = new Entity()
  e1.addComponentOrReplace(p1)
  e1.addComponentOrReplace(m1)
  e1.addComponentOrReplace(billboard)
  engine.addEntity(e1)
  e1.getComponentOrCreate(Transform).position.set(5, 1.6, 5)
}

{
  const e2 = new Entity()
  e2.addComponentOrReplace(p1)
  e2.addComponentOrReplace(m2)
  e2.addComponentOrReplace(billboard)
  engine.addEntity(e2)
  e2.getComponentOrCreate(Transform).position.set(4, 1.6, 5)
}
