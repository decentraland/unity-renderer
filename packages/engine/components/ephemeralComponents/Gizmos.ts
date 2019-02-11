import { BaseComponent } from '../BaseComponent'
import { scene } from 'engine/renderer'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { Gizmo } from 'decentraland-ecs/src/decentraland/Gizmos'

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

let activeEntity: BaseEntity = null
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

{
  gizmoManager.gizmos.positionGizmo.onDragEndObservable.add(() => {
    if (!activeEntity) return
    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })

  gizmoManager.gizmos.scaleGizmo.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })

  gizmoManager.gizmos.rotationGizmo.onDragEndObservable.add(() => {
    if (!activeEntity) return

    activeEntity.dispatchUUIDEvent('gizmoEvent', {
      type: 'gizmoDragEnded',
      transform: {
        position: activeEntity.position,
        rotation: activeEntity.rotationQuaternion,
        scale: activeEntity.scaling
      },
      entityId: activeEntity.uuid
    })
  })
}

export function selectGizmo(type: Gizmo) {
  selectedGizmo = type

  if (type === Gizmo.MOVE && currentConfiguration.position) {
    gizmoManager.gizmos.positionGizmo.attachedMesh = activeEntity
    gizmoManager.gizmos.rotationGizmo.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo.attachedMesh = null
  } else if (type === Gizmo.ROTATE && currentConfiguration.rotation) {
    gizmoManager.gizmos.positionGizmo.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo.attachedMesh = activeEntity
    gizmoManager.gizmos.scaleGizmo.attachedMesh = null
  } else if (type === Gizmo.SCALE && currentConfiguration.scale) {
    gizmoManager.gizmos.positionGizmo.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo.attachedMesh = activeEntity
  } else {
    gizmoManager.gizmos.positionGizmo.attachedMesh = null
    gizmoManager.gizmos.rotationGizmo.attachedMesh = null
    gizmoManager.gizmos.scaleGizmo.attachedMesh = null
    selectedGizmo = Gizmo.NONE
    return false
  }
  return true
}

export class Gizmos extends BaseComponent<GizmoConfiguration> {
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
      activeEntity = this.entity

      if (currentConfiguration.selectedGizmo) {
        selectGizmo(currentConfiguration.selectedGizmo)
      } else if (isSelectedGizmoValid()) {
        selectGizmo(selectedGizmo)
      } else {
        selectGizmo(switchGizmo())
      }
    }

    if (selectedGizmo !== Gizmo.NONE) {
      activeEntity.dispatchUUIDEvent('gizmoEvent', {
        type: 'gizmoSelected',
        gizmoType: selectedGizmo,
        entityId: this.entity.uuid
      })
    }
  }

  configureGizmos() {
    currentConfiguration = this.value
    const isWorldCoordinates = !currentConfiguration.localReference
    gizmoManager.gizmos.positionGizmo.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
    gizmoManager.gizmos.positionGizmo.updateGizmoRotationToMatchAttachedMesh = !isWorldCoordinates
    gizmoManager.gizmos.scaleGizmo.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
    gizmoManager.gizmos.rotationGizmo.updateGizmoRotationToMatchAttachedMesh = !isWorldCoordinates
    gizmoManager.gizmos.rotationGizmo.updateGizmoPositionToMatchAttachedMesh = !isWorldCoordinates
  }

  update() {
    if (this.entity === activeEntity) {
      this.configureGizmos()
    }
  }

  attach(entity: BaseEntity) {
    super.attach(entity)
    entity.addListener('onClick', this.activate)
  }

  detach() {
    this.active = false
    this.entity.removeListener('onClick', this.activate)
    if (activeEntity === this.entity) {
      gizmoManager.gizmos.positionGizmo.attachedMesh = null
      gizmoManager.gizmos.rotationGizmo.attachedMesh = null
      gizmoManager.gizmos.scaleGizmo.attachedMesh = null
    }
    activeEntity = null
    this.entity = null
  }
}
