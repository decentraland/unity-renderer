
dcl.subscribe('sceneStart')

function Vector3(x, y, z) {
  return { x, y, z }
}
function Quaternion(x, y, z, w) {
  return { x, y, z, w }
}
function Transform(transformData) {
  return transformData
}
function GLTFShape(url) {
  return { src: url }
}
const ids = {}
function normalEntityId(id) {
  if (!ids[id]) {
    ids[id] = (Object.keys(ids).length + 1).toString(10)
  }
  return ids[id]
}
function Entity(id) {
  this.id = normalEntityId(id)
  return this
}
Entity.prototype.setParent = function(parent) {
  dcl.setParent(this.id, parent.id)
}
Entity.prototype.addComponentOrReplace = function(component) {
  if (component.position && component.rotation && component.scale) {
    dcl.updateEntityComponent(this.id, 'engine.transform', 1, JSON.stringify(component))
  } else if (component.src) {
    dcl.componentCreated('gl_' + this.id, 'engine.shape', 54)
    dcl.componentUpdated('gl_' + this.id, JSON.stringify(component))
    dcl.attachEntityComponent(this.id, 'engine.shape', 'gl_' + this.id)
  }
}
var engine = {
  addEntity: function(entity) {
    dcl.addEntity(entity.id)
    if (entity.id === "1") {
      dcl.setParent(entity.id, "0")
    }
  }
}

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
const gltfShape2 = new GLTFShape("models/EP_09.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
ep.addComponentOrReplace(gltfShape2)
