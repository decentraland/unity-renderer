
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
  position: new Vector3(2.5, 0, 11.5),
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
  position: new Vector3(8.5, 0.2476482391357422, 3),
  rotation: new Quaternion(-8.730236238452065e-16, -0.5679166913032532, 6.770093108343644e-8, 0.8230860829353333),
  scale: new Vector3(1, 1.3280184268951416, 1)
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
  rotation: new Quaternion(3.3281885908289946e-16, -0.3457911014556885, 4.1221504432087386e-8, 0.9383115768432617),
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
  scale: new Vector3(1, 1.6005480289459229, 1)
})
grassSprout4.addComponentOrReplace(transform8)
grassSprout4.addComponentOrReplace(gltfShape4)

const grassSprout5 = new Entity('grassSprout5')
engine.addEntity(grassSprout5)
grassSprout5.setParent(_scene)
const transform9 = new Transform({
  position: new Vector3(12.5, 0, 13),
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
  rotation: new Quaternion(-5.292598351518055e-17, -0.6455901265144348, 7.69603332173574e-8, 0.763684093952179),
  scale: new Vector3(1, 1, 1)
})
grassSprout6.addComponentOrReplace(transform10)
grassSprout6.addComponentOrReplace(gltfShape4)

const flowerSprouts = new Entity('flowerSprouts')
engine.addEntity(flowerSprouts)
flowerSprouts.setParent(_scene)
const transform11 = new Transform({
  position: new Vector3(7.346514701843262, 0, 14.072413444519043),
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
  position: new Vector3(14.10966968536377, 0, 5),
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

const snow01 = new Entity('snow01')
engine.addEntity(snow01)
snow01.setParent(_scene)
const transform14 = new Transform({
  position: new Vector3(7, 0, 7),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.1069202423095703, 0.25070422887802124, 3.9693355560302734)
})
snow01.addComponentOrReplace(transform14)
const gltfShape6 = new GLTFShape("models/Snow_01.glb")
gltfShape6.withCollisions = true
gltfShape6.visible = true
snow01.addComponentOrReplace(gltfShape6)

const snow02 = new Entity('snow02')
engine.addEntity(snow02)
snow02.setParent(_scene)
const transform15 = new Transform({
  position: new Vector3(12, 0, 13),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(2.3168864250183105, 0.311171293258667, 2.3168864250183105)
})
snow02.addComponentOrReplace(transform15)
const gltfShape7 = new GLTFShape("models/Snow_02.glb")
gltfShape7.withCollisions = true
gltfShape7.visible = true
snow02.addComponentOrReplace(gltfShape7)

const snowrock01 = new Entity('snowrock01')
engine.addEntity(snowrock01)
snowrock01.setParent(_scene)
const transform16 = new Transform({
  position: new Vector3(10, 0, 6.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1.5, 1.5, 1.5)
})
snowrock01.addComponentOrReplace(transform16)
const gltfShape8 = new GLTFShape("models/SnowRock_01.glb")
gltfShape8.withCollisions = true
gltfShape8.visible = true
snowrock01.addComponentOrReplace(gltfShape8)

const plant01 = new Entity('plant01')
engine.addEntity(plant01)
plant01.setParent(_scene)
const transform17 = new Transform({
  position: new Vector3(3.5, 0, 13.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant01.addComponentOrReplace(transform17)
const gltfShape9 = new GLTFShape("models/Plant_01.glb")
gltfShape9.withCollisions = true
gltfShape9.visible = true
plant01.addComponentOrReplace(gltfShape9)

const plant012 = new Entity('plant012')
engine.addEntity(plant012)
plant012.setParent(_scene)
const transform18 = new Transform({
  position: new Vector3(5.5, 0, 3),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
plant012.addComponentOrReplace(transform18)
plant012.addComponentOrReplace(gltfShape9)

const bush02 = new Entity('bush02')
engine.addEntity(bush02)
bush02.setParent(_scene)
const transform19 = new Transform({
  position: new Vector3(7, 0, 6.5),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
bush02.addComponentOrReplace(transform19)
const gltfShape10 = new GLTFShape("models/Bush_02.glb")
gltfShape10.withCollisions = true
gltfShape10.visible = true
bush02.addComponentOrReplace(gltfShape10)

const bush03 = new Entity('bush03')
engine.addEntity(bush03)
bush03.setParent(_scene)
const transform20 = new Transform({
  position: new Vector3(4.5, 0, 11.5),
  rotation: new Quaternion(1.1778919573857106e-15, -0.8205099105834961, 9.781239640460626e-8, 0.5716323852539062),
  scale: new Vector3(0.5601911544799805, 0.5601911544799805, 0.5601911544799805)
})
bush03.addComponentOrReplace(transform20)
const gltfShape11 = new GLTFShape("models/Bush_03.glb")
gltfShape11.withCollisions = true
gltfShape11.visible = true
bush03.addComponentOrReplace(gltfShape11)

const snowtree03 = new Entity('snowtree03')
engine.addEntity(snowtree03)
snowtree03.setParent(_scene)
const transform21 = new Transform({
  position: new Vector3(9.5, 0, 6),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
snowtree03.addComponentOrReplace(transform21)
const gltfShape12 = new GLTFShape("models/SnowTree_03.glb")
gltfShape12.withCollisions = true
gltfShape12.visible = true
snowtree03.addComponentOrReplace(gltfShape12)

const snowlog02 = new Entity('snowlog02')
engine.addEntity(snowlog02)
snowlog02.setParent(_scene)
const transform22 = new Transform({
  position: new Vector3(11.296821594238281, 0, 10.5),
  rotation: new Quaternion(-4.510888254283761e-16, 0.4592313766479492, -5.474464259691558e-8, 0.8883166909217834),
  scale: new Vector3(1, 1, 1)
})
snowlog02.addComponentOrReplace(transform22)
const gltfShape13 = new GLTFShape("models/SnowLog_02.glb")
gltfShape13.withCollisions = true
gltfShape13.visible = true
snowlog02.addComponentOrReplace(gltfShape13)

const snowlog01 = new Entity('snowlog01')
engine.addEntity(snowlog01)
snowlog01.setParent(_scene)
const transform23 = new Transform({
  position: new Vector3(4.5, 0, 2.5),
  rotation: new Quaternion(-2.077821715149348e-16, -0.6574377417564392, 7.83726719078004e-8, 0.7535089254379272),
  scale: new Vector3(1, 1, 1)
})
snowlog01.addComponentOrReplace(transform23)
const gltfShape14 = new GLTFShape("models/SnowLog_01.glb")
gltfShape14.withCollisions = true
gltfShape14.visible = true
snowlog01.addComponentOrReplace(gltfShape14)

const fire01 = new Entity('fire01')
engine.addEntity(fire01)
fire01.setParent(_scene)
const transform24 = new Transform({
  position: new Vector3(7, 0, 12),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
fire01.addComponentOrReplace(transform24)
const gltfShape15 = new GLTFShape("models/Fire_01.glb")
gltfShape15.withCollisions = true
gltfShape15.visible = true
fire01.addComponentOrReplace(gltfShape15)

const flower01 = new Entity('flower01')
engine.addEntity(flower01)
flower01.setParent(_scene)
const transform25 = new Transform({
  position: new Vector3(3, 0, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower01.addComponentOrReplace(transform25)
const gltfShape16 = new GLTFShape("models/Flower_01.glb")
gltfShape16.withCollisions = true
gltfShape16.visible = true
flower01.addComponentOrReplace(gltfShape16)

const flower012 = new Entity('flower012')
engine.addEntity(flower012)
flower012.setParent(_scene)
const transform26 = new Transform({
  position: new Vector3(15.101435661315918, 0, 10),
  rotation: new Quaternion(4.1047714255799906e-16, -0.5086968541145325, 6.064138347028347e-8, 0.8609457015991211),
  scale: new Vector3(1, 1, 1)
})
flower012.addComponentOrReplace(transform26)
flower012.addComponentOrReplace(gltfShape16)

const flower013 = new Entity('flower013')
engine.addEntity(flower013)
flower013.setParent(_scene)
const transform27 = new Transform({
  position: new Vector3(9.5, 0, 4),
  rotation: new Quaternion(0, 0, 0, 1),
  scale: new Vector3(1, 1, 1)
})
flower013.addComponentOrReplace(transform27)
flower013.addComponentOrReplace(gltfShape16)
