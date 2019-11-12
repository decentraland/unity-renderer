import {
  PlaneShape,
  Material,
  Entity,
  engine,
  Transform,
  Billboard,
  Texture,
  TransparencyMode
} from 'decentraland-ecs/src'

const tex = new Texture('img #7 @ $1.png', { hasAlpha: true })

const m1 = new Material()
m1.albedoTexture = tex
m1.metallic = 0
m1.roughness = 1
m1.alphaTest = 0.5
m1.transparencyMode = TransparencyMode.ALPHA_BLEND

const m2 = new Material()
m2.albedoTexture = tex
m2.metallic = 0
m2.roughness = 1
m2.transparencyMode = TransparencyMode.ALPHA_TEST

const m3 = new Material()
m3.albedoTexture = tex
m3.alphaTexture = tex
m3.metallic = 0
m3.roughness = 1
m3.transparencyMode = TransparencyMode.AUTO

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

{
  const e3 = new Entity()
  e3.addComponentOrReplace(p1)
  e3.addComponentOrReplace(m3)
  e3.addComponentOrReplace(billboard)
  engine.addEntity(e3)
  e3.getComponentOrCreate(Transform).position.set(3, 1.6, 5)
}
