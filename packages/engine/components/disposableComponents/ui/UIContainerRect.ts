import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { createSchemaValidator } from '../../helpers/schemaValidator'
import { parseVerticalAlignment, parseHorizontalAlignment } from 'engine/entities/utils/parseAttrs'
import { UIControl } from './UIControl'
import { UIContainerRectShape } from 'decentraland-ecs/src/decentraland/UIShapes'

const schemaValidator = createSchemaValidator({
  opacity: { type: 'number', default: 1 },
  adaptWidth: { type: 'boolean', default: false },
  adaptHeight: { type: 'boolean', default: false },
  thickness: { type: 'number', default: 0 },
  cornerRadius: { type: 'number', default: 0 },
  width: { type: 'string', default: '100%' },
  height: { type: 'string', default: '100%' },
  top: { type: 'string', default: '0px' },
  left: { type: 'string', default: '0px' },
  color: { type: 'string', default: 'white' },
  background: { type: 'string', default: 'transparent' },
  hAlign: { type: 'string', default: 'center' },
  vAlign: { type: 'string', default: 'center' },
  visible: { type: 'boolean', default: true }
})

export class UIContainerRect extends UIControl<UIContainerRectShape, BABYLON.GUI.Rectangle> {
  control = new BABYLON.GUI.Rectangle('rect')

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

  async updateData(data: UIContainerRectShape): Promise<void> {
    this.data = schemaValidator(data)
    this.control.adaptWidthToChildren = this.data.adaptWidth
    this.control.adaptHeightToChildren = this.data.adaptHeight
    this.control.verticalAlignment = parseVerticalAlignment(this.data.vAlign)
    this.control.horizontalAlignment = parseHorizontalAlignment(this.data.hAlign)
    this.control.thickness = this.data.thickness
    this.control.cornerRadius = this.data.cornerRadius
    this.control.alpha = Math.max(0, Math.min(1, this.data.opacity))
    this.control.width = this.data.width
    this.control.height = this.data.height
    this.control.left = this.data.left
    this.control.top = this.data.top
    this.control.color = this.data.color
    this.control.background = this.data.background
    this.control.isVisible = this.data.visible

    this.data.parentComponent && this.setParent(this.data.parentComponent)
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_CONTAINER_RECT, UIContainerRect)
