import { Transform } from './ephemeralComponents/Transform'
import { Billboard } from './ephemeralComponents/Billboard'
import { HighlightBox } from './ephemeralComponents/HighlightBox'
import { Sound } from './ephemeralComponents/Sound'
import { TextShape } from './ephemeralComponents/TextShape'

import './disposableComponents/BasicMaterial'
import './disposableComponents/BoxShape'
import './disposableComponents/CircleShape'
import './disposableComponents/ConeShape'
import './disposableComponents/CylinderShape'
import './disposableComponents/DisposableComponent'
import './disposableComponents/GLTFShape'
import './disposableComponents/OBJShape'
import './disposableComponents/PBRMaterial'
import './disposableComponents/PlaneShape'
import './disposableComponents/SphereShape'
import './disposableComponents/UUIDComponent'

import './disposableComponents/ui/UIControl'
import './disposableComponents/ui/UIContainerRect'
import './disposableComponents/ui/UIContainerStack'
import './disposableComponents/ui/UIFullscreenTexture'
import './disposableComponents/ui/UIImage'
import './disposableComponents/ui/UIInputText'
import './disposableComponents/ui/UISlider'
import './disposableComponents/ui/UIText'

import { CLASS_ID } from 'decentraland-ecs/src'

// We re-export it to avoid circular references from BaseEntity
export { BaseComponent } from './BaseComponent'

export const componentRegistry = {
  [CLASS_ID.TRANSFORM]: Transform,
  [CLASS_ID.BILLBOARD]: Billboard,
  [CLASS_ID.HIGHLIGHT_ENTITY]: HighlightBox,
  [CLASS_ID.SOUND]: Sound,
  [CLASS_ID.TEXT_SHAPE]: TextShape
}

export type IComponentName = keyof typeof componentRegistry
