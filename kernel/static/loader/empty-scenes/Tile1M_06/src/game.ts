
const _scene = new Entity('_scene')
engine.addEntity(_scene)
const transform = new Transform({
  position: new Vector3(0, 0, 0),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
_scene.addComponentOrReplace(transform)

const entity = new Entity('entity')
engine.addEntity(entity)
entity.setParent(_scene)
const gltfShape = new GLTFShape("models/FloorBaseGrass_01/FloorBaseGrass_01.glb")
gltfShape.withCollisions = true
gltfShape.visible = true
entity.addComponentOrReplace(gltfShape)
const transform2 = new Transform({
  position: new Vector3(8, 0, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
entity.addComponentOrReplace(transform2)

const snowrock04 = new Entity('snowrock04')
engine.addEntity(snowrock04)
snowrock04.setParent(_scene)
const transform3 = new Transform({
  position: new Vector3(8, 0, 3.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock04.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/SnowRock_04.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
snowrock04.addComponentOrReplace(gltfShape2)

const floorSnow = new Entity('floorSnow')
engine.addEntity(floorSnow)
floorSnow.setParent(_scene)
const transform4 = new Transform({
  position: new Vector3(8, 0.0005483627319335938, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
floorSnow.addComponentOrReplace(transform4)
const gltfShape3 = new GLTFShape("models/Floor_Snow.glb")
gltfShape3.withCollisions = true
gltfShape3.visible = true
floorSnow.addComponentOrReplace(gltfShape3)

const grassSprout = new Entity('grassSprout')
engine.addEntity(grassSprout)
grassSprout.setParent(_scene)
const transform5 = new Transform({
  position: new Vector3(4.5, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout.addComponentOrReplace(transform5)
const gltfShape4 = new GLTFShape("models/Grass_03/Grass_03.glb")
gltfShape4.withCollisions = true
gltfShape4.visible = true
grassSprout.addComponentOrReplace(gltfShape4)

const grassSprout2 = new Entity('grassSprout2')
engine.addEntity(grassSprout2)
grassSprout2.setParent(_scene)
const transform6 = new Transform({
  position: new Vector3(2.5, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout2.addComponentOrReplace(transform6)
grassSprout2.addComponentOrReplace(gltfShape4)

const grassSprout3 = new Entity('grassSprout3')
engine.addEntity(grassSprout3)
grassSprout3.setParent(_scene)
const transform7 = new Transform({
  position: new Vector3(14, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout3.addComponentOrReplace(transform7)
grassSprout3.addComponentOrReplace(gltfShape4)

const grassSprout4 = new Entity('grassSprout4')
engine.addEntity(grassSprout4)
grassSprout4.setParent(_scene)
const transform8 = new Transform({
  position: new Vector3(2, 0, 7.909650802612305),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout4.addComponentOrReplace(transform8)
grassSprout4.addComponentOrReplace(gltfShape4)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform9 = new Transform({
  position: new Vector3(10, 0, 9),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout5.addComponentOrReplace(transform9)
grassSprout5.addComponentOrReplace(gltfShape4)

const grassSprout6 = new Entity('grassSprout6')
engine.addEntity(grassSprout6)
grassSprout6.setParent(_scene)
const transform10 = new Transform({
  position: new Vector3(14, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform10)
grassSprout6.addComponentOrReplace(gltfShape4)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(14.5, 0, 14.5),
  rotation: new Quaternion(2.032753924292364e-15, -0.7190765738487244, 8.57205932902616e-8, 0.6949308514595032),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts.addComponentOrReplace(transform11)
const gltfShape5 = new GLTFShape("models/Plant_03/Plant_03.glb")
gltfShape5.withCollisions = true
gltfShape5.visible = true
flowerSprouts.addComponentOrReplace(gltfShape5)

const flowerSprouts2 = new Entity('flowerSprouts2')
engine.addEntity(flowerSprouts2)
flowerSprouts2.setParent(_scene)
const transform12 = new Transform({
  position: new Vector3(3, 0.4789273738861084, 1.5),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform12)
flowerSprouts2.addComponentOrReplace(gltfShape5)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform13 = new Transform({
  position: new Vector3(4, 0, 9.028438568115234),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform13)
flowerSprouts3.addComponentOrReplace(gltfShape5)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(7, 0, 4.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree03.addComponentOrReplace(transform14)
const gltfShape6 = new GLTFShape("models/SnowTree_03.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
snowtree03.addComponentOrReplace(gltfShape6)

const snowtree02 = new Entity('snowtree02')
engine.addEntity(snowtree02)
snowtree02.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(13.5, 0, 10),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree02.addComponentOrReplace(transform15)
const gltfShape7 = new GLTFShape("models/SnowTree_02.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snowtree02.addComponentOrReplace(gltfShape7)

const snow03 = new Entity('snow03')
engine.addEntity(snow03)
snow03.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(11.549419403076172, 0, 5.4835205078125),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.8951902389526367, 0.8693055510520935, 2.8951902389526367)
})
snow03.addComponentOrReplace(transform16)
const gltfShape8 = new GLTFShape("models/Snow_03.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
snow03.addComponentOrReplace(gltfShape8)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(7.5, 0, 11.685281753540039),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(3.1404342651367188, 0.7164441347122192, 4.790041923522949)
})
snow02.addComponentOrReplace(transform17)
const gltfShape9 = new GLTFShape("models/Snow_02.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
snow02.addComponentOrReplace(gltfShape9)

const bush02 = new Entity('bush02')
engine.addEntity(bush02)
bush02.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(3, 0, 11.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush02.addComponentOrReplace(transform18)
const gltfShape10 = new GLTFShape("models/Bush_02.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
bush02.addComponentOrReplace(gltfShape10)

const flower01 = new Entity('flower01')
engine.addEntity(flower01)
flower01.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(3.5, 0, 5.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower01.addComponentOrReplace(transform19)
const gltfShape11 = new GLTFShape("models/Flower_01.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
flower01.addComponentOrReplace(gltfShape11)

const flower012 = new Entity('flower012')
engine.addEntity(flower012)
flower012.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(10.5, 0, 13),
  rotation: new Quaternion(7.985651264423682e-17, -0.49658942222595215, 5.919806866927502e-8, -0.8679856061935425),
  scale: new Vector3(1, 1, 1)
})
flower012.addComponentOrReplace(transform20)
flower012.addComponentOrReplace(gltfShape11)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(14.194491386413574, 0, 12.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock01.addComponentOrReplace(transform21)
const gltfShape12 = new GLTFShape("models/SnowRock_01.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
snowrock01.addComponentOrReplace(gltfShape12)
