import {
  UICanvas
  , UIContainerRect
  , UIImage
  , Color4, Texture
} from 'decentraland-ecs/src'

const ui = new UICanvas
  ()

const container = new UIContainerRect
  (ui)
container.name = 'testRectContainer'
container.color = Color4.Green()
container.width = '50%'
container.height = '50%'

const imageBack = new UIContainerRect
  (container)
imageBack.name = 'imageBack'
imageBack.color = Color4.Red()
imageBack.width = '128px'
imageBack.height = '128px'

const image = new UIImage
  (container, new Texture('img.png'))
image.name = 'testUIImage'
image.width = '128px'
image.height = '128px'
image.sourceWidth = 128
image.sourceHeight = 128
image.sourceTop = 0
image.sourceLeft = 0
image.paddingLeft = 10
image.paddingRight = 10
image.paddingTop = 10
image.paddingBottom = 10
image.isPointerBlocker = true
