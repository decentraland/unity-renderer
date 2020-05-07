import { ObservableComponent, DisposableComponent, getComponentId } from '../ecs/Component'
import { CLASS_ID, OnUUIDEvent, Texture, Font } from './Components'
import { Color4 } from './math'
import { OnTextSubmit, OnBlur, OnChanged, OnClick, OnFocus } from './UIEvents'
/**
 * @public
 */
export abstract class UIShape extends ObservableComponent {
  /**
   * Defines if the entity and its children should be rendered
   */
  @ObservableComponent.field
  name: string | null = null

  @ObservableComponent.field
  visible: boolean = true

  @ObservableComponent.field
  opacity: number = 1

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.uiValue
  width: string | number = '100px'

  @ObservableComponent.uiValue
  height: string | number = '50px'

  @ObservableComponent.uiValue
  positionX: string | number = '0px'

  @ObservableComponent.uiValue
  positionY: string | number = '0px'

  @ObservableComponent.field
  isPointerBlocker: boolean = true

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
 * @internal
 * NOTE(Brian): this should be deprecated
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_FULLSCREEN_SHAPE)
export class UIFullScreen extends UIShape {
  constructor() {
    super(null)
  }
}

/**
 * @internal
 * NOTE(Brian): this should be deprecated
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_WORLD_SPACE_SHAPE)
export class UIWorldSpace extends UIShape {
  constructor() {
    super(null)
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_SCREEN_SPACE_SHAPE)
export class UICanvas extends UIShape {
  constructor() {
    super(null)
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_CONTAINER_RECT)
export class UIContainerRect extends UIShape {
  @ObservableComponent.field
  adaptWidth: boolean = false

  @ObservableComponent.field
  adaptHeight: boolean = false

  @ObservableComponent.field
  thickness: number = 0

  @ObservableComponent.field
  color: Color4 = Color4.Clear()

  @ObservableComponent.field
  alignmentUsesSize: boolean = true
}

/**
 * @public
 */
export enum UIStackOrientation {
  VERTICAL,
  HORIZONTAL
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_CONTAINER_STACK)
export class UIContainerStack extends UIShape {
  @ObservableComponent.field
  adaptWidth: boolean = true

  @ObservableComponent.field
  adaptHeight: boolean = true

  @ObservableComponent.field
  color: Color4 = Color4.Clear()

  @ObservableComponent.field
  stackOrientation: UIStackOrientation = UIStackOrientation.VERTICAL

  @ObservableComponent.field
  spacing: Number = 0
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_BUTTON_SHAPE)
export class UIButton extends UIShape {
  @ObservableComponent.field
  fontSize: number = 10

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.field
  thickness: number = 0

  @ObservableComponent.field
  cornerRadius: number = 0

  @ObservableComponent.field
  color: Color4 = Color4.White()

  @ObservableComponent.field
  background: Color4 = Color4.White()

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: Color4 = Color4.Black()

  @ObservableComponent.field
  text: string = 'button'
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_TEXT_SHAPE)
export class UIText extends UIShape {
  @ObservableComponent.field
  outlineWidth: number = 0

  @ObservableComponent.field
  outlineColor: Color4 = Color4.White()

  @ObservableComponent.field
  color: Color4 = Color4.White()

  @ObservableComponent.field
  fontSize: number = 10

  @ObservableComponent.field
  fontAutoSize: boolean = false

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.component
  font?: Font

  @ObservableComponent.field
  value: string = ''

  @ObservableComponent.field
  lineSpacing: number = 0

  @ObservableComponent.field
  lineCount: number = 0

  @ObservableComponent.field
  adaptWidth: boolean = false

  @ObservableComponent.field
  adaptHeight: boolean = false

  @ObservableComponent.field
  textWrapping: boolean = false

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: Color4 = Color4.Black()

  @ObservableComponent.field
  hTextAlign: string = 'left'

  @ObservableComponent.field
  vTextAlign: string = 'bottom'

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_INPUT_TEXT_SHAPE)
export class UIInputText extends UIShape {
  @ObservableComponent.field
  outlineWidth: number = 0

  @ObservableComponent.field
  outlineColor: Color4 = Color4.Black()

  @ObservableComponent.field
  color: Color4 = Color4.Clear()

  @ObservableComponent.field
  thickness: number = 1

  @ObservableComponent.field
  fontSize: number = 10

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.component
  font?: Font

  @ObservableComponent.field
  value: string = ''

  @ObservableComponent.field
  placeholderColor: Color4 = Color4.White()

  @ObservableComponent.field
  placeholder: string = ''

  @ObservableComponent.field
  margin: number = 10

  @ObservableComponent.field
  maxWidth: number = 100

  @ObservableComponent.field
  hTextAlign: string = 'left'

  @ObservableComponent.field
  vTextAlign: string = 'bottom'

  @ObservableComponent.field
  autoStretchWidth: boolean = true

  @ObservableComponent.field
  background: Color4 = Color4.Black()

  @ObservableComponent.field
  focusedBackground: Color4 = Color4.Black()

  @ObservableComponent.field
  textWrapping: boolean = false

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: Color4 = Color4.White()

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0

  @OnUUIDEvent.uuidEvent
  onTextSubmit: OnTextSubmit | null = null

  @OnUUIDEvent.uuidEvent
  onChanged: OnChanged | null = null

  @OnUUIDEvent.uuidEvent
  onFocus: OnFocus | null = null

  @OnUUIDEvent.uuidEvent
  onBlur: OnBlur | null = null
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_IMAGE_SHAPE)
export class UIImage extends UIShape {
  @ObservableComponent.field
  sourceLeft: number = 0

  @ObservableComponent.field
  sourceTop: number = 0

  @ObservableComponent.field
  sourceWidth: number = 1

  @ObservableComponent.field
  sourceHeight: number = 1

  @ObservableComponent.component
  source?: Texture

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0

  @ObservableComponent.field
  sizeInPixels: boolean = true

  @OnUUIDEvent.uuidEvent
  onClick: OnClick | null = null

  constructor(parent: UIShape, source: Texture) {
    super(parent)
    this.source = source
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.UI_SLIDER_SHAPE)
export class UIScrollRect extends UIShape {
  @ObservableComponent.field
  valueX: number = 0

  @ObservableComponent.field
  valueY: number = 0

  @ObservableComponent.field
  borderColor: Color4 = Color4.White()

  @ObservableComponent.field
  backgroundColor: Color4 = Color4.Clear()

  @ObservableComponent.field
  isHorizontal: boolean = false

  @ObservableComponent.field
  isVertical: boolean = false

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0

  @OnUUIDEvent.uuidEvent
  onChanged: OnChanged | null = null
}
