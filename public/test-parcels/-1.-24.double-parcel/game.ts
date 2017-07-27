import {
  engine,
  BoxShape,
  PlaneShape,
  CylinderShape,
  Entity,
  Vector3,
  Transform,
  Material,
  Color3,
  SphereShape
} from 'decentraland-ecs/src'

const entity = new Entity()
const mat1 = new Material()
entity.set(new BoxShape())
entity.set(
  new Transform({
    position: new Vector3(0.5, 0.5, 0.5)
  })
)
mat1.albedoColor = Color3.FromHexString('#4CC3D9')
entity.set(mat1)

const entity2 = new Entity()
const mat2 = new Material()
entity2.set(new BoxShape())
entity2.set(
  new Transform({
    position: new Vector3(9.5, 0.5, 9.5)
  })
)
mat2.albedoColor = Color3.FromHexString('#EF2D5E')
entity2.set(mat2)

const entity3 = new Entity()
const mat3 = new Material()
entity3.set(new BoxShape())
entity3.set(
  new Transform({
    position: new Vector3(9.5, 0.5, 0.5)
  })
)
mat3.albedoColor = Color3.FromHexString('#FFC65D')
entity3.set(mat3)

const entity4 = new Entity()
const mat4 = new Material()
entity4.set(new BoxShape())
entity4.set(
  new Transform({
    position: new Vector3(0.5, 0.5, 9.5)
  })
)
mat4.albedoColor = Color3.FromHexString('#7BC8A4')
entity4.set(mat4)

const entity5 = new Entity()
const mat5 = new Material()
entity5.set(new BoxShape())
entity5.set(
  new Transform({
    position: new Vector3(4, 0.5, 3)
  })
)
mat5.albedoColor = Color3.FromHexString('#4CC3D9')
entity5.set(mat5)

const entity6 = new Entity()
const mat6 = new Material()
entity6.set(new SphereShape())
entity6.set(
  new Transform({
    position: new Vector3(3, 1.25, 5),
    scale: new Vector3(1.25, 1, 1.25)
  })
)
mat6.albedoColor = Color3.FromHexString('#EF2D5E')
entity6.set(mat6)

const entity7 = new Entity()
const mat7 = new Material()
entity7.set(new CylinderShape())
entity7.set(
  new Transform({
    position: new Vector3(1, 0.75, 3)
  })
)
mat7.albedoColor = Color3.FromHexString('#FFC65D')
entity7.set(mat7)

const entity8 = new Entity()
const mat8 = new Material()
entity8.set(new PlaneShape())
entity8.set(
  new Transform({
    position: new Vector3(0, 0, 4),
    scale: new Vector3(4, 1, 4)
  })
)
mat8.albedoColor = Color3.FromHexString('#7BC8A4')
entity8.set(mat8)

engine.addEntity(entity)
engine.addEntity(entity2)
engine.addEntity(entity3)
engine.addEntity(entity4)
engine.addEntity(entity5)
engine.addEntity(entity6)
engine.addEntity(entity7)
engine.addEntity(entity8)
