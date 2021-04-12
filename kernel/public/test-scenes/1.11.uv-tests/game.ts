import { engine, PlaneShape, Entity, Transform, BasicMaterial, Texture, Vector3, BoxShape } from 'decentraland-ecs/src'

const generateDiceUV = (faces : number, offset : number = 0) : number[] => {
  const sizeDiv = 1.0 / 6.0
  let uvs : number[] = []
  for(let i = 0; i < faces; ++i) {
    uvs = [
      ...uvs,
      sizeDiv*(i+1+offset), // lower-right corner
      0,

      sizeDiv*(i+offset), // lower-left corner
      0,

      sizeDiv*(i+offset), // upper-left corner
      1,

      sizeDiv*(i+1+offset), // upper-right corner
      1,
    ]
  }
  return uvs
}

//Create material and configure fields
const material = new BasicMaterial()
let diceTexture = new Texture("dice.png", {
  samplingMode: 0
})

material.texture = diceTexture

// Create plane shape
const planeShapeEntity = new Entity()

const plane = new PlaneShape()

plane.uvs = generateDiceUV(2)

planeShapeEntity.addComponent(
  new Transform({
    position: new Vector3(12, 1, 12),
    scale: new Vector3(1, 1, 1),
  })
)
planeShapeEntity.addComponent(plane)
planeShapeEntity.addComponent(material)

engine.addEntity(planeShapeEntity)


// Create box shape
const boxShapeEntity = new Entity()

const box = new BoxShape()

box.uvs = generateDiceUV(6)

boxShapeEntity.addComponent(
  new Transform({
    position: new Vector3(2, 1, 2),
    scale: new Vector3(1, 1, 1)
  })
)
boxShapeEntity.addComponent(box)
boxShapeEntity.addComponent(material)

engine.addEntity(boxShapeEntity)