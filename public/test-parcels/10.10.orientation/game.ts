import {
  Entity,
  engine,
  Vector3,
  Transform,
  Material,
  Color3,
  GLTFShape,
  BoxShape,
  SphereShape,
  OBJShape,
  Quaternion,
  TextShape
} from 'decentraland-ecs/src'

function makeThing(Shape: any, position: Vector3, scale?: Vector3) {
  const ent = new Entity()
  const shape = new Shape()
  shape.withCollisions = true
  ent.addComponentOrReplace(shape)

  ent.addComponentOrReplace(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

const glassMaterial = new Material()
glassMaterial.albedoColor = Color3.White()
glassMaterial.metallic = 0.9
glassMaterial.roughness = 0.1

const plasticMaterial = new Material()
plasticMaterial.albedoColor = Color3.White()
plasticMaterial.metallic = 0.1
plasticMaterial.roughness = 0.9

const pinkMaterial = new Material()
pinkMaterial.albedoColor = Color3.FromHexString('#EF2D5E')

const stair1mat = new Material()
stair1mat.albedoColor = Color3.FromHexString('#7BC8A4')
stair1mat.emissiveColor = Color3.FromHexString('#7BC8A4')

const stair2mat = new Material()
stair2mat.albedoColor = Color3.FromHexString('#FF0000')
stair2mat.emissiveColor = Color3.FromHexString('#FF0000')

const stair3mat = new Material()
stair3mat.albedoColor = Color3.FromHexString('#00FF00')
stair3mat.emissiveColor = Color3.FromHexString('#00FF00')

const stair4mat = new Material()
stair4mat.albedoColor = Color3.FromHexString('#0000FF')
stair4mat.emissiveColor = Color3.FromHexString('#0000FF')

const e1 = makeThing(SphereShape, new Vector3(3, 1.25, 5), new Vector3(1.5, 1.5, 1.5))
e1.addComponentOrReplace(glassMaterial)

const e2 = makeThing(SphereShape, new Vector3(5, 1.25, 5), new Vector3(1.5, 1.5, 1.5))
e2.addComponentOrReplace(plasticMaterial)

const e3 = makeThing(BoxShape, new Vector3(0.5, 0.5, 0.5))
const e3mat = new Material()
e3mat.albedoColor = Color3.FromHexString('#4CC3D9')
e3.addComponentOrReplace(e3mat)

const e4 = makeThing(BoxShape, new Vector3(9.5, 0.5, 9.5))
e4.addComponentOrReplace(pinkMaterial)

const e5 = makeThing(BoxShape, new Vector3(9.5, 0.5, 0.5))
const e5mat = new Material()
e5mat.albedoColor = Color3.FromHexString('#FFC65D')
e5.addComponentOrReplace(e5mat)

const e6 = makeThing(BoxShape, new Vector3(0.5, 0.5, 9.5))
const e6mat = new Material()
e6mat.albedoColor = Color3.FromHexString('#7BC8A4')
e6.addComponentOrReplace(e6mat)

const e7 = makeThing(BoxShape, new Vector3(4, 0.5, 3))
e7.getComponent(Transform).rotation = Quaternion.Euler(0, 45, 0)
const e7mat = new Material()
e7mat.albedoColor = Color3.FromHexString('#4CC3D9')
e7.addComponentOrReplace(e7mat)

const container = new Entity()
container.addComponentOrReplace(
  new Transform({
    position: new Vector3(5, 0, 5),
    scale: new Vector3(0.3, 1, 0.3)
  })
)
engine.addEntity(container)

const e8 = makeThing(BoxShape, new Vector3(9.5, 0, 9.5), new Vector3(10, 0.1, 10))
e8.addComponentOrReplace(stair1mat)
e8.setParent(container)

const e9 = makeThing(BoxShape, new Vector3(9.5, 0.1, 7.5), new Vector3(10, 0.1, 10))
e9.addComponentOrReplace(stair2mat)
e9.setParent(container)

const e10 = makeThing(BoxShape, new Vector3(9.5, 0.2, 5.5), new Vector3(10, 0.1, 10))
e10.addComponentOrReplace(stair3mat)
e10.setParent(container)

const e11 = makeThing(BoxShape, new Vector3(9.5, 0.3, 3.5), new Vector3(10, 0.1, 10))
e11.addComponentOrReplace(stair4mat)
e11.setParent(container)

const e12 = new Entity()
e12.addComponentOrReplace(
  new Transform({
    position: new Vector3(-5, 0, 19)
  })
)
e12.addComponentOrReplace(new GLTFShape('stairs.gltf'))
engine.addEntity(e12)

const e13 = new Entity()
e13.addComponentOrReplace(
  new Transform({
    position: new Vector3(-5, 0, 31),
    scale: new Vector3(0.2, 0.2, 0.2)
  })
)
e13.addComponentOrReplace(new GLTFShape('Test_abc.gltf'))
engine.addEntity(e13)

const e14 = new Entity()
e14.addComponentOrReplace(
  new Transform({
    position: new Vector3(-15, 0, 31),
    scale: new Vector3(0.2, 0.2, 0.2)
  })
)
e14.addComponentOrReplace(new OBJShape('Test_abc.obj'))
engine.addEntity(e14)

const e15 = makeThing(BoxShape, new Vector3(-11, 0.3, 45), new Vector3(10, 0.1, 10))
e15.getComponent(Transform).rotation = Quaternion.Euler(20, 0, 0)
e15.addComponentOrReplace(e6mat)

const e16 = new Entity()
e16.addComponentOrReplace(
  new Transform({
    position: new Vector3(-15, 0, 31),
    scale: new Vector3(0.2, 0.2, 0.2)
  })
)
const e16text = new TextShape('Hello world')
e16text.color = Color3.FromHexString('#7BC8A4')
e16.addComponentOrReplace(e16text)

const e17 = new Entity()
e17.addComponentOrReplace(
  new Transform({
    position: new Vector3(-15, 0, 31),
    scale: new Vector3(0.2, 0.2, 0.2)
  })
)
const e17text = new TextShape('Hello world')
e17text.color = new Color3(10 / 255, 173 / 255, 34 / 255)
e17.addComponentOrReplace(e17text)
