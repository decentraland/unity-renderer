import {
  engine,
  Material,
  Entity,
  Transform,
  PlaneShape,
  Vector3,
  Billboard,
  Texture,
  BoxShape
} from 'decentraland-ecs/src'

const root = new Entity()
engine.addEntity(root)
root.getComponentOrCreate(Transform).scale.setAll(1.6)

const ent1 = new Entity()
const mat1 = new Material()
const shape1 = new PlaneShape()
const uvs = [0, 1, 1, 1, 1, 0, 0, 0]
shape1.uvs = [...uvs, ...uvs]

const t1 = new Transform({
  position: new Vector3(6, 1.6, 5),
  scale: new Vector3(4, 4, 4)
})

mat1.metallic = 1
mat1.roughness = 1
mat1.alphaTexture = mat1.albedoTexture = new Texture('img.png', { hasAlpha: true, samplingMode: 3 })

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
  position: new Vector3(3, 1.6, 5),
  scale: new Vector3(4, 4, 4)
})

mat2.metallic = 0
mat2.roughness = 1
mat2.albedoTexture = new Texture('img.png', { hasAlpha: true, samplingMode: 0 })

const Q = new Entity()
engine.addEntity(Q)
Q.addComponent(mat2)
Q.addComponent(new BoxShape())
Q.getComponentOrCreate(Transform).position.set(3, 3, 3)

ent2.addComponentOrReplace(new Billboard())
ent2.addComponentOrReplace(mat2)
ent2.addComponentOrReplace(t2)
ent2.addComponentOrReplace(shape2)
ent2.setParent(root)
engine.addEntity(ent2)
