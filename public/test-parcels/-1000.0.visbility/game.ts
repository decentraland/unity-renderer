import {
  GLTFShape,
  Vector3,
  Entity,
  Transform,
  BoxShape,
  ConeShape,
  CylinderShape,
  engine,
  PlaneShape,
  SphereShape,
  CircleShape
} from 'decentraland-ecs/src'

function makeEntity(shape: any, position: Vector3, visible: boolean) {
  const ent = new Entity()
  const s = new shape()
  s.visible = visible
  ent.addComponentOrReplace(s)  
  ent.addComponentOrReplace(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

const entity = new Entity()
var gltfShape = new GLTFShape('models/Tree_Scene.glb');
gltfShape.visible = true
entity.addComponent(gltfShape)
entity.addComponent(new Transform({ position: new Vector3(8, 0, 8) }))
engine.addEntity(entity)

makeEntity(BoxShape, new Vector3(0, 2, -2), true)
makeEntity(ConeShape, new Vector3(-12, 2, -2), false)
makeEntity(CylinderShape, new Vector3(8, 2, 4), true)
makeEntity(PlaneShape, new Vector3(12, 2, -2), false)
let sphereEnt = makeEntity(SphereShape, new Vector3(14, 2, -2), true)
makeEntity(CircleShape, new Vector3(26, 2, 2), true)

let totalTime = 0

class AWaiterSystem {
  static instance = new AWaiterSystem()

  update(dt: number) {
    totalTime += dt

    if (totalTime > 8) {
      sphereEnt.getComponent(SphereShape).visible = false
	  gltfShape.visible = false
    }
  }
}

engine.addSystem(AWaiterSystem.instance)