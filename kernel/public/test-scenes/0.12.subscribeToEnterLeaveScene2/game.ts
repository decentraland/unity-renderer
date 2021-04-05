import { log, engine, Entity, BoxShape, Material, Color3, Transform, Vector3, onEnterScene, onLeaveScene } from 'decentraland-ecs/src'

//Create entity and assign shape
const box = new Entity()
box.addComponent(new BoxShape())

//Create material and configure its fields
const material = new Material()
material.albedoColor = Color3.Gray()

//Assign the material to the entity
box.addComponent(material)

box.addComponent(new Transform({
  position: new Vector3(8, 0, 16),
  scale: new Vector3(16, 0.1, 32),
}))

engine.addEntity(box);

onEnterScene.add(({ userId }) => {
  material.albedoColor = Color3.Red()
  log("onEnterScene: ", userId)
})

onLeaveScene.add(({ userId }) => {
  material.albedoColor = Color3.Gray()
  log("onLeaveScene: ", userId)
})