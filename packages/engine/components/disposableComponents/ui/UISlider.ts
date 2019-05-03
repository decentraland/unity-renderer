import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { UIValue } from 'decentraland-ecs/src/ecs/UIValue'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UIScrollRect } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'
import { IEvents } from 'decentraland-ecs/src/decentraland/Types'
import { Color3 } from 'babylonjs'

const schemaValidator = createSchemaValidator({
  id: { type: 'string', default: null },
  visible: { type: 'boolean', default: true },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  zIndex: { type: 'number', default: 0 },
  positionX: { type: 'uiValue', default: new UIValue(0) },
  positionY: { type: 'uiValue', default: new UIValue(0) },
  width: { type: 'number', default: 100 },
  height: { type: 'number', default: 20 },
  isPointerBlocker: { type: 'boolean', default: false },

  color: { type: 'color', default: Color3.White() },
  opacity: { type: 'number', default: 1.0 },

  paddingTop: { type: 'number', default: 0 },
  paddingRight: { type: 'number', default: 0 },
  paddingBottom: { type: 'number', default: 0 },
  paddingLeft: { type: 'number', default: 0 },

  minimum: { type: 'number', default: 0 },
  maximum: { type: 'number', default: 1 },
  value: { type: 'number', default: 0 },
  borderColor: { type: 'color', default: Color3.White() },
  background: { type: 'color', default: Color3.Black() },
  barOffset: { type: 'number', default: 5 },
  thumbWidth: { type: 'number', default: 30 },
  isThumbCircle: { type: 'boolean', default: false },
  isThumbClamped: { type: 'boolean', default: false },
  isVertical: { type: 'boolean', default: false },
  swapOrientation: { type: 'boolean', default: false }
})

class UISlider extends UIControl<UIScrollRect, BABYLON.GUI.Slider> {
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

  async updateData(data: UIScrollRect): Promise<void> {
    this.data = schemaValidator(data)
    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.minimum = 0
    this.control.maximum = 1
    this.control.zIndex = 0
    this.control.background = this.data.backgroundColor.toHexString()
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.value = this.data.isHorizontal ? this.data.valueX : this.data.valueY
    this.control.borderColor = this.data.borderColor.toHexString()
    this.control.barOffset = 5
    this.control.thumbWidth = 10
    this.control.isThumbCircle = true
    this.control.isThumbClamped = true
    this.control.isVertical = true
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.paddingLeft = this.data.paddingLeft
    this.control.top = -this.data.positionY
    this.control.left = this.data.positionX
    this.control.isVisible = this.data.visible
    this.control.rotation = 0
    this.control.isPointerBlocker = this.data.isPointerBlocker

    if (this.data.parentComponent) {
      this.setParent(this.data.parentComponent)
    }
  }

  dispatchOnChanged = (data: IEvents['onChange']) => {
    this.entities.forEach($ => $.dispatchUUIDEvent('onChange', data))
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_SLIDER_SHAPE, UISlider)
