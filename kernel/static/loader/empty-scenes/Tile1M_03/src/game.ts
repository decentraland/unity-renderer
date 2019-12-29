
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

const snowrock04 = new Entity('snowrock04')
engine.addEntity(snowrock04)
snowrock04.setParent(_scene)
const transform4 = new Transform({
  position: new Vector3(3.5, 0, 9),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock04.addComponentOrReplace(transform4)
const gltfShape3 = new GLTFShape("models/SnowRock_04.glb")
gltfShape3.withCollisions = true
gltfShape3.visible = true
snowrock04.addComponentOrReplace(gltfShape3)

const snowlog01 = new Entity('snowlog01')
engine.addEntity(snowlog01)
snowlog01.setParent(_scene)
const transform5 = new Transform({
  position: new Vector3(12, 0, 4),
  rotation: new Quaternion(7.637795347065533e-17, 0.34715867042541504, -4.138453491009386e-8, 0.9378064870834351),
  scale: new Vector3(1, 1, 1)
})
snowlog01.addComponentOrReplace(transform5)
const gltfShape4 = new GLTFShape("models/SnowLog_01.glb")
gltfShape4.withCollisions = true
gltfShape4.visible = true
snowlog01.addComponentOrReplace(gltfShape4)

const snowrock042 = new Entity('snowrock042')
engine.addEntity(snowrock042)
snowrock042.setParent(_scene)
const transform6 = new Transform({
  position: new Vector3(4.610942363739014, 0, 12.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock042.addComponentOrReplace(transform6)
snowrock042.addComponentOrReplace(gltfShape3)

const floorSnow = new Entity('floorSnow')
engine.addEntity(floorSnow)
floorSnow.setParent(_scene)
const transform7 = new Transform({
  position: new Vector3(8, 0.0005483627319335938, 8),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
floorSnow.addComponentOrReplace(transform7)
const gltfShape5 = new GLTFShape("models/Floor_Snow.glb")
gltfShape5.withCollisions = true
gltfShape5.visible = true
floorSnow.addComponentOrReplace(gltfShape5)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform8 = new Transform({
  position: new Vector3(14, 0.3062257766723633, 3.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowrock01.addComponentOrReplace(transform8)
const gltfShape6 = new GLTFShape("models/SnowRock_01.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
snowrock01.addComponentOrReplace(gltfShape6)

const snowtree032 = new Entity('snowtree032')
engine.addEntity(snowtree032)
snowtree032.setParent(_scene)
const gltfShape7 = new GLTFShape("models/SnowTree_03.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snowtree032.addComponentOrReplace(gltfShape7)
const transform9 = new Transform({
  position: new Vector3(4, 0, 4.5),
  rotation: new Quaternion(3.6312916547678615e-16, -0.7758766412734985, 9.24916889744054e-8, 0.6308847069740295),
  scale: new Vector3(1.2288727760314941, 1.2288727760314941, 1.2288727760314941)
})
snowtree032.addComponentOrReplace(transform9)

const snowtree022 = new Entity('snowtree022')
engine.addEntity(snowtree022)
snowtree022.setParent(_scene)
const gltfShape8 = new GLTFShape("models/SnowTree_02.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
snowtree022.addComponentOrReplace(gltfShape8)
const transform10 = new Transform({
  position: new Vector3(4, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree022.addComponentOrReplace(transform10)

const snowlog02 = new Entity('snowlog02')
engine.addEntity(snowlog02)
snowlog02.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(5.411953926086426, 0.05103778839111328, 8.493051528930664),
  rotation: new Quaternion(2.202614285174936e-16, -0.23055486381053925, 2.748427796461783e-8, 0.973059356212616),
  scale: new Vector3(1, 1, 1)
})
snowlog02.addComponentOrReplace(transform11)
const gltfShape9 = new GLTFShape("models/SnowLog_02.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
snowlog02.addComponentOrReplace(gltfShape9)

const grassSprout = new Entity('grassSprout')
engine.addEntity(grassSprout)
grassSprout.setParent(_scene)
const transform12 = new Transform({
  position: new Vector3(7, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout.addComponentOrReplace(transform12)
const gltfShape10 = new GLTFShape("models/Grass_03/Grass_03.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
grassSprout.addComponentOrReplace(gltfShape10)

const grassSprout2 = new Entity('grassSprout2')
engine.addEntity(grassSprout2)
grassSprout2.setParent(_scene)
const transform13 = new Transform({
  position: new Vector3(5.5, 0, 5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout2.addComponentOrReplace(transform13)
grassSprout2.addComponentOrReplace(gltfShape10)

const grassSprout3 = new Entity('grassSprout3')
engine.addEntity(grassSprout3)
grassSprout3.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(9.727373123168945, 0, 9.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout3.addComponentOrReplace(transform14)
grassSprout3.addComponentOrReplace(gltfShape10)

const grassSprout4 = new Entity('grassSprout4')
engine.addEntity(grassSprout4)
grassSprout4.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(1, 0, 14.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout4.addComponentOrReplace(transform15)
grassSprout4.addComponentOrReplace(gltfShape10)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(1, 0, 2),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout5.addComponentOrReplace(transform16)
grassSprout5.addComponentOrReplace(gltfShape10)

const grassSprout6 = new Entity('grassSprout6')
engine.addEntity(grassSprout6)
grassSprout6.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(14, 0, 1.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform17)
grassSprout6.addComponentOrReplace(gltfShape10)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(14.5, 0, 11),
  rotation: new Quaternion(2.032753924292364e-15, -0.7190765738487244, 8.57205932902616e-8, 0.6949308514595032),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts.addComponentOrReplace(transform18)
const gltfShape11 = new GLTFShape("models/Plant_03/Plant_03.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
flowerSprouts.addComponentOrReplace(gltfShape11)

const flowerSprouts2 = new Entity('flowerSprouts2')
engine.addEntity(flowerSprouts2)
flowerSprouts2.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(11, 0.4789273738861084, 5.5),
  rotation: new Quaternion(-5.119466286172044e-15, -0.9998422861099243, 1.1919047437913832e-7, 0.01776476390659809),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts2.addComponentOrReplace(transform19)
flowerSprouts2.addComponentOrReplace(gltfShape11)

const flowerSprouts3 = new Entity('flowerSprouts3')
engine.addEntity(flowerSprouts3)
flowerSprouts3.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(4, 0, 9.028438568115234),
  rotation: new Quaternion(6.154739918096759e-16, -0.7383295297622681, 8.801572448646766e-8, 0.6744402050971985),
  scale: new Vector3(1, 1, 1)
})
flowerSprouts3.addComponentOrReplace(transform20)
flowerSprouts3.addComponentOrReplace(gltfShape11)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(9.637752532958984, 0, 12.057339668273926),
  rotation: new Quaternion(5.840853330645957e-15, 0.9992162585258484, -1.191158531810288e-7, -0.039585329592227936),
  scale: new Vector3(3.428689479827881, 0.10282117128372192, 3.799405097961426)
})
snow02.addComponentOrReplace(transform21)
const gltfShape12 = new GLTFShape("models/Snow_02.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
snow02.addComponentOrReplace(gltfShape12)

const snow022 = new Entity('snow022')
engine.addEntity(snow022)
snow022.setParent(_scene)
const transform22 = new Transform({
  position: new Vector3(2.4853157997131348, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snow022.addComponentOrReplace(transform22)
snow022.addComponentOrReplace(gltfShape12)

const snow03 = new Entity('snow03')
engine.addEntity(snow03)
snow03.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(12.5, 0, 7),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.0359723567962646, 0.5240237712860107, 1)
})
snow03.addComponentOrReplace(transform23)
const gltfShape13 = new GLTFShape("models/Snow_03.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
snow03.addComponentOrReplace(gltfShape13)

const snowtree023 = new Entity('snowtree023')
engine.addEntity(snowtree023)
snowtree023.setParent(_scene)
snowtree023.addComponentOrReplace(gltfShape8)
const transform24 = new Transform({
  position: new Vector3(13.899871826171875, 0, 13.415389060974121),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.0969419479370117, 1.0969419479370117, 1.0969419479370117)
})
snowtree023.addComponentOrReplace(transform24)

const snowman = new Entity('snowman')
engine.addEntity(snowman)
snowman.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(8.184085845947266, 0, 11.966928482055664),
  rotation: new Quaternion(7.306219905879423e-15, -0.9903469085693359, 1.1805852295765362e-7, 0.13861127197742462),
  scale: new Vector3(0.6057720184326172, 0.6057720184326172, 0.6057720184326172)
})
snowman.addComponentOrReplace(transform25)
const gltfShape14 = new GLTFShape("models/Snowman.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
snowman.addComponentOrReplace(gltfShape14)

const bush01 = new Entity('bush01')
engine.addEntity(bush01)
bush01.setParent(_scene)
const transform26 = new Transform({
  position: new Vector3(7.5, 0, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(0.42386627197265625, 0.42386627197265625, 0.42386627197265625)
})
bush01.addComponentOrReplace(transform26)
const gltfShape15 = new GLTFShape("models/Bush_01.glb")
gltfShape15.withCollisions = true
gltfShape15.visible = true
bush01.addComponentOrReplace(gltfShape15)

const bush03 = new Entity('bush03')
engine.addEntity(bush03)
bush03.setParent(_scene)
const transform27 = new Transform({
  position: new Vector3(12.5, 0, 10),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush03.addComponentOrReplace(transform27)
const gltfShape16 = new GLTFShape("models/Bush_03.glb")
gltfShape16.withCollisions = true
gltfShape16.visible = true
bush03.addComponentOrReplace(gltfShape16)
