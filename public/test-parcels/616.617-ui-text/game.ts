import { UICanvas, UIText, Color4 } from 'decentraland-ecs/src'

const ui = new UICanvas()

const text = new UIText(ui)
text.name = 'testUIText'
text.paddingLeft = 10
text.paddingRight = 10
text.paddingTop = 10
text.paddingBottom = 10
text.isPointerBlocker = true
text.outlineWidth = 0.1
text.outlineColor = Color4.Green()
text.color = Color4.Red()
text.fontSize = 100
text.fontWeight = 'normal'
text.opacity = 1
text.value = 'Hello World!'
text.lineSpacing = 0
text.lineCount = 0
text.adaptWidth = true
text.adaptHeight = true
text.textWrapping = false
text.shadowBlur = 0
text.shadowOffsetX = 0
text.shadowOffsetY = 0
text.shadowColor = Color4.Black()
text.hAlign = 'center'
text.vAlign = 'center'
text.hTextAlign = 'center'
text.vTextAlign = 'center'
text.width = 300
text.height = 200
text.positionX = 0
text.positionY = 0
text.visible = true
