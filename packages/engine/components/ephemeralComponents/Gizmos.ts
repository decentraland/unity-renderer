import { BaseComponent } from '../BaseComponent'
import { scene } from 'engine/renderer'
import { BaseEntity, findParentEntity } from 'engine/entities/BaseEntity'
import { Gizmo } from 'decentraland-ecs/src/decentraland/Gizmos'
import { removeEntityOutline, addEntityOutline } from './Outline'

type GizmoConfiguration = {
  position: boolean
  rotation: boolean
  scale: boolean
  cycle: boolean
  localReference: boolean
  selectedGizmo?: Gizmo
}

export const gizmoManager = new BABYLON.GizmoManager(scene)
gizmoManager.positionGizmoEnabled = true
gizmoManager.rotationGizmoEnabled = true
gizmoManager.boundingBoxGizmoEnabled = false
gizmoManager.scaleGizmoEnabled = true
gizmoManager.usePointerToAttachGizmos = false

const defaultValue: GizmoConfiguration = {
  position: true,
  rotation: true,
  scale: true,
  cycle: true,
  localReference: false
}

let activeEntity: BaseEntity | null = null
let selectedGizmo: Gizmo = Gizmo.MOVE
let currentConfiguration = defaultValue

function isSelectedGizmoValid() {
  switch (selectedGizmo) {
    case Gizmo.MOVE:
      return !!currentConfiguration.position
    case Gizmo.ROTATE:
      return !!currentConfiguration.rotation
    case Gizmo.SCALE:
      return !!currentConfiguration.scale
    case Gizmo.NONE:
      return true
  }
  return false
}

function switchGizmo() {
  let nextGizmo = selectedGizmo

  switch (nextGizmo) {
    case Gizmo.NONE:
    case Gizmo.MOVE:
      if (currentConfiguration.rotation) {
        nextGizmo = Gizmo.ROTATE
        break
      }
    // tslint:disable-next-line:no-switch-case-fall-through
    case Gizmo.ROTATE:
      if (currentConfiguration.scale) {
        nextGizmo = Gizmo.SCALE
        break
      }
    // tslint:disable-next-line:no-switch-case-fall-through
    case Gizmo.SCALE:
      if (currentConfiguration.position) {
        nextGizmo = Gizmo.MOVE
        break
      }
    // tslint:disable-next-line:no-switch-case-fall-through
    default:
      if (currentConfiguration.rotation) {
        nextGizmo = Gizmo.ROTATE
      } else {
        nextGizmo = Gizmo.NONE
      }
      break
  }

  return nextGizmo
}
const upVector = BABYLON.Vector3.Up()

const dragBehavior = new BABYLON.PointerDragBehavior({ dragPlaneNormal: upVector })
{
  // Add drag behavior to handle events when the gizmo is dragged
  dragBehavior.moveAttached = false

  let localDelta = new BABYLON.Vector3()
  let dragTarget = new BABYLON.Vector3()
  let tmpMatrix = new BABYLON.Matrix()

  dragBehavior.onDragStartObservable.add(event => {
    if (activeEntity) {
      dragTarget.copyFrom(activeEntity.position)

      activeEntity.computeWorldMatrix().invertToRef(tmpMatrix)
      tmpMatrix.setTranslationFromFloats(0, 0, 0)
      BABYLON.Vector3.TransformCoordinatesToRef(BABYLON.Vector3.Up(), tmpMatrix, upVector)
    }
  })

  dragBehavior.onDragObservable.add(event => {
    if (activeEntity) {
      // Convert delta to local translation if it has a parent
      if (activeEntity.parent) {
        activeEntity.parent.computeWorldMatrix().invertToRef(tmpMatrix)
        tmpMatrix.setTranslationFromFloats(0, 0, 0)
        BABYLON.Vector3.TransformCoordinatesToRef(event.delta, tmpMatrix, localDelta)
      } else {
        localDelta.copyFrom(event.delta)
      }

      dragTarget.addInPlace(localDelta)
      const { snapDistance } = gizmoManager.gizmos.positionGizmo!
      if (snapDistance) {
        activeEntity.position.set(
          Math.round(dragTarget.x / snapDistance) * snapDistance,
          Math.round(dragTarget.y / snapDistance) * snapDistance,
          Math.round(dragTarget.z / snapDistance) * snapDistance
        )
      } else {
        activeEntity.position.copyFrom(dragTarget)
      }
    }
  })

  dragBehavior.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion!,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })

  gizmoManager.gizmos.positionGizmo!.onDragStartObservable.add(() => {
    if (!activeEntity) return

    const { snapDistance } = gizmoManager.gizmos.positionGizmo!
    if (snapDistance) {
      activeEntity.position.x -= activeEntity.position.x % snapDistance
      activeEntity.position.y -= activeEntity.position.y % snapDistance
      activeEntity.position.z -= activeEntity.position.z % snapDistance
    }
  })

  gizmoManager.gizmos.rotationGizmo!.onDragStartObservable.add(() => {
    if (!activeEntity) return

    const { snapDistance } = gizmoManager.gizmos.rotationGizmo!
    if (snapDistance) {
      const angles = activeEntity.rotationQuaternion!.toEulerAngles()
      angles.x -= angles.x % snapDistance
      angles.y -= angles.y % snapDistance
      angles.z -= angles.z % snapDistance
      activeEntity.rotationQuaternion = BABYLON.Quaternion.RotationYawPitchRoll(angles.y, angles.x, angles.z)
    }
  })
}

{
  gizmoManager.gizmos.positionGizmo!.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion!,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })

  gizmoManager.gizmos.scaleGizmo!.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion!,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })

  gizmoManager.gizmos.rotationGizmo!.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion!,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })
}

export function selectGizmo(type: Gizmo) {
  selectedGizmo = type

  if (type === Gizmo.MOVE && currentConfiguration.position) {
    gizmoManager.gizmos.positionGizmo!.attachedMesh = activeEntity
    gizmoManager.gizmos.rotationGizmo!.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo!.attachedMesh = null
  } else if (type === Gizmo.ROTATE && currentConfiguration.rotation) {
    gizmoManager.gizmos.positionGizmo!.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo!.attachedMesh = activeEntity
    gizmoManager.gizmos.scaleGizmo!.attachedMesh = null
  } else if (type === Gizmo.SCALE && currentConfiguration.scale) {
    gizmoManager.gizmos.positionGizmo!.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo!.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo!.attachedMesh = activeEntity
  } else {
    gizmoManager.gizmos.positionGizmo!.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo!.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo!.attachedMesh = null
    selectedGizmo = Gizmo.NONE
    return false
  }
  return true
}

function selectActiveEntity(newActiveEntity: BaseEntity | null) {
  if (activeEntity === newActiveEntity) {
    return
  }

  let context = activeEntity && activeEntity.context

  if (activeEntity) {
    gizmoManager.gizmos.positionGizmo!.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo!.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo!.attachedMesh = null
    removeEntityOutline(activeEntity)
    activeEntity.removeBehavior(dragBehavior)
  }

  activeEntity = newActiveEntity

  if (newActiveEntity) {
    addEntityOutline(newActiveEntity)
    newActiveEntity.addBehavior(dragBehavior, true)
  }

  if (context && !newActiveEntity) {
    context.emit('uuidEvent', {
      uuid: 'gizmo-event',
      payload: {
        type: 'gizmoSelected',
        gizmoType: selectedGizmo,
        entityId: null
      }
    })
  }
}

scene.onPointerObservable.add(evt => {
  if (evt.type === BABYLON.PointerEventTypes.POINTERTAP) {
    let shouldDeselect = !evt.pickInfo || !evt.pickInfo.pickedMesh

    if (activeEntity && evt.pickInfo && evt.pickInfo.pickedMesh) {
      const parent = findParentEntity(evt.pickInfo.pickedMesh)

      if (!parent) {
        shouldDeselect = true
      } else {
        const hasGizmo = !!parent.getBehaviorByName('gizmos')
        if (!hasGizmo) {
          shouldDeselect = true
        } else {
          // it will be automatically selected
        }
      }
    }

    if (shouldDeselect) {
      selectActiveEntity(null)
    }
  }
})

export class Gizmos extends BaseComponent<GizmoConfiguration> {
  readonly name: string = 'gizmos'

  active = true

  transformValue(data: GizmoConfiguration) {
    return {
      ...defaultValue,
      ...data
    }
  }

  activate = () => {
    this.configureGizmos()
    if (currentConfiguration.cycle && this.entity === activeEntity) {
      selectGizmo(switchGizmo())
    } else {
      selectActiveEntity(this.entity)

      if (currentConfiguration.selectedGizmo) {
        selectGizmo(currentConfiguration.selectedGizmo)
      } else if (isSelectedGizmoValid()) {
        selectGizmo(selectedGizmo)
      } else {
        selectGizmo(switchGizmo())
      }
    }

    activeEntity!.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoSelected',
      gizmoType: selectedGizmo,
      entityId: this.entity.uuid
    })
  }

  didUpdateMesh = () => {
    this.update()
  }

  configureGizmos() {
    if (this.value) {
      currentConfiguration = this.value
      const isWorldCoordinates = !currentConfiguration.localReference
      gizmoManager.gizmos.positionGizmo!.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
      gizmoManager.gizmos.positionGizmo!.updateGizmoRotationToMatchAttachedMesh = !isWorldCoordinates
      gizmoManager.gizmos.scaleGizmo!.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
      gizmoManager.gizmos.rotationGizmo!.updateGizmoRotationToMatchAttachedMesh = !isWorldCoordinates
      gizmoManager.gizmos.rotationGizmo!.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
    }
  }

  update() {
    if (this.entity === activeEntity) {
      this.configureGizmos()
    }
  }

  attach(entity: BaseEntity) {
    super.attach(entity)
    entity.addListener('onClick', this.activate)
    this.entity.onChangeObject3DObservable.add(this.didUpdateMesh)
    this.didUpdateMesh()
  }

  detach() {
    this.active = false
    this.entity.removeListener('onClick', this.activate)
    this.entity.onChangeObject3DObservable.removeCallback(this.didUpdateMesh)
    if (activeEntity === this.entity) {
      selectActiveEntity(null)
    }
    delete this.entity
  }
}
