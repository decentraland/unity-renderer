const ui = new Entity()
const screenSpaceUI = new UICanvas()
ui.addComponentOrReplace(screenSpaceUI)
engine.addEntity(ui)

const textShape = new UIText(screenSpaceUI)
textShape.value = 'Click on the box to open this'
textShape.fontSize = 30
textShape.height = '25px'

const close = new Entity()
const closeShape = new UIButton(screenSpaceUI)
closeShape.text = 'Close UI'
closeShape.fontSize = 15
closeShape.color = Color4.Black()
closeShape.background = Color4.Yellow()
closeShape.cornerRadius = 10
closeShape.thickness = 1
closeShape.width = '120px'
closeShape.height = '30px'
closeShape.positionY = '80px'
close.addComponentOrReplace(
  new OnClick(() => {
    log('clicked on the close button')
    screenSpaceUI.visible = false
  })
)
close.addComponentOrReplace(closeShape)
engine.addEntity(close)

const textTopShape = new UIText(screenSpaceUI)
textTopShape.value = 'Top'
textTopShape.vAlign = 'top'
textTopShape.fontSize = 30

const textBottomShape = new UIText(screenSpaceUI)
textBottomShape.value = 'Bottom'
textBottomShape.vAlign = 'bottom'
textBottomShape.fontSize = 30

const textLeftShape = new UIText(screenSpaceUI)
textLeftShape.value = 'Left'
textLeftShape.hAlign = 'left'
textLeftShape.hTextAlign = 'left'
textLeftShape.fontSize = 30

const textRightShape = new UIText(screenSpaceUI)
textRightShape.value = 'Right'
textRightShape.hAlign = 'right'
textRightShape.hTextAlign = 'right'
textRightShape.fontSize = 30

const cube = new Entity()
const transform = new Transform({ position: new Vector3(5, 1, 5) })
cube.addComponentOrReplace(transform)

cube.addComponentOrReplace(
  new OnClick(() => {
    log('clicked on the box')
    screenSpaceUI.visible = true
  })
)

cube.addComponentOrReplace(new BoxShape())

engine.addEntity(cube)

declare var dcl: any

dcl.onEvent((evt: any) => {
  if (evt.type === 'TEST_TRIGGER') {
    cube.getComponent(OnClick).callback({} as any)
  }
})
