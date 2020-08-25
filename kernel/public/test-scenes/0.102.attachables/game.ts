import { Entity, engine, Vector3, Transform, BoxShape, Material, Color3, Attachable } from 'decentraland-ecs/src'

// This entity will rotate with the camera, when in first person mode
const followTheCamera = new Entity()
followTheCamera.addComponent(new BoxShape())
followTheCamera.addComponent(new Transform({
  position: new Vector3(1, 0, 1),
  scale: new Vector3(0.5, 0.5, 2)
}))
engine.addEntity(followTheCamera)
followTheCamera.setParent(Attachable.PLAYER)

// This entity will follow the avatar, and remain unaffected by the camera rotation
const followAvatar = new Entity()
followAvatar.addComponent(new BoxShape())
followAvatar.addComponent(new Transform({
  position: new Vector3(0, 0, 0.5),
  scale: new Vector3(0.5, 0.5, 0.5)
}))
const material = new Material()
material.albedoColor = Color3.FromHexString('#FF00FF')
material.metallic = 0.2
material.roughness = 1.0
followAvatar.addComponent(material)
engine.addEntity(followAvatar)
followAvatar.setParent(Attachable.AVATAR_POSITION)



