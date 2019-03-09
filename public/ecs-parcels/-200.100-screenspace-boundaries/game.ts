const ui = new Entity()
const screenSpaceUI = new UIScreenSpaceShape()
ui.set(screenSpaceUI)
engine.addEntity(ui)

const textShape = new UITextShape(screenSpaceUI)
textShape.value = 'Click on the box to open this'
textShape.fontSize = 30
textShape.height = '25px'

const close = new Entity()
const closeShape = new UIImageShape(screenSpaceUI)
closeShape.source = './images/close-button.png'
closeShape.sourceWidth = '483px'
closeShape.sourceHeight = '148px'
closeShape.sourceTop = '0px'
closeShape.sourceLeft = '0px'
closeShape.width = '120px'
closeShape.height = '50px'
closeShape.top = '80px'
close.set(
  new OnClick(() => {
    log('clicked on the close image')
    screenSpaceUI.visible = false
  })
)
close.set(closeShape)
engine.addEntity(close)

const textTopShape = new UITextShape(screenSpaceUI)
textTopShape.value = 'Top'
textTopShape.vAlign = 'top'
textTopShape.fontSize = 30

const textBottomShape = new UITextShape(screenSpaceUI)
textBottomShape.value = 'Bottom'
textBottomShape.vAlign = 'bottom'
textBottomShape.fontSize = 30

const textLeftShape = new UITextShape(screenSpaceUI)
textLeftShape.value = 'Left'
textLeftShape.hAlign = 'left'
textLeftShape.hTextAlign = 'left'
textLeftShape.fontSize = 30

const textRightShape = new UITextShape(screenSpaceUI)
textRightShape.value = 'Right'
textRightShape.hAlign = 'right'
textRightShape.hTextAlign = 'right'
textRightShape.fontSize = 30

const cube = new Entity()
const transform = new Transform({ position: new Vector3(5, 1, 5) })
cube.set(transform)

cube.set(
  new OnClick(() => {
    log('clicked on the box')
    screenSpaceUI.visible = true
  })
)

cube.set(new BoxShape())

engine.addEntity(cube)

declare var dcl: any

dcl.onEvent((evt: any) => {
  if (evt.type === 'TEST_TRIGGER') {
    cube.get(OnClick).callback({} as any)
  }
})
