
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

const snowtree01 = new Entity('snowtree01')
engine.addEntity(snowtree01)
snowtree01.setParent(_scene)
const transform3 = new Transform({
  position: new Vector3(9.5, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree01.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/SnowTree_01.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
snowtree01.addComponentOrReplace(gltfShape2)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform4 = new Transform({
  position: new Vector3(6, 0, 11),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.1635494232177734, 1.1635494232177734, 1.1635494232177734)
})
snowtree03.addComponentOrReplace(transform4)
const gltfShape3 = new GLTFShape("models/SnowTree_03.glb")
gltfShape3.withCollisions = true
gltfShape3.visible = true
snowtree03.addComponentOrReplace(gltfShape3)

const snowrock04 = new Entity('snowrock04')
engine.addEntity(snowrock04)
snowrock04.setParent(_scene)
const transform5 = new Transform({
  position: new Vector3(3.5, 0, 9),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock04.addComponentOrReplace(transform5)
const gltfShape4 = new GLTFShape("models/SnowRock_04.glb")
gltfShape4.withCollisions = true
gltfShape4.visible = true
snowrock04.addComponentOrReplace(gltfShape4)

const snowlog01 = new Entity('snowlog01')
engine.addEntity(snowlog01)
snowlog01.setParent(_scene)
const transform6 = new Transform({
  position: new Vector3(5, 0, 9.423120498657227),
  rotation: new Quaternion(7.637795347065533e-17, 0.34715867042541504, -4.138453491009386e-8, 0.9378064870834351),
  scale: new Vector3(1, 1, 1)
})
snowlog01.addComponentOrReplace(transform6)
const gltfShape5 = new GLTFShape("models/SnowLog_01.glb")
gltfShape5.withCollisions = true
gltfShape5.visible = true
snowlog01.addComponentOrReplace(gltfShape5)

const snowrock042 = new Entity('snowrock042')
engine.addEntity(snowrock042)
snowrock042.setParent(_scene)
const transform7 = new Transform({
  position: new Vector3(10.5, 0, 12.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock042.addComponentOrReplace(transform7)
snowrock042.addComponentOrReplace(gltfShape4)

const floorSnow = new Entity('floorSnow')
engine.addEntity(floorSnow)
floorSnow.setParent(_scene)
const transform8 = new Transform({
  position: new Vector3(8, 0.0005483627319335938, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
floorSnow.addComponentOrReplace(transform8)
const gltfShape6 = new GLTFShape("models/Floor_Snow.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
floorSnow.addComponentOrReplace(gltfShape6)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform9 = new Transform({
  position: new Vector3(12, 0.3062257766723633, 4.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock01.addComponentOrReplace(transform9)
const gltfShape7 = new GLTFShape("models/SnowRock_01.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snowrock01.addComponentOrReplace(gltfShape7)

const snowtree022 = new Entity('snowtree022')
engine.addEntity(snowtree022)
snowtree022.setParent(_scene)
const gltfShape8 = new GLTFShape("models/SnowTree_02.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
snowtree022.addComponentOrReplace(gltfShape8)
const transform10 = new Transform({
  position: new Vector3(13.709029197692871, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree022.addComponentOrReplace(transform10)

const grassSprout = new Entity('grassSprout')
engine.addEntity(grassSprout)
grassSprout.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(8.5, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout.addComponentOrReplace(transform11)
const gltfShape9 = new GLTFShape("models/Grass_03/Grass_03.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
grassSprout.addComponentOrReplace(gltfShape9)

const grassSprout2 = new Entity('grassSprout2')
engine.addEntity(grassSprout2)
grassSprout2.setParent(_scene)
const transform12 = new Transform({
  position: new Vector3(2.5, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout2.addComponentOrReplace(transform12)
grassSprout2.addComponentOrReplace(gltfShape9)

const grassSprout3 = new Entity('grassSprout3')
engine.addEntity(grassSprout3)
grassSprout3.setParent(_scene)
const transform13 = new Transform({
  position: new Vector3(14, 0, 9),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 2, 1)
})
grassSprout3.addComponentOrReplace(transform13)
grassSprout3.addComponentOrReplace(gltfShape9)

const grassSprout4 = new Entity('grassSprout4')
engine.addEntity(grassSprout4)
grassSprout4.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(2, 0, 7.909650802612305),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout4.addComponentOrReplace(transform14)
grassSprout4.addComponentOrReplace(gltfShape9)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(9, 0, 7),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout5.addComponentOrReplace(transform15)
grassSprout5.addComponentOrReplace(gltfShape9)

const grassSprout6 = new Entity('grassSprout6')
engine.addEntity(grassSprout6)
grassSprout6.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(14, 0.2151179313659668, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform16)
grassSprout6.addComponentOrReplace(gltfShape9)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(14, 0, 12.5),
  rotation: new Quaternion(2.032753924292364e-15, -0.7190765738487244, 8.57205932902616e-8, 0.6949308514595032),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts.addComponentOrReplace(transform17)
const gltfShape10 = new GLTFShape("models/Plant_03/Plant_03.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
flowerSprouts.addComponentOrReplace(gltfShape10)

const flowerSprouts2 = new Entity('flowerSprouts2')
engine.addEntity(flowerSprouts2)
flowerSprouts2.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(1, 0.4789273738861084, 13),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform18)
flowerSprouts2.addComponentOrReplace(gltfShape10)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(6.5, 0, 7),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform19)
flowerSprouts3.addComponentOrReplace(gltfShape10)

const snow01 = new Entity('snow01')
engine.addEntity(snow01)
snow01.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(11.735761642456055, 0, 5.412519931793213),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.1069202423095703, 0.4091476798057556, 2.988677501678467)
})
snow01.addComponentOrReplace(transform20)
const gltfShape11 = new GLTFShape("models/Snow_01.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
snow01.addComponentOrReplace(gltfShape11)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(3.53665828704834, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.8577499389648438, 0.3608022928237915, 3.347811698913574)
})
snow02.addComponentOrReplace(transform21)
const gltfShape12 = new GLTFShape("models/Snow_02.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
snow02.addComponentOrReplace(gltfShape12)

const snowrock043 = new Entity('snowrock043')
engine.addEntity(snowrock043)
snowrock043.setParent(_scene)
snowrock043.addComponentOrReplace(gltfShape4)
const transform22 = new Transform({
  position: new Vector3(10, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.6140127182006836, 1.6140127182006836, 1.6140127182006836)
})
snowrock043.addComponentOrReplace(transform22)

const flower01 = new Entity('flower01')
engine.addEntity(flower01)
flower01.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(2.5, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower01.addComponentOrReplace(transform23)
const gltfShape13 = new GLTFShape("models/Flower_01.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
flower01.addComponentOrReplace(gltfShape13)

const flower012 = new Entity('flower012')
engine.addEntity(flower012)
flower012.setParent(_scene)
const transform24 = new Transform({
  position: new Vector3(8, 0, 12),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower012.addComponentOrReplace(transform24)
flower012.addComponentOrReplace(gltfShape13)

const plant01 = new Entity('plant01')
engine.addEntity(plant01)
plant01.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(13.5, 0, 14.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant01.addComponentOrReplace(transform25)
const gltfShape14 = new GLTFShape("models/Plant_01.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
plant01.addComponentOrReplace(gltfShape14)

const plant012 = new Entity('plant012')
engine.addEntity(plant012)
plant012.setParent(_scene)
const transform26 = new Transform({
  position: new Vector3(14, 0.25221943855285645, 4.5),
  rotation: new Quaternion(4.413921245971069e-16, 0.4820246696472168, -5.7461814861881066e-8, -0.8761577010154724),
  scale: new Vector3(1, 1, 1)
})
plant012.addComponentOrReplace(transform26)
plant012.addComponentOrReplace(gltfShape14)

const snowrock06 = new Entity('snowrock06')
engine.addEntity(snowrock06)
snowrock06.setParent(_scene)
const transform27 = new Transform({
  position: new Vector3(6.5, 0, 5.137862205505371),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 0.3201022148132324, 1)
})
snowrock06.addComponentOrReplace(transform27)
const gltfShape15 = new GLTFShape("models/SnowRock_06.glb")
gltfShape15.withCollisions = true
gltfShape15.visible = true
snowrock06.addComponentOrReplace(gltfShape15)

const bush02 = new Entity('bush02')
engine.addEntity(bush02)
bush02.setParent(_scene)
const transform28 = new Transform({
  position: new Vector3(10, 0, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush02.addComponentOrReplace(transform28)
const gltfShape16 = new GLTFShape("models/Bush_02.glb")
gltfShape16.withCollisions = true
gltfShape16.visible = true
bush02.addComponentOrReplace(gltfShape16)

const bush03 = new Entity('bush03')
engine.addEntity(bush03)
bush03.setParent(_scene)
const transform29 = new Transform({
  position: new Vector3(2, 0, 10),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush03.addComponentOrReplace(transform29)
const gltfShape17 = new GLTFShape("models/Bush_03.glb")
gltfShape17.withCollisions = true
gltfShape17.visible = true
bush03.addComponentOrReplace(gltfShape17)
