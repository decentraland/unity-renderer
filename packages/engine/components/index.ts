import { Transform } from './ephemeralComponents/Transform'
import { Billboard } from './ephemeralComponents/Billboard'
import { HighlightBox } from './ephemeralComponents/HighlightBox'
import { TextShape } from './ephemeralComponents/TextShape'
import { Gizmos } from './ephemeralComponents/Gizmos'
import { AudioSource } from './ephemeralComponents/AudioSource'

import './disposableComponents/Texture'
import './disposableComponents/BasicMaterial'
import './disposableComponents/BoxShape'
import './disposableComponents/CircleShape'
import './disposableComponents/ConeShape'
import './disposableComponents/CylinderShape'
import './disposableComponents/DisposableComponent'
import './disposableComponents/GLTFShape'
import './disposableComponents/NFTShape'

import './disposableComponents/OBJShape'
import './disposableComponents/PBRMaterial'
import './disposableComponents/PlaneShape'
import './disposableComponents/SphereShape'
import './disposableComponents/AudioClip'

import './disposableComponents/ui/UIControl'
import './disposableComponents/ui/UIContainerRect'
import './disposableComponents/ui/UIContainerStack'
import './disposableComponents/ui/UIFullscreenTexture'
import './disposableComponents/ui/UIScreenSpace'
import './disposableComponents/ui/UIImage'
import './disposableComponents/ui/UIInputText'
import './disposableComponents/ui/UISlider'
import './disposableComponents/ui/UIText'
import './disposableComponents/ui/UIButton'

import { CLASS_ID } from 'decentraland-ecs/src'
import { DEBUG, PREVIEW, EDITOR } from 'config'
import { BaseComponent } from './BaseComponent'
import { ConstructorOf } from 'engine/entities/BaseEntity'
import { Animator } from './ephemeralComponents/Animator'

// We re-export it to avoid circular references from BaseEntity
export { BaseComponent } from './BaseComponent'

export const componentRegistry: Record<number, ConstructorOf<BaseComponent<any>>> = {
  [CLASS_ID.TRANSFORM]: Transform,
  [CLASS_ID.BILLBOARD]: Billboard,
  [CLASS_ID.HIGHLIGHT_ENTITY]: HighlightBox,
  [CLASS_ID.TEXT_SHAPE]: TextShape,
  [CLASS_ID.ANIMATION]: Animator,
  [CLASS_ID.AUDIO_SOURCE]: AudioSource
}

if (DEBUG || PREVIEW || EDITOR) {
  componentRegistry[CLASS_ID.GIZMOS] = Gizmos
}

export type IComponentName = keyof typeof componentRegistry
