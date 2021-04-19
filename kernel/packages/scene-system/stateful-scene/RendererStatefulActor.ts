import { CLASS_ID } from 'decentraland-ecs/src'
import type { IEngineAPI } from 'shared/apis/IEngineAPI'
import {
  AttachEntityComponentPayload,
  ComponentCreatedPayload,
  ComponentRemovedPayload,
  ComponentUpdatedPayload,
  CreateEntityPayload,
  EntityAction,
  RemoveEntityPayload,
  UpdateEntityComponentPayload
} from 'shared/types'
import {
  Component,
  ComponentData,
  ComponentId,
  EntityId,
  StateContainer,
  StateContainerListener,
  StatefulActor
} from './types'
import { generatePBObjectJSON } from '../sdk/Utils'
import { EventSubscriber } from 'decentraland-rpc'

export class RendererStatefulActor extends StatefulActor implements StateContainerListener {
  private disposableComponents: number = 0
  private readonly eventSubscriber: EventSubscriber

  constructor(protected readonly engine: IEngineAPI, private readonly sceneId: string) {
    super()
    this.eventSubscriber = new EventSubscriber(this.engine)
  }

  addEntity(entityId: EntityId, components?: Component[]): void {
    const batch: EntityAction[] = [
      {
        type: 'CreateEntity',
        payload: { id: entityId } as CreateEntityPayload
      }
    ]
    if (components) {
      components
        .map(({ componentId, data }) => this.mapComponentToActions(entityId, componentId, data))
        .forEach((actions) => batch.push(...actions))
    }
    this.engine.sendBatch(batch)
  }

  removeEntity(entityId: EntityId): void {
    this.engine.sendBatch([
      {
        type: 'RemoveEntity',
        payload: { id: entityId } as RemoveEntityPayload
      }
    ])
  }

  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void {
    const updates = this.mapComponentToActions(entityId, componentId, data)
    this.engine.sendBatch(updates)
  }

  removeComponent(entityId: EntityId, componentId: ComponentId): void {
    const { name } = this.getInfoAboutComponent(componentId)
    this.engine.sendBatch([
      {
        type: 'ComponentRemoved',
        tag: entityId,
        payload: {
          entityId,
          name
        } as ComponentRemovedPayload
      }
    ])
  }

  sendInitFinished() {
    this.engine.sendBatch([
      {
        type: 'InitMessagesFinished',
        tag: 'scene',
        payload: '{}'
      }
    ])
  }

  /**
   * Take a @param container and update it when an change to the º occurs
   */
  forwardChangesTo(container: StateContainer) {
    this.onAddEntity((entityId, components) => container.addEntity(entityId, components))
    this.onRemoveEntity((entityId) => container.removeEntity(entityId))
    this.onSetComponent((entityId, componentId, data) => container.setComponent(entityId, componentId, data))
    this.onRemoveComponent((entityId, componentId) => container.removeComponent(entityId, componentId))
  }

  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'AddEntity') {
        listener(payload.entityId, payload.components)
      }
    })
  }

  onRemoveEntity(listener: (entityId: EntityId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'RemoveEntity') {
        listener(payload.entityId)
      }
    })
  }

  onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'SetComponent') {
        listener(payload.entityId, payload.componentId, payload.data)
      }
    })
  }

  onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'RemoveComponent') {
        listener(payload.entityId, payload.componentId)
      }
    })
  }

  private mapComponentToActions(entityId: EntityId, componentId: ComponentId, data: ComponentData): EntityAction[] {
    const { disposability } = this.getInfoAboutComponent(componentId)
    if (disposability === ComponentDisposability.DISPOSABLE) {
      return this.buildDisposableComponentActions(entityId, componentId, data)
    } else {
      return [
        {
          type: 'UpdateEntityComponent',
          tag: this.sceneId + '_' + entityId + '_' + componentId,
          payload: {
            entityId,
            classId: componentId,
            json: generatePBObjectJSON(componentId, data)
          } as UpdateEntityComponentPayload
        }
      ]
    }
  }

  private buildDisposableComponentActions(entityId: EntityId, classId: number, data: ComponentData): EntityAction[] {
    const id = `C${this.disposableComponents++}`
    return [
      {
        type: 'ComponentCreated',
        tag: id,
        payload: {
          id,
          classId
        } as ComponentCreatedPayload
      },
      {
        type: 'ComponentUpdated',
        tag: id,
        payload: {
          id,
          json: JSON.stringify(data)
        } as ComponentUpdatedPayload
      },
      {
        type: 'AttachEntityComponent',
        tag: entityId,
        payload: {
          entityId,
          id
        } as AttachEntityComponentPayload
      }
    ]
  }

  private getInfoAboutComponent(componentId: ComponentId): { name: string; disposability: ComponentDisposability } {
    switch (componentId) {
      case CLASS_ID.TRANSFORM:
        return { name: 'transform', disposability: ComponentDisposability.NON_DISPOSABLE }
      case CLASS_ID.NAME:
        return { name: 'name', disposability: ComponentDisposability.DISPOSABLE }
      case CLASS_ID.GLTF_SHAPE:
        return { name: 'shape', disposability: ComponentDisposability.DISPOSABLE }
      case CLASS_ID.NFT_SHAPE:
        return { name: 'shape', disposability: ComponentDisposability.DISPOSABLE }
      case CLASS_ID.LOCKED_ON_EDIT:
        return { name: 'lockedOnEdit', disposability: ComponentDisposability.DISPOSABLE }
      case CLASS_ID.VISIBLE_ON_EDIT:
        return { name: 'visibleOnEdit', disposability: ComponentDisposability.DISPOSABLE }
    }
    throw new Error('Component not implemented yet')
  }
}

enum ComponentDisposability {
  DISPOSABLE,
  NON_DISPOSABLE
}
