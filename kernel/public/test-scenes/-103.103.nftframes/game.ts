import {
  NFTShape,
  Entity,
  engine,
  Transform,
  Vector3,
  Color3,
  TextShape,
  PictureFrameStyle
} from 'decentraland-ecs/src'

let x = 1
let z = 1

for (let i = 0; i < 22; i++) {
  createNFT('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536', i, x, z)

  x += 2
  if (x >= 14) {
    x = 1
    z += 2
  }
}

function createNFT(scr: string, style: PictureFrameStyle, x: number, z: number) {
  const entity = new Entity()
  const shapeComponent = new NFTShape(scr, {
    style: style,
    color: new Color3(Math.random(), Math.random(), Math.random())
  })
  entity.addComponent(shapeComponent)
  entity.addComponent(
    new Transform({
      position: new Vector3(x, 1.5, z)
    })
  )

  engine.addEntity(entity)

  const nameE = new Entity()
  nameE.addComponent(new TextShape())
  nameE.addComponent(
    new Transform({
      position: new Vector3(x, 0.8, z),
      scale: new Vector3(0.15, 0.15, 1)
    })
  )
  nameE.getComponent(TextShape).value = PictureFrameStyle[style]
  engine.addEntity(nameE)
}
