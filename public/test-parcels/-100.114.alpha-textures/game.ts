import { engine, Material, Entity, Transform, PlaneShape, Vector3, Billboard } from 'decentraland-ecs/src'

const root = new Entity()
engine.addEntity(root)
root.getOrCreate(Transform).scale.setAll(1.6)

const ent1 = new Entity()
const mat1 = new Material()
const shape1 = new PlaneShape()
const t1 = new Transform({
  position: new Vector3(5, 1.6, 5)
})

mat1.metallic = 1
mat1.roughness = 1
mat1.albedoTexture = 'img.png'
mat1.alphaTexture = 'img.png'
ent1.setParent(root)
ent1.set(new Billboard())
ent1.set(mat1)
ent1.set(t1)
ent1.set(shape1)
engine.addEntity(ent1)

const ent2 = new Entity()
const mat2 = new Material()
const shape2 = new PlaneShape()
const t2 = new Transform({
  position: new Vector3(4, 1.6, 5)
})
mat2.metallic = 0
mat2.roughness = 1
mat2.albedoTexture = 'img.png'
mat2.hasAlpha = true
ent2.set(new Billboard())
ent2.set(mat2)
ent2.set(t2)
ent2.set(shape2)
ent2.setParent(root)
engine.addEntity(ent2)
