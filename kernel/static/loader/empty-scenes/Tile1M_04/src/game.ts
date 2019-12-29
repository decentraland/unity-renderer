
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

const snowtree02 = new Entity('snowtree02')
engine.addEntity(snowtree02)
snowtree02.setParent(_scene)
const transform3 = new Transform({
  position: new Vector3(6, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree02.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/SnowTree_02.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
snowtree02.addComponentOrReplace(gltfShape2)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform4 = new Transform({
  position: new Vector3(3, 0, 6),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree03.addComponentOrReplace(transform4)
const gltfShape3 = new GLTFShape("models/SnowTree_03.glb")
gltfShape3.withCollisions = true
gltfShape3.visible = true
snowtree03.addComponentOrReplace(gltfShape3)

const snowrock042 = new Entity('snowrock042')
engine.addEntity(snowrock042)
snowrock042.setParent(_scene)
const transform5 = new Transform({
  position: new Vector3(1.5, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock042.addComponentOrReplace(transform5)
const gltfShape4 = new GLTFShape("models/SnowRock_04.glb")
gltfShape4.withCollisions = true
gltfShape4.visible = true
snowrock042.addComponentOrReplace(gltfShape4)

const floorSnow = new Entity('floorSnow')
engine.addEntity(floorSnow)
floorSnow.setParent(_scene)
const transform6 = new Transform({
  position: new Vector3(8, 0.0005483627319335938, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
floorSnow.addComponentOrReplace(transform6)
const gltfShape5 = new GLTFShape("models/Floor_Snow.glb")
gltfShape5.withCollisions = true
gltfShape5.visible = true
floorSnow.addComponentOrReplace(gltfShape5)

const snowtree032 = new Entity('snowtree032')
engine.addEntity(snowtree032)
snowtree032.setParent(_scene)
snowtree032.addComponentOrReplace(gltfShape3)
const transform7 = new Transform({
  position: new Vector3(7.5, 0, 10),
  rotation: new Quaternion(3.6312916547678615e-16, -0.7758766412734985, 9.24916889744054e-8, 0.6308847069740295),
  scale: new Vector3(1.1859164237976074, 1.1859164237976074, 1.1859164237976074)
})
snowtree032.addComponentOrReplace(transform7)

const snowtree022 = new Entity('snowtree022')
engine.addEntity(snowtree022)
snowtree022.setParent(_scene)
snowtree022.addComponentOrReplace(gltfShape2)
const transform8 = new Transform({
  position: new Vector3(13.709029197692871, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree022.addComponentOrReplace(transform8)

const grassSprout = new Entity('grassSprout')
engine.addEntity(grassSprout)
grassSprout.setParent(_scene)
const transform9 = new Transform({
  position: new Vector3(8.5, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout.addComponentOrReplace(transform9)
const gltfShape6 = new GLTFShape("models/Grass_03/Grass_03.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
grassSprout.addComponentOrReplace(gltfShape6)

const grassSprout3 = new Entity('grassSprout3')
engine.addEntity(grassSprout3)
grassSprout3.setParent(_scene)
const transform10 = new Transform({
  position: new Vector3(14, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout3.addComponentOrReplace(transform10)
grassSprout3.addComponentOrReplace(gltfShape6)

const grassSprout4 = new Entity('grassSprout4')
engine.addEntity(grassSprout4)
grassSprout4.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(2, 0, 7.909650802612305),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout4.addComponentOrReplace(transform11)
grassSprout4.addComponentOrReplace(gltfShape6)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform12 = new Transform({
  position: new Vector3(5, 0.20238351821899414, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1.5, 1)
})
grassSprout5.addComponentOrReplace(transform12)
grassSprout5.addComponentOrReplace(gltfShape6)

const grassSprout6 = new Entity('grassSprout6')
engine.addEntity(grassSprout6)
grassSprout6.setParent(_scene)
const transform13 = new Transform({
  position: new Vector3(14, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform13)
grassSprout6.addComponentOrReplace(gltfShape6)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(13, 0, 8),
  rotation: new Quaternion(2.032753924292364e-15, -0.7190765738487244, 8.57205932902616e-8, 0.6949308514595032),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts.addComponentOrReplace(transform14)
const gltfShape7 = new GLTFShape("models/Plant_03/Plant_03.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
flowerSprouts.addComponentOrReplace(gltfShape7)

const flowerSprouts2 = new Entity('flowerSprouts2')
engine.addEntity(flowerSprouts2)
flowerSprouts2.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(7.5, 0.4789273738861084, 5),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform15)
flowerSprouts2.addComponentOrReplace(gltfShape7)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(4, 0, 9.028438568115234),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform16)
flowerSprouts3.addComponentOrReplace(gltfShape7)

const snow01 = new Entity('snow01')
engine.addEntity(snow01)
snow01.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(12.246003150939941, 0, 10.5),
  rotation: new Quaternion(-2.562966850180843e-17, 0.4103763699531555, -4.8920668405116885e-8, 0.9119162559509277),
  scale: new Vector3(1.7326583862304688, 0.18920201063156128, 1.7326583862304688)
})
snow01.addComponentOrReplace(transform17)
const gltfShape8 = new GLTFShape("models/Snow_01.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
snow01.addComponentOrReplace(gltfShape8)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(5.5, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(5.203949928283691, 0.5700840950012207, 5.203949928283691)
})
snow02.addComponentOrReplace(transform18)
const gltfShape9 = new GLTFShape("models/Snow_02.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
snow02.addComponentOrReplace(gltfShape9)

const plant01 = new Entity('plant01')
engine.addEntity(plant01)
plant01.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(9.5, 0, 14),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant01.addComponentOrReplace(transform19)
const gltfShape10 = new GLTFShape("models/Plant_01.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
plant01.addComponentOrReplace(gltfShape10)

const fire01 = new Entity('fire01')
engine.addEntity(fire01)
fire01.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(5.5, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
fire01.addComponentOrReplace(transform20)
const gltfShape11 = new GLTFShape("models/Fire_01.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
fire01.addComponentOrReplace(gltfShape11)

const bush03 = new Entity('bush03')
engine.addEntity(bush03)
bush03.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(11.5, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(0.8075332641601562, 0.8075332641601562, 0.8075332641601562)
})
bush03.addComponentOrReplace(transform21)
const gltfShape12 = new GLTFShape("models/Bush_03.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
bush03.addComponentOrReplace(gltfShape12)

const plant012 = new Entity('plant012')
engine.addEntity(plant012)
plant012.setParent(_scene)
const transform22 = new Transform({
  position: new Vector3(6, 0, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant012.addComponentOrReplace(transform22)
plant012.addComponentOrReplace(gltfShape10)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(2, 0, 3.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock01.addComponentOrReplace(transform23)
const gltfShape13 = new GLTFShape("models/SnowRock_01.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
snowrock01.addComponentOrReplace(gltfShape13)

const snowlog03 = new Entity('snowlog03')
engine.addEntity(snowlog03)
snowlog03.setParent(_scene)
const transform24 = new Transform({
  position: new Vector3(4.047587871551514, 0, 10.799144744873047),
  rotation: new Quaternion(-1.045443850643279e-15, 0.449405699968338, -5.357333421329713e-8, 0.8933277726173401),
  scale: new Vector3(1, 1, 1)
})
snowlog03.addComponentOrReplace(transform24)
const gltfShape14 = new GLTFShape("models/SnowLog_03.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
snowlog03.addComponentOrReplace(gltfShape14)

const bush032 = new Entity('bush032')
engine.addEntity(bush032)
bush032.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(4, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(0.7331709861755371, 0.7331709861755371, 0.7331709861755371)
})
bush032.addComponentOrReplace(transform25)
bush032.addComponentOrReplace(gltfShape12)
