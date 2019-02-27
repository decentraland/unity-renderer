import { engine, Material, Entity, Transform, PlaneShape, Vector3, Billboard } from 'decentraland-ecs/src'

const root = new Entity()
engine.addEntity(root)
root.getComponentOrCreate(Transform).scale.setAll(1.6)

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
ent1.addComponentOrReplace(new Billboard())
ent1.addComponentOrReplace(mat1)
ent1.addComponentOrReplace(t1)
ent1.addComponentOrReplace(shape1)
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
ent2.addComponentOrReplace(new Billboard())
ent2.addComponentOrReplace(mat2)
ent2.addComponentOrReplace(t2)
ent2.addComponentOrReplace(shape2)
ent2.setParent(root)
engine.addEntity(ent2)
