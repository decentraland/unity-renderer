import { engine, Entity, Transform, Vector3, TextShape } from 'decentraland-ecs'

const root = new Entity()
engine.addEntity(root)
root.getComponentOrCreate(Transform).scale.setAll(1.6)

function createText(position: Vector3, text: string, params: any) {
  const ent = new Entity()
  const shape = new TextShape(text)
  ent.addComponentOrReplace(shape)
  Object['assign'](shape, params)
  ent.addComponentOrReplace(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  ent.setParent(root)
}

createText(new Vector3(7, 2, 8.01), 'Hello world!', { color: '#000000', fontSize: 70 })
createText(new Vector3(5, 1, 5), 'Hello world!', { color: '#00cc00', outlineWidth: 10, vAlign: 'top' })
createText(new Vector3(5, 2.95, 5), 'Hello world!', { color: '#cc00cc' })
createText(new Vector3(5, 3.1, 2), 'Hello world!', {
  color: 'rgb(10, 173, 34)',
  scale: 2,
  outlineWidth: 10,
  vAlign: 'bottom'
})
createText(new Vector3(5, 3, 0), 'Hello world5!', { color: '#ff0000', fontFamily: 'Times New Roman' })
createText(new Vector3(5, 2.65, 2.5), 'Hello world!', { color: '#000000' })
createText(new Vector3(5, 2.9, 6), '你好，世界!', { color: '#000000', fontFamily: 'Helvetica' })
createText(new Vector3(5, 5, 5), '안녕하세요! 안녕하세요!', {
  color: 'red',
  fontSize: 70,
  outlineWidth: 3,
  hAlign: 'center'
})
createText(new Vector3(3, 3, 2), 'こんにちは世界', { color: '#000000' })
createText(new Vector3(3, 3.4, 2), 'Hello world! new line with whitespace! aksnfvkjanvkjansjvnkan', {
  color: 'cyan',
  hAlign: 'right',
  vAlign: 'center',
  fontSize: 30,
  width: 5,
  height: 5,
  textWrapping: true
})
createText(
  new Vector3(5, 2.1, 5),
  'Hello world! Hello world! Hello world! Hello world! Hello world! Hello world! Hello world!',
  { textWrapping: true, color: '#7f08ce', scale: 0.7 }
)
createText(new Vector3(5, 3, 2), 'Hello world!', { color: '#000000', scale: 3, opacity: 0.2, hAlign: 'left' })
