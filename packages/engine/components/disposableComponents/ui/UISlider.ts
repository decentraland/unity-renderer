import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UISliderShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'
import { IEvents } from 'decentraland-ecs/src/decentraland/Types'

const schemaValidator = createSchemaValidator({
  minimum: { type: 'number', default: 0 },
  maximum: { type: 'number', default: 1 },
  color: { type: 'string', default: '#fff' },
  opacity: { type: 'number', default: 1.0 },
  value: { type: 'number', default: 0 },
  borderColor: { type: 'string', default: '#fff' },
  background: { type: 'string', default: 'black' },
  barOffset: { type: 'string', default: '5px' },
  thumbWidth: { type: 'string', default: '30px' },
  isThumbCircle: { type: 'boolean', default: false },
  isThumbClamped: { type: 'boolean', default: false },
  isVertical: { type: 'boolean', default: false },
  visible: { type: 'boolean', default: true },
  zIndex: { type: 'number', default: 0 },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  width: { type: 'string', default: '100%' },
  height: { type: 'string', default: '20px' },
  top: { type: 'string', default: '0px' },
  left: { type: 'string', default: '0px' },
  paddingTop: { type: 'string', default: '0px' },
  paddingRight: { type: 'string', default: '0px' },
  paddingBottom: { type: 'string', default: '0px' },
  paddingLeft: { type: 'string', default: '0px' },
  swapOrientation: { type: 'boolean', default: false },
  isPointerBlocker: { type: 'boolean', default: false }
})

class UISlider extends UIControl<UISliderShape, BABYLON.GUI.Slider> {
  control = new BABYLON.GUI.Slider('slider')

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)

    this.control.onValueChangedObservable.add($ => {
      this.dispatchOnChanged({ pointerId: -1, value: $ })
    })
  }

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.control) {
      if (this.control.onValueChangedObservable.hasObservers()) {
        this.control.onValueChangedObservable.clear()
      }
      this.control.dispose()
      delete this.control
    }

    super.dispose()
  }

  async updateData(data: UISliderShape): Promise<void> {
    this.data = schemaValidator(data)
    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.minimum = this.data.minimum
    this.control.maximum = this.data.maximum
    this.control.color = this.data.color
    this.control.zIndex = this.data.zIndex
    this.control.background = this.data.background
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.value = this.data.value
    this.control.borderColor = this.data.borderColor
    this.control.barOffset = this.data.barOffset
    this.control.thumbWidth = this.data.thumbWidth
    this.control.isThumbCircle = this.data.isThumbCircle
    this.control.isThumbClamped = this.data.isThumbClamped
    this.control.isVertical = this.data.isVertical
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.paddingLeft = this.data.paddingLeft
    this.control.top = this.data.top
    this.control.left = this.data.left
    this.control.isVisible = this.data.visible
    this.control.rotation = 0
    this.control.isPointerBlocker = this.data.isPointerBlocker

    if (this.data.swapOrientation) {
      this.control.rotation = Math.PI
    }

    this.setParent(this.data.parentComponent)
  }

  dispatchOnChanged = (data: IEvents['onChange']) => {
    this.entities.forEach($ => $.dispatchUUIDEvent('onChange', data))
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_SLIDER_SHAPE, UISlider)
