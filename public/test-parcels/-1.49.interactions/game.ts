import { BoxShape, Transform, Material, engine, Entity, Vector3, Color3, OnPointerDown } from 'decentraland-ecs/src'

const c1 = Color3.FromHexString('#ff0000')
const c2 = Color3.FromHexString('#00ffFF')

const ent = new Entity()
ent.set(
  new Transform({
    position: new Vector3()
  })
)

const mat = new Material()
mat.albedoColor = c1
ent.set(mat)

ent.set(new BoxShape())

ent.set(
  new OnPointerDown(() => {
    mat.albedoColor = mat.albedoColor && mat.albedoColor.toHexString() === '#ff0000' ? c2 : c1
  })
)

engine.addEntity(ent)
