
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

const deer = new Entity('deer')
engine.addEntity(deer)
deer.setParent(_scene)
const transform3 = new Transform({
  position: new Vector3(9, 0.01533818244934082, 10.68980598449707),
  rotation: new Quaternion(-2.220446049250313e-16, -0.3826834559440613, 4.561941935321556e-8, 0.9238795638084412),
  scale: new Vector3(0.7785987854003906, 0.7785987854003906, 0.7785987854003906)
})
deer.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/Deer.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
deer.addComponentOrReplace(gltfShape2)

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
  position: new Vector3(8.5, 0, 3),
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
  position: new Vector3(9, 0, 13.5),
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
  position: new Vector3(2, 0, 4),
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
  position: new Vector3(13, 0, 8),
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
  position: new Vector3(5, 0.4789273738861084, 1),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform12)
flowerSprouts2.addComponentOrReplace(gltfShape5)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform13 = new Transform({
  position: new Vector3(2.5, 0, 14),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform13)
flowerSprouts3.addComponentOrReplace(gltfShape5)

const snowtree02 = new Entity('snowtree02')
engine.addEntity(snowtree02)
snowtree02.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(4.5, 0, 3.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree02.addComponentOrReplace(transform14)
const gltfShape6 = new GLTFShape("models/SnowTree_02.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
snowtree02.addComponentOrReplace(gltfShape6)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(6, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.1379737854003906, 2.1379737854003906, 2.1379737854003906)
})
snowrock01.addComponentOrReplace(transform15)
const gltfShape7 = new GLTFShape("models/SnowRock_01.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snowrock01.addComponentOrReplace(gltfShape7)

const bush03 = new Entity('bush03')
engine.addEntity(bush03)
bush03.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(11.5, 0, 11.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush03.addComponentOrReplace(transform16)
const gltfShape8 = new GLTFShape("models/Bush_03.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
bush03.addComponentOrReplace(gltfShape8)

const bush01 = new Entity('bush01')
engine.addEntity(bush01)
bush01.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(6.5, 0, 7),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(0.5038633346557617, 0.5038633346557617, 0.5038633346557617)
})
bush01.addComponentOrReplace(transform17)
const gltfShape9 = new GLTFShape("models/Bush_01.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
bush01.addComponentOrReplace(gltfShape9)

const plant01 = new Entity('plant01')
engine.addEntity(plant01)
plant01.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(4, 0, 5.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant01.addComponentOrReplace(transform18)
const gltfShape10 = new GLTFShape("models/Plant_01.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
plant01.addComponentOrReplace(gltfShape10)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(10, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snow02.addComponentOrReplace(transform19)
const gltfShape11 = new GLTFShape("models/Snow_02.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
snow02.addComponentOrReplace(gltfShape11)

const snow01 = new Entity('snow01')
engine.addEntity(snow01)
snow01.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(5, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.5280165672302246, 0.35782039165496826, 1.5280165672302246)
})
snow01.addComponentOrReplace(transform20)
const gltfShape12 = new GLTFShape("models/Snow_01.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
snow01.addComponentOrReplace(gltfShape12)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(14, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree03.addComponentOrReplace(transform21)
const gltfShape13 = new GLTFShape("models/SnowTree_03.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
snowtree03.addComponentOrReplace(gltfShape13)

const flower01 = new Entity('flower01')
engine.addEntity(flower01)
flower01.setParent(_scene)
const transform22 = new Transform({
  position: new Vector3(3.5037384033203125, 0, 9.242154121398926),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower01.addComponentOrReplace(transform22)
const gltfShape14 = new GLTFShape("models/Flower_01.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
flower01.addComponentOrReplace(gltfShape14)

const flower012 = new Entity('flower012')
engine.addEntity(flower012)
flower012.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(12.652639389038086, 0, 2.07572340965271),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower012.addComponentOrReplace(transform23)
flower012.addComponentOrReplace(gltfShape14)

const snowrock012 = new Entity('snowrock012')
engine.addEntity(snowrock012)
snowrock012.setParent(_scene)
const transform24 = new Transform({
  position: new Vector3(14, 0, 6.5),
  rotation: new Quaternion(-1.3891844319574142e-15, -0.8689413070678711, 1.0358586877146081e-7, 0.4949151873588562),
  scale: new Vector3(1, 1, 1)
})
snowrock012.addComponentOrReplace(transform24)
snowrock012.addComponentOrReplace(gltfShape7)

const snowrock03 = new Entity('snowrock03')
engine.addEntity(snowrock03)
snowrock03.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(1.151803970336914, 0, 14.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 0.35647010803222656, 1)
})
snowrock03.addComponentOrReplace(transform25)
const gltfShape15 = new GLTFShape("models/SnowRock_03.glb")
gltfShape15.withCollisions = true
gltfShape15.visible = true
snowrock03.addComponentOrReplace(gltfShape15)
