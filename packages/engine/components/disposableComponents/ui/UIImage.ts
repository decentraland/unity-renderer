import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UIImageShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'

const schemaValidator = createSchemaValidator({
  id: { type: 'string', default: null },
  opacity: { type: 'number', default: 1 },
  sourceLeft: { type: 'string', default: null },
  sourceTop: { type: 'string', default: null },
  sourceWidth: { type: 'string', default: null },
  sourceHeight: { type: 'string', default: null },
  source: { type: 'string', default: null },
  width: { type: 'string', default: '100%' },
  height: { type: 'string', default: '100%' },
  top: { type: 'string', default: '0px' },
  left: { type: 'string', default: '0px' },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  paddingTop: { type: 'string', default: '0px' },
  paddingRight: { type: 'string', default: '0px' },
  paddingBottom: { type: 'string', default: '0px' },
  paddingLeft: { type: 'string', default: '0px' },
  visible: { type: 'boolean', default: true },
  isPointerBlocker: { type: 'boolean', default: false }
})

class Class extends UIControl<UIImageShape, BABYLON.GUI.Image> {
  control = new BABYLON.GUI.Image('image', '')

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)

    this.control.onPointerUpObservable.add($ => {
      this.dispatchOnClick($.buttonIndex)
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
      this.control.dispose()
      delete this.control
    }

    super.dispose()
  }

  async updateData(data: UIImageShape): Promise<void> {
    this.data = schemaValidator(data)

    this.control.sourceLeft = parseInt(this.data.sourceLeft, 10)
    this.control.sourceTop = parseInt(this.data.sourceTop, 10)
    this.control.sourceWidth = parseInt(this.data.sourceWidth, 10)
    this.control.sourceHeight = parseInt(this.data.sourceHeight, 10)
    this.control.source = this.data.source
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.top = this.data.top
    this.control.left = this.data.left
    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    // missing this.uiEntity.fontWeight = this.data.fontWeight
    this.control.paddingTop = this.data.paddingTop
    this.control.paddingRight = this.data.paddingRight
    this.control.paddingBottom = this.data.paddingBottom
    this.control.isVisible = this.data.visible
    this.control.isPointerBlocker = this.data.isPointerBlocker

    this.setParent(this.data.parentComponent)
  }

  dispatchOnClick = (pointerId: number) => {
    this.entities.forEach($ =>
      $.dispatchUUIDEvent('onClick', {
        entityId: $.uuid
      })
    )
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_IMAGE_SHAPE, Class)
