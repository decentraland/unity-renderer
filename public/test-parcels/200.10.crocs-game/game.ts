import { Vector3, Entity, OBJShape, Transform, Quaternion, engine } from 'decentraland-ecs/src'

function makeObj(src: string, position: Vector3, scale: Vector3, rotation?: Vector3) {
  const ent = new Entity()
  const shape = new OBJShape(src)
  ent.addComponentOrReplace(shape)
  ent.addComponentOrReplace(
    new Transform({
      position,
      rotation: rotation ? Quaternion.Euler(rotation.x, rotation.y, rotation.z) : undefined,
      scale
    })
  )
  engine.addEntity(ent)
  return shape
}

makeObj('models/base.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/play_button.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/croc0.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/croc0_001.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/croc0_002.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/croc0_003.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/croc0_004.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/game_over.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3)).visible = false
makeObj('models/instructions.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/notice.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator_001.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator_002.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator_003.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator_004.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/separator_005.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/subtitle.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/side.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/side2.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
makeObj('models/title.obj', new Vector3(5, 0, 5), new Vector3(0.3, 0.3, 0.3))
