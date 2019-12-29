
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
  position: new Vector3(12.5, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree01.addComponentOrReplace(transform3)
const gltfShape2 = new GLTFShape("models/SnowTree_01.glb")
gltfShape2.withCollisions = true
gltfShape2.visible = true
snowtree01.addComponentOrReplace(gltfShape2)

const snowtree02 = new Entity('snowtree02')
engine.addEntity(snowtree02)
snowtree02.setParent(_scene)
const transform4 = new Transform({
  position: new Vector3(6, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree02.addComponentOrReplace(transform4)
const gltfShape3 = new GLTFShape("models/SnowTree_02.glb")
gltfShape3.withCollisions = true
gltfShape3.visible = true
snowtree02.addComponentOrReplace(gltfShape3)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform5 = new Transform({
  position: new Vector3(3, 0, 6),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree03.addComponentOrReplace(transform5)
const gltfShape4 = new GLTFShape("models/SnowTree_03.glb")
gltfShape4.withCollisions = true
gltfShape4.visible = true
snowtree03.addComponentOrReplace(gltfShape4)

const deer = new Entity('deer')
engine.addEntity(deer)
deer.setParent(_scene)
const transform6 = new Transform({
  position: new Vector3(11.525484085083008, 0.2866312265396118, 10.192675590515137),
  rotation: new Quaternion(-2.220446049250313e-16, -0.3826834559440613, 4.561941935321556e-8, 0.9238795638084412),
  scale: new Vector3(0.7785987854003906, 0.7785987854003906, 0.7785987854003906)
})
deer.addComponentOrReplace(transform6)
const gltfShape5 = new GLTFShape("models/Deer.glb")
gltfShape5.withCollisions = true
gltfShape5.visible = true
deer.addComponentOrReplace(gltfShape5)

const snowrock04 = new Entity('snowrock04')
engine.addEntity(snowrock04)
snowrock04.setParent(_scene)
const transform7 = new Transform({
  position: new Vector3(3.5, 0, 9),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock04.addComponentOrReplace(transform7)
const gltfShape6 = new GLTFShape("models/SnowRock_04.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
snowrock04.addComponentOrReplace(gltfShape6)

const snowlog01 = new Entity('snowlog01')
engine.addEntity(snowlog01)
snowlog01.setParent(_scene)
const transform8 = new Transform({
  position: new Vector3(5, 0, 4),
  rotation: new Quaternion(7.637795347065533e-17, 0.34715867042541504, -4.138453491009386e-8, 0.9378064870834351),
  scale: new Vector3(1, 1, 1)
})
snowlog01.addComponentOrReplace(transform8)
const gltfShape7 = new GLTFShape("models/SnowLog_01.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snowlog01.addComponentOrReplace(gltfShape7)

const snowrock042 = new Entity('snowrock042')
engine.addEntity(snowrock042)
snowrock042.setParent(_scene)
const transform9 = new Transform({
  position: new Vector3(10.5, 0, 12.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock042.addComponentOrReplace(transform9)
snowrock042.addComponentOrReplace(gltfShape6)

const floorSnow = new Entity('floorSnow')
engine.addEntity(floorSnow)
floorSnow.setParent(_scene)
const transform10 = new Transform({
  position: new Vector3(8, 0.0005483627319335938, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
floorSnow.addComponentOrReplace(transform10)
const gltfShape8 = new GLTFShape("models/Floor_Snow.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
floorSnow.addComponentOrReplace(gltfShape8)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(12, 0.3062257766723633, 4.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock01.addComponentOrReplace(transform11)
const gltfShape9 = new GLTFShape("models/SnowRock_01.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
snowrock01.addComponentOrReplace(gltfShape9)

const snowrock032 = new Entity('snowrock032')
engine.addEntity(snowrock032)
snowrock032.setParent(_scene)
const transform12 = new Transform({
  position: new Vector3(7.5, 0, 7.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock032.addComponentOrReplace(transform12)
const gltfShape10 = new GLTFShape("models/SnowRock_03.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
snowrock032.addComponentOrReplace(gltfShape10)

const snowtree032 = new Entity('snowtree032')
engine.addEntity(snowtree032)
snowtree032.setParent(_scene)
snowtree032.addComponentOrReplace(gltfShape4)
const transform13 = new Transform({
  position: new Vector3(4, 0, 10.5),
  rotation: new Quaternion(3.6312916547678615e-16, -0.7758766412734985, 9.24916889744054e-8, 0.6308847069740295),
  scale: new Vector3(1.1859164237976074, 1.1859164237976074, 1.1859164237976074)
})
snowtree032.addComponentOrReplace(transform13)

const snowtree022 = new Entity('snowtree022')
engine.addEntity(snowtree022)
snowtree022.setParent(_scene)
snowtree022.addComponentOrReplace(gltfShape3)
const transform14 = new Transform({
  position: new Vector3(13.709029197692871, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree022.addComponentOrReplace(transform14)

const snowlog02 = new Entity('snowlog02')
engine.addEntity(snowlog02)
snowlog02.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(8.882978439331055, 0.05103778839111328, 13.335166931152344),
  rotation: new Quaternion(2.202614285174936e-16, -0.23055486381053925, 2.748427796461783e-8, 0.973059356212616),
  scale: new Vector3(1, 1, 1)
})
snowlog02.addComponentOrReplace(transform15)
const gltfShape11 = new GLTFShape("models/SnowLog_02.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
snowlog02.addComponentOrReplace(gltfShape11)

const grassSprout = new Entity('grassSprout')
engine.addEntity(grassSprout)
grassSprout.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(8.5, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout.addComponentOrReplace(transform16)
const gltfShape12 = new GLTFShape("models/Grass_03/Grass_03.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
grassSprout.addComponentOrReplace(gltfShape12)

const grassSprout2 = new Entity('grassSprout2')
engine.addEntity(grassSprout2)
grassSprout2.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(2.5, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout2.addComponentOrReplace(transform17)
grassSprout2.addComponentOrReplace(gltfShape12)

const grassSprout3 = new Entity('grassSprout3')
engine.addEntity(grassSprout3)
grassSprout3.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(14, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout3.addComponentOrReplace(transform18)
grassSprout3.addComponentOrReplace(gltfShape12)

const grassSprout4 = new Entity('grassSprout4')
engine.addEntity(grassSprout4)
grassSprout4.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(2, 0, 7.909650802612305),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout4.addComponentOrReplace(transform19)
grassSprout4.addComponentOrReplace(gltfShape12)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(2, 0, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout5.addComponentOrReplace(transform20)
grassSprout5.addComponentOrReplace(gltfShape12)

const grassSprout6 = new Entity('grassSprout6')
engine.addEntity(grassSprout6)
grassSprout6.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(14, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform21)
grassSprout6.addComponentOrReplace(gltfShape12)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform22 = new Transform({
  position: new Vector3(13, 0, 8),
  rotation: new Quaternion(2.032753924292364e-15, -0.7190765738487244, 8.57205932902616e-8, 0.6949308514595032),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts.addComponentOrReplace(transform22)
const gltfShape13 = new GLTFShape("models/Plant_03/Plant_03.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
flowerSprouts.addComponentOrReplace(gltfShape13)

const flowerSprouts2 = new Entity('flowerSprouts2')
engine.addEntity(flowerSprouts2)
flowerSprouts2.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(7.5, 0.4789273738861084, 5),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform23)
flowerSprouts2.addComponentOrReplace(gltfShape13)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform24 = new Transform({
  position: new Vector3(4, 0, 9.028438568115234),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform24)
flowerSprouts3.addComponentOrReplace(gltfShape13)

const snow01 = new Entity('snow01')
engine.addEntity(snow01)
snow01.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(7.5, 0, 6.9416399002075195),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.1069202423095703, 0.4091476798057556, 2.1069202423095703)
})
snow01.addComponentOrReplace(transform25)
const gltfShape14 = new GLTFShape("models/Snow_01.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
snow01.addComponentOrReplace(gltfShape14)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform26 = new Transform({
  position: new Vector3(3.53665828704834, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.3168864250183105, 0.311171293258667, 2.3168864250183105)
})
snow02.addComponentOrReplace(transform26)
const gltfShape15 = new GLTFShape("models/Snow_02.glb")
gltfShape15.withCollisions = true
gltfShape15.visible = true
snow02.addComponentOrReplace(gltfShape15)
