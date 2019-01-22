import { ObservableComponent, DisposableComponent, getComponentId } from '../ecs/Component'
import { CLASS_ID } from './Components'

/**
 * @public
 */
export abstract class UIShape extends ObservableComponent {
  /**
   * Defines if the entity and its children should be rendered
   */
  @ObservableComponent.field
  visible: boolean = true

  private _parent?: UIShape

  constructor(parent: UIShape | null) {
    super()
    if (parent) {
      this._parent = parent
      this.data.parentComponent = getComponentId(parent as any)
    }
  }

  get parent() {
    return this._parent
  }

  // @internal
  get parentComponent(): string | undefined {
    return this.data.parentComponent
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_WORLD_SPACE_SHAPE)
export class UIWorldSpaceShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  width: string = '1'

  @ObservableComponent.field
  height: string = '1'

  @ObservableComponent.field
  visible: boolean = true

  constructor() {
    super(null)
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_SCREEN_SPACE_SHAPE)
export class UIScreenSpaceShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  visible: boolean = true

  constructor() {
    super(null)
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_CONTAINER_RECT)
export class UIContainerRectShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  opacity: number = 1

  @ObservableComponent.field
  adaptWidth: boolean = false

  @ObservableComponent.field
  adaptHeight: boolean = false

  @ObservableComponent.field
  thickness: number = 0

  @ObservableComponent.field
  cornerRadius: number = 0

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '100%'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  color: string = 'white'

  @ObservableComponent.field
  background: string = 'transparent'

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  visible: boolean = true
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_CONTAINER_STACK)
export class UIContainerStackShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  opacity: number = 1

  @ObservableComponent.field
  adaptWidth: boolean = false

  @ObservableComponent.field
  adaptHeight: boolean = false

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '100%'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  color: string = 'white'

  @ObservableComponent.field
  background: string = 'transparent'

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  vertical: boolean = true

  @ObservableComponent.field
  visible: boolean = true
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_TEXT_SHAPE)
export class UITextShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  outlineWidth: number = 0

  @ObservableComponent.field
  outlineColor: string = '#fff'

  @ObservableComponent.field
  color: string = '#fff'

  @ObservableComponent.field
  fontFamily: string = 'Arial'

  @ObservableComponent.field
  fontSize: number = 100

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.field
  opacity: number = 1.0

  @ObservableComponent.field
  value: string = ''

  @ObservableComponent.field
  lineSpacing: string = '0px'

  @ObservableComponent.field
  lineCount: number = 0

  @ObservableComponent.field
  resizeToFit: boolean = false

  @ObservableComponent.field
  textWrapping: boolean = false

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: string = '#fff'

  @ObservableComponent.field
  zIndex: number = 0

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  hTextAlign: string = 'center'

  @ObservableComponent.field
  vTextAlign: string = 'center'

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '100px'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  paddingTop: string = '0px'

  @ObservableComponent.field
  paddingRight: string = '0px'

  @ObservableComponent.field
  paddingBottom: string = '0px'

  @ObservableComponent.field
  paddingLeft: string = '0px'

  @ObservableComponent.field
  visible: boolean = true
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_INPUT_TEXT_SHAPE)
export class UIInputTextShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  color: string = '#fff'

  @ObservableComponent.field
  thickness: number = 1

  @ObservableComponent.field
  fontFamily: string = 'Arial'

  @ObservableComponent.field
  fontSize: number = 100

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.field
  opacity: number = 1.0

  @ObservableComponent.field
  value: string = ''

  @ObservableComponent.field
  placeholderColor: string = '#fff'

  @ObservableComponent.field
  placeholder: string = ''

  @ObservableComponent.field
  margin: string = '10px'

  @ObservableComponent.field
  maxWidth: string = '100%'

  @ObservableComponent.field
  autoStretchWidth: boolean = true

  @ObservableComponent.field
  background: string = 'black'

  @ObservableComponent.field
  focusedBackground: string = 'black'

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: string = '#fff'

  @ObservableComponent.field
  zIndex: number = 0

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '50px'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  paddingTop: string = '0px'

  @ObservableComponent.field
  paddingRight: string = '0px'

  @ObservableComponent.field
  paddingBottom: string = '0px'

  @ObservableComponent.field
  paddingLeft: string = '0px'

  @ObservableComponent.field
  visible: boolean = true

  @ObservableComponent.field
  isPointerBlocker: boolean = false
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_IMAGE_SHAPE)
export class UIImageShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  opacity: number = 1

  @ObservableComponent.field
  sourceLeft: string | null = null

  @ObservableComponent.field
  sourceTop: string | null = null

  @ObservableComponent.field
  sourceWidth: string | null = null

  @ObservableComponent.field
  sourceHeight: string | null = null

  @ObservableComponent.field
  source: string | null = null

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '100%'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  paddingTop: string = '0px'

  @ObservableComponent.field
  paddingRight: string = '0px'

  @ObservableComponent.field
  paddingBottom: string = '0px'

  @ObservableComponent.field
  paddingLeft: string = '0px'

  @ObservableComponent.field
  visible: boolean = true

  @ObservableComponent.field
  isPointerBlocker: boolean = false
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_SLIDER_SHAPE)
export class UISliderShape extends UIShape {
  @ObservableComponent.field
  id: string | null = null

  @ObservableComponent.field
  minimum: number = 0

  @ObservableComponent.field
  maximum: number = 1

  @ObservableComponent.field
  color: string = '#fff'

  @ObservableComponent.field
  opacity: number = 1.0

  @ObservableComponent.field
  value: number = 0

  @ObservableComponent.field
  borderColor: string = '#fff'

  @ObservableComponent.field
  background: string = 'black'

  @ObservableComponent.field
  barOffset: string = '5px'

  @ObservableComponent.field
  thumbWidth: string = '30px'

  @ObservableComponent.field
  isThumbCircle: boolean = false

  @ObservableComponent.field
  isThumbClamped: boolean = false

  @ObservableComponent.field
  isVertical: boolean = false

  @ObservableComponent.field
  visible: boolean = true

  @ObservableComponent.field
  zIndex: number = 0

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  width: string = '100%'

  @ObservableComponent.field
  height: string = '20px'

  @ObservableComponent.field
  top: string = '0px'

  @ObservableComponent.field
  left: string = '0px'

  @ObservableComponent.field
  paddingTop: string = '0px'

  @ObservableComponent.field
  paddingRight: string = '0px'

  @ObservableComponent.field
  paddingBottom: string = '0px'

  @ObservableComponent.field
  paddingLeft: string = '0px'

  @ObservableComponent.field
  onChanged: string = ''

  @ObservableComponent.field
  swapOrientation: boolean = false

  @ObservableComponent.field
  isPointerBlocker: boolean = false
}
