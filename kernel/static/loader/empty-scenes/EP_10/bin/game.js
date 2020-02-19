
const _scene = new Entity('_scene')
engine.addEntity(_scene)
const transform = new Transform({
  position: new Vector3(0, 0, 0),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
_scene.addComponentOrReplace(transform)

const ep = new Entity('ep')
engine.addEntity(ep)
ep.setParent(_scene)
const transform3 = new Transform({
  position: new Vector3(8, 0, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
ep.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/EP_10.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
ep.addComponentOrReplace(gltfShape2)
