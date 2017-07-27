import * as BABYLON from 'babylonjs'
import { validators } from '../helpers/schemaValidator'
// tslint:disable:whitespace
type BaseEntity = import('engine/entities/BaseEntity').BaseEntity
type SharedSceneContext = import('engine/entities/SharedSceneContext').SharedSceneContext

export abstract class DisposableComponent {
  contributions = {
    textureCount: 0,
    materialCount: 0,
    geometriesCount: 0
  }

  entities: Set<BaseEntity> = new Set()
  loadingDone: boolean = true

  constructor(public context: SharedSceneContext, public uuid: string) {
    // stub
  }

  attachTo(entity: BaseEntity) {
    if (!this.entities.has(entity)) {
      this.onAttach(entity)
      this.entities.add(entity)
    }
  }

  removeFrom(entity: BaseEntity) {
    if (this.entities.has(entity)) {
      this.onDetach(entity)
      this.entities.delete(entity)
    }
  }

  abstract async updateData(data: any): Promise<void>
  abstract onAttach(entity: BaseEntity): void
  abstract onDetach(entity: BaseEntity): void

  dispose() {
    this.entities.forEach($ => this.removeFrom($))
  }
}

export namespace DisposableComponent {
  export const constructors = new Map<number, typeof DisposableComponent>()

  export function registerClassId<T extends typeof DisposableComponent>(classId: number, ctor: T) {
    if (constructors.has(classId)) {
      throw new Error(`classId(${classId}) is already registered`)
    }
    constructors.set(classId, ctor)
  }
}

export type BasicShapeFields = {
  withCollisions: boolean
  visible: boolean
}

export abstract class BasicShape<T> extends DisposableComponent {
  static readonly nameInEntity = 'basic-shape'

  data: T & Partial<BasicShapeFields> = {
    visible: true,
    withCollisions: false
  } as any

  constructor(public context: SharedSceneContext, public uuid: string) {
    super(context, uuid)
    this.contributions.geometriesCount += 1
  }

  abstract generateModel(): BABYLON.Mesh

  async updateData(data: any) {
    this.data = data || {}

    if (!('visible' in this.data)) {
      this.data.visible = true
    }

    if (!('withCollisions' in this.data)) {
      this.data.withCollisions = false
    }

    this.entities.forEach($ => this.onAttach($))
  }

  onAttach(entity: BaseEntity): void {
    if (this.data.visible === false) {
      entity.removeObject3D(BasicShape.nameInEntity)
    } else {
      const model = this.generateModel()

      model.actionManager = entity.getActionManager()

      entity.setObject3D(BasicShape.nameInEntity, model)

      model.setEnabled(!!this.data.visible)

      this.setCollisions(entity)
    }
  }

  onDetach(entity: BaseEntity): void {
    const model = entity.getObject3D(BasicShape.nameInEntity)

    if (model) {
      entity.removeObject3D(BasicShape.nameInEntity)
      model.dispose(false, false)
    }
  }

  dispose() {
    // stub
  }

  private setCollisions(entity: BaseEntity) {
    const mesh = entity.getObject3D(BasicShape.nameInEntity)
    if (!mesh || !(mesh instanceof BABYLON.AbstractMesh)) return
    mesh.checkCollisions = validators.boolean(this.data.withCollisions, false)
  }
}
