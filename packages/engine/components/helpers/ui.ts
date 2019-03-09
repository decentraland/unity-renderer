import { BaseEntity } from 'engine/entities/BaseEntity'
import { CLASS_ID } from 'decentraland-ecs/src'
import { DisposableComponent } from 'engine/components/disposableComponents/DisposableComponent'

export function isScreenSpaceComponent(component: any) {
  const ctor = DisposableComponent.constructors.get(CLASS_ID.UI_SCREEN_SPACE_SHAPE)
  if (!ctor) return false
  return component instanceof ctor
}

export function hasScreenSpaceComponent(entity: BaseEntity) {
  if (entity.disposableComponents.size > 0) {
    const componentShape = entity.disposableComponents.get('shape')
    return componentShape && isScreenSpaceComponent(componentShape)
  }
  return false
}

export function getScreenSpaceComponent(entity: BaseEntity) {
  if (hasScreenSpaceComponent(entity)) {
    return entity.disposableComponents.get('shape')
  }
  return null
}
