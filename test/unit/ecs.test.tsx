import { expect } from 'chai'

import {
  engine,
  DisposableComponent,
  Entity,
  getComponentId,
  DisposableComponentCreated,
  DisposableComponentRemoved,
  ISystem,
  Component,
  Transform,
  getComponentName,
  Engine,
  OnClick,
  UUIDEvent,
  Quaternion,
  Vector3
} from 'decentraland-ecs/src'

import { future } from 'fp-future'
import {
  loadTestParcel,
  testScene,
  saveScreenshot,
  wait,
  waitForMesh,
  PlayerCamera,
  positionCamera
} from '../testHelpers'
import { sleep } from 'atomicHelpers/sleep'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { AudioClip } from 'engine/components/disposableComponents/AudioClip'
import { AudioSource } from 'engine/components/ephemeralComponents/AudioSource'
import { GLTFShape } from 'engine/components/disposableComponents/GLTFShape'
import { Animator } from 'engine/components/ephemeralComponents/Animator'
import { vrCamera } from 'engine/renderer/camera'
import { interactWithScene } from 'engine/renderer/input'
import { scene } from 'engine/renderer'
import { BasicShape } from 'engine/components/disposableComponents/DisposableComponent'
import { PBRMaterial } from 'engine/components/disposableComponents/PBRMaterial'
import { WebGLParcelScene } from 'engine/dcl/WebGLParcelScene'
import { BoxShape } from 'engine/components/disposableComponents/BoxShape'
import { UIScreenSpace } from 'engine/components/disposableComponents/ui/UIScreenSpace'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'

declare var describe: any, it: any

describe('ECS', () => {
  describe('unit', () => {
    describe('@Component (disposable)', () => {
      const componentName = 'asd'

      @DisposableComponent(componentName, -1)
      class TheDispoCompo {
        asd = 1

        constructor() {
          this.asd = 3
        }
      }

      it('DisposableComponent.engine must be set', () => {
        expect(DisposableComponent.engine).to.be.instanceOf(Engine)
      })

      it('should have symbol in the class as long as the instance', () => {
        const inst = new TheDispoCompo()

        expect(inst).to.be.instanceOf(TheDispoCompo)
        expect(getComponentName(inst)).to.eq(componentName)
        expect(getComponentName(TheDispoCompo)).to.eq(componentName)

        expect(Object.values(engine.disposableComponents)).to.contain(
          inst,
          'the disposable component should be present in engine.disposableComponents'
        )
      })

      it('should add the component to an entity using the indicated component name', () => {
        const entity = new Entity()
        entity.addComponent(new TheDispoCompo())
        expect(entity.getComponent(TheDispoCompo)).to.be.instanceOf(TheDispoCompo)
        expect(entity.components[componentName]).to.be.instanceOf(TheDispoCompo)
        expect(entity.components[componentName]).to.eq(entity.getComponent(TheDispoCompo))
        entity.removeComponent(entity.getComponent(TheDispoCompo))
        expect(entity.components[componentName]).to.be.undefined

        // TODO: WHAT HAPPENS WITH DISPOSABLE COMPONENTS NOT ADDED TO THE ENSHIN?
      })
    })

    describe('@Component', () => {
      const componentName = 'asd'

      @Component(componentName)
      class TheCompo {
        asd = 1
        constructor() {
          this.asd = 3
        }
      }
      @Component(componentName)
      class TheCompoNotTheCompo {
        asd = 1
        constructor() {
          this.asd = 3
        }
      }
      it('should have symbol in the class as long as the instance', () => {
        const inst = new TheCompo()

        expect(inst).to.be.instanceOf(TheCompo)
        expect(getComponentName(inst)).to.eq(componentName)
        expect(getComponentName(TheCompo)).to.eq(componentName)
      })

      it('should add the component to an entity using the indicated component name', () => {
        const entity = new Entity()

        const compo = new TheCompo()

        expect(entity.hasComponent(compo)).to.eq(false)
        expect(entity.hasComponent(TheCompo)).to.eq(false)
        expect(entity.hasComponent(TheCompoNotTheCompo)).to.eq(false)
        expect(entity.hasComponent(componentName)).to.eq(false)
        entity.addComponent(compo)
        expect(entity.hasComponent(compo)).to.eq(true, 'has(compo)')
        expect(entity.hasComponent(TheCompo)).to.eq(true, 'has(TheCompo)')
        expect(entity.hasComponent(TheCompoNotTheCompo)).to.eq(false, 'has(TheCompoNotTheCompo)')
        expect(entity.hasComponent(componentName)).to.eq(true, 'has(componentName)')

        expect(entity.getComponent(TheCompo)).to.be.instanceOf(TheCompo)
        expect(entity.components[componentName]).to.be.instanceOf(TheCompo)
        expect(entity.components[componentName]).to.eq(entity.getComponent(TheCompo))
        entity.removeComponent(entity.getComponent(TheCompo))
        expect(entity.components[componentName]).to.be.undefined
      })
    })

    describe('ComponentGroup with transform', () => {
      const group = engine.getComponentGroup(Transform)

      it('should have no entities for any component not present in any entity', () => {
        expect(group.entities.length).to.eq(0)
      })

      it('instantiate an entity with a component and add it to the engine should modify the group', () => {
        const entity = new Entity()

        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        expect(group.entities.length).to.eq(0)
        const transform = entity.getComponentOrCreate(Transform)
        expect(transform).to.be.instanceOf(Transform)
        expect(Object.values(entity.components)).to.contain(transform)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.removeComponent(transform)
        expect(Object.values(entity.components)).to.not.contain(transform)
        expect(group.entities.length).to.eq(0)
        engine.removeEntity(entity)
        expect(group.entities.length).to.eq(0)
      })
    })
    describe('ComponentGroup with one component', () => {
      @Component('a component')
      class AComponent {}

      const group = engine.getComponentGroup(AComponent)

      it('should have no entities for any component not present in any entity', () => {
        expect(group.entities.length).to.eq(0)
      })

      it('instantiate an entity with a component and add it to the engine should modify the group', () => {
        const entity = new Entity()
        const component = new AComponent()
        entity.addComponentOrReplace(component)
        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        engine.removeEntity(entity)
        expect(group.entities.length).to.eq(0)
      })

      it('a entity in the engine should modify the groups when changing components', () => {
        const entity = new Entity()
        engine.addEntity(entity)

        expect(group.entities.length).to.eq(0)

        const component = new AComponent()
        entity.addComponentOrReplace(component)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        entity.removeComponent(component)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group')

        engine.removeEntity(entity)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group 2')
      })
    })

    describe('ComponentGroup with two component', () => {
      @Component('a component')
      class AComponent {}

      @Component('another component')
      class AnotherComponent {}

      const group = engine.getComponentGroup(AComponent, AnotherComponent)

      it('should have no entities for any component not present in any entity', () => {
        expect(group.entities.length).to.eq(0)
      })

      it('instantiate an entity with a component and add it to the engine should modify the group', () => {
        const entity = new Entity()
        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        entity.addComponentOrReplace(component2)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        engine.removeEntity(entity)
        expect(group.entities.length).to.eq(0)
      })

      it('adding the 2nd component after the entity already has one should add it to the ComponentGroup', () => {
        const entity = new Entity()
        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        entity.addComponentOrReplace(component2)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.removeComponent(component)
        expect(group.entities.length).to.eq(0)
        engine.removeEntity(entity)
      })

      it('a entity in the engine should modify the groups when changing components', () => {
        const entity = new Entity()
        engine.addEntity(entity)

        expect(group.entities.length).to.eq(0)

        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        entity.addComponentOrReplace(component2)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        engine.removeEntity(entity)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group')

        entity.removeComponent(component2)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group 2')
      })
    })

    describe('ComponentGroup with two component, create group after entities', () => {
      @Component('a component')
      class AComponent {}

      @Component('another component')
      class AnotherComponent {}

      it('instantiate an entity with a component and add it to the engine should modify the group', () => {
        const entity = new Entity()
        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        engine.addEntity(entity)
        entity.addComponentOrReplace(component2)
        const group = engine.getComponentGroup(AComponent, AnotherComponent)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        engine.removeEntity(entity)
        expect(group.entities.length).to.eq(0)
        engine.removeComponentGroup(group)
      })

      it('adding the 2nd component after the entity already has one should add it to the ComponentGroup', () => {
        const entity = new Entity()
        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        engine.addEntity(entity)
        entity.addComponentOrReplace(component2)
        const group = engine.getComponentGroup(AComponent, AnotherComponent)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.removeComponent(component)
        expect(group.entities.length).to.eq(0)
        engine.removeEntity(entity)
        engine.removeComponentGroup(group)
      })

      it('a entity in the engine should modify the groups when changing components', () => {
        const entity = new Entity()
        engine.addEntity(entity)

        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.addComponentOrReplace(component)
        entity.addComponentOrReplace(component2)

        const group = engine.getComponentGroup(AComponent, AnotherComponent)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        engine.removeEntity(entity)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group')

        entity.removeComponent(component2)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group 2')

        engine.removeComponentGroup(group)
      })
    })

    describe('Events', () => {
      it('should call event only once when event is added before adding entity', async () => {
        const uuidEventFuture = future()
        const uuidEventFutureComponent = future()

        let counter = 0

        const entity = new Entity()

        const clicker = new OnClick(event => {
          counter += 1
          uuidEventFutureComponent.resolve(event)
        })

        entity.addComponentOrReplace(clicker)

        engine.addEntity(entity)

        engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
          uuidEventFuture.resolve(event.uuid)
        })

        const event = new UUIDEvent(clicker.uuid, { data: { from: {}, direction: {}, pointerId: 1, length: 0 } })

        engine.eventManager.fireEvent(event)

        expect(await uuidEventFuture).to.eq(clicker.uuid, 'event should be fired')
        await uuidEventFutureComponent

        await sleep(100)

        expect(counter).to.eq(1, 'only one event should have been called')
      })

      it('should call event only once when event is added after adding entity', async () => {
        const uuidEventFuture = future()
        const uuidEventFutureComponent = future()

        let counter = 0

        const entity = new Entity()

        const clicker = new OnClick(event => {
          counter += 1
          uuidEventFutureComponent.resolve(event)
        })

        engine.addEntity(entity)

        entity.addComponentOrReplace(clicker)

        engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
          uuidEventFuture.resolve(event.uuid)
        })

        const event = new UUIDEvent(clicker.uuid, { data: { from: {}, direction: {}, pointerId: 1, length: 0 } })

        engine.eventManager.fireEvent(event)

        expect(await uuidEventFuture).to.eq(clicker.uuid, 'event should be fired')
        await uuidEventFutureComponent

        await sleep(100)

        expect(counter).to.eq(1, 'only one event should have been called')
      })

      it('should not be called after the component is removed', async () => {
        const uuidEventFuture = future()
        const uuidEventFutureComponent = future()

        const entity = new Entity()

        const clicker = new OnClick(event => {
          uuidEventFutureComponent.reject(new Error())
        })

        entity.addComponentOrReplace(clicker)

        engine.addEntity(entity)

        entity.removeComponent(clicker)

        engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
          uuidEventFuture.resolve(event.uuid)
        })

        const event = new UUIDEvent(clicker.uuid, void 0)

        engine.eventManager.fireEvent(event)

        expect(await uuidEventFuture).to.eq(clicker.uuid, 'event should be fired')

        setTimeout(() => uuidEventFutureComponent.resolve(0), 100)

        await uuidEventFutureComponent
      })
    })
  })

  describe('integration', () => {
    it('should register a globally disposable component', async () => {
      @DisposableComponent('gltf-model', 55)
      class GLTFModel {
        src: string = ''
        onDispose() {
          // stub
        }
      }

      const idFuture = future()

      engine.eventManager.addListener(DisposableComponentCreated, idFuture, event => {
        idFuture.resolve(event.componentId)
      })

      const model = new GLTFModel()
      const id = getComponentId(model)
      const ent = new Entity()
      ent.addComponentOrReplace(model)
      model.src = 'example.gltf'

      expect(await idFuture).to.eq(id, 'event should be fired')
      expect(engine['disposableComponents'][id]).to.eq(model, 'component should be present')
    })

    it('should dispose a disposable component', async () => {
      const idFuture = future()
      const callbackFuture = future()

      @DisposableComponent('gltf-model', 55)
      class GLTFModel {
        src: string = ''
        onDispose() {
          callbackFuture.resolve(true)
        }
      }

      engine.eventManager.addListener(DisposableComponentRemoved, idFuture, event => {
        idFuture.resolve(event.componentId)
      })

      const model = new GLTFModel()
      const id = getComponentId(model)
      const ent = new Entity()
      ent.addComponentOrReplace(model)
      model.src = 'example.gltf'
      engine.disposeComponent(model)

      expect(await idFuture).to.eq(id, 'event should be fired')
      expect(await callbackFuture).to.eq(true, 'callback should be called')
      expect(engine['disposableComponents'][id]).to.eq(undefined, 'component should be removed')
    })

    it('should handle system priorities', async () => {
      class SomeSystem implements ISystem {}

      const instances: ISystem[] = []

      for (let i = 0; i < 7; i++) {
        instances.push(new SomeSystem())
      }

      function check(engineIndex: number, localIndex: number) {
        expect(engine.systems[engineIndex].system).to.eq(
          instances[localIndex],
          `local ${localIndex} != ${engineIndex} was ${engine.systems.findIndex(
            $ => $.system === instances[localIndex]
          )}`
        )
      }

      const i = engine.systems.length

      engine.addSystem(instances[0], 0)
      engine.addSystem(instances[1], 1)

      check(i + 0, 0)
      check(i + 1, 1)
      expect(engine.addedSystems.includes(instances[0])).to.eq(true)
      expect(engine.addedSystems.includes(instances[1])).to.eq(true)

      engine.addSystem(instances[2])
      engine.addSystem(instances[3], 1)
      engine.addSystem(instances[4], 2)
      engine.addSystem(instances[5], 2)
      engine.addSystem(instances[6], 5)

      check(i + 0, 0)
      check(i + 1, 2)
      check(i + 2, 1)
      check(i + 3, 3)
      check(i + 4, 4)
      check(i + 5, 5)
      check(i + 6, 6)

      engine.removeSystem(instances[1])
      engine.removeSystem(instances[5])

      expect(engine.addedSystems.includes(instances[1])).to.eq(false)
      expect(engine.addedSystems.includes(instances[5])).to.eq(false)

      check(i + 0, 0)
      check(i + 1, 2)
      check(i + 2, 3)
      check(i + 3, 4)
      check(i + 4, 6)

      engine.addSystem(instances[1], 2)
      engine.addSystem(instances[5], 9)

      check(i + 0, 0)
      check(i + 1, 2)
      check(i + 2, 3)
      check(i + 3, 4)
      check(i + 4, 1)
      check(i + 5, 6)
      check(i + 6, 5)

      for (let i = 0; i < instances.length; i++) {
        expect(engine.addedSystems.includes(instances[i])).to.eq(true)
      }

      for (let system of instances) {
        engine.removeSystem(system)
      }

      expect(engine.systems.length).to.eq(i)
    })

    it('should correctly set parent entities', () => {
      const ent1 = new Entity()
      const ent2 = new Entity()
      ent1.setParent(ent2)
      engine.addEntity(ent1)
      engine.addEntity(ent2)

      expect(ent1.getParent()).to.eq(ent2)
      expect(Object.keys(ent2.children).some(c => c === ent1.uuid)).to.eq(true)
    })

    it('should set parents after adding entities to engine', () => {
      const ent1 = new Entity()
      const ent2 = new Entity()
      const ent3 = new Entity()

      engine.addEntity(ent3)
      ent3.setParent(ent2)

      expect(ent3.getParent()).to.eq(ent2)
      expect(Object.keys(ent2.children).some(c => c === ent3.uuid)).to.eq(true)

      engine.addEntity(ent2)
      engine.addEntity(ent1)
      ent2.setParent(ent1)

      expect(ent2.getParent()).to.eq(ent1)
      expect(Object.keys(ent1.children).some(c => c === ent2.uuid)).to.eq(true)
    })

    it('should fail to set itself as a parent', () => {
      const ent1 = new Entity()
      ;(ent1 as any).uuid = 'ent1'
      engine.addEntity(ent1)

      try {
        ent1.setParent(ent1)
      } catch (e) {
        expect(e.message).to.eq(
          `Failed to set parent for entity "ent1": An entity can't set itself as a its own parent`
        )
      }
    })

    it('should fail to set simple circular parent references', () => {
      const ent1 = new Entity()
      const ent2 = new Entity()
      ;(ent1 as any).uuid = 'ent1'
      ;(ent2 as any).uuid = 'ent2'

      ent1.setParent(ent2)
      engine.addEntity(ent1)
      engine.addEntity(ent2)

      try {
        ent2.setParent(ent1)
      } catch (e) {
        expect(e.message).to.eq(
          'Failed to set parent for entity "ent2": Circular parent references are not allowed (See entity "ent1")'
        )
      }
    })

    it('should fail to set indirect circular parent references', () => {
      const ent1 = new Entity()
      const ent2 = new Entity()
      const ent3 = new Entity()
      ;(ent1 as any).uuid = 'ent1'
      ;(ent2 as any).uuid = 'ent2'
      ;(ent3 as any).uuid = 'ent3'

      engine.addEntity(ent1)
      ent1.setParent(ent3)
      ent2.setParent(ent1)
      engine.addEntity(ent2)
      engine.addEntity(ent3)

      try {
        ent3.setParent(ent2)
      } catch (e) {
        expect(e.message).to.eq(
          'Failed to set parent for entity "ent3": Circular parent references are not allowed (See entity "ent1")'
        )
      }
    })

    it('should recursively remove entities', () => {
      // cleanup previous stuff
      for (let key in engine['_entities']) {
        engine.removeEntity(engine['_entities'][key])
      }

      const ent1 = new Entity()
      const ent2 = new Entity()
      const ent3 = new Entity()

      ent2.setParent(ent1)
      ent3.setParent(ent2)

      engine.addEntity(ent1)
      engine.addEntity(ent2)
      engine.addEntity(ent3)

      engine.removeEntity(ent1)

      expect(Object.keys(engine['_entities']).length).to.eq(0)
    })
  })

  describe('unit', () => {
    {
      let entityFuture = future<BaseEntity>()

      testScene(-100, 234, ({ parcelScenePromise }) => {
        it('should have a transform component', async function(this: any) {
          this.timeout(50000)
          const parcelScene = await parcelScenePromise
          const [, [, entity]] = parcelScene.context.entities // should be the first child

          expect(!!entity).to.eq(true, 'entity should exist')
          expect(entity.position.x).to.eq(5)
          expect(entity.position.z).to.eq(5)
          expect(parcelScene.context.disposableComponents.size).to.eq(1, 'there must be one disposable component')
          const [[, shape]] = parcelScene.context.disposableComponents
          expect(shape.entities.has(entity)).to.eq(true, 'the shape must have the entity')
          entityFuture.resolve(entity)
          await waitForMesh(entity)
        })

        saveScreenshot(`gamekit-gltf.png`, {
          from: [-100, 1.6, 234],
          lookAt: [-99.5, 1, 234.5]
        })

        wait(100)
      })

      describe('sanity from previous test', () => {
        it('entity from previoust test must have been disposed', async () => {
          const entity = await entityFuture
          expect(entity.isDisposed()).to.eq(true)
        })
      })
    }

    testScene(
      -100,
      103,
      ({ parcelScenePromise, sceneHost, ensureNoErrors }) => {
        it('dcl.onUpdate must work', async () => {
          sceneHost.update(0)

          while (sceneHost.devToolsAdapter!.exceptions.length < 1) {
            await sleep(100)
          }

          expect(sceneHost.devToolsAdapter!.exceptions.length).eq(1)

          expect(sceneHost.devToolsAdapter!.exceptions[0].toString()).to.include('onUpdate works')
        })

        it('should have a transform component', async () => {
          const parcelScene = await parcelScenePromise

          const entity = parcelScene.context.entities.get('__test_id__') // should be the first child
          expect(!!entity).to.eq(true, 'entity should exist')

          expect(parcelScene.context.disposableComponents.size).to.eq(
            1,
            'should have one registered disposable component'
          )

          sceneHost.update(1) // dispose should happen on this update
        })

        it('components should have been disposed', async () => {
          const parcelScene = await parcelScenePromise

          expect(parcelScene.context.disposableComponents.size).to.eq(
            0,
            'should have zero registered disposable component'
          )
        })
      },
      true
    )

    testScene(-100, 105, ({ parcelScenePromise, sceneHost, ensureNoErrors }) => {
      it('should have a transform component', async () => {
        sceneHost.update(0)
        const parcelScene = await parcelScenePromise

        while (parcelScene.context.disposableComponents.size < 3) {
          await sleep(100)
        }

        expect(parcelScene.context.disposableComponents.size).eq(3, 'components')
        expect(parcelScene.context.entities.size).eq(2, 'entities')

        const material = parcelScene.context.disposableComponents.values().next().value

        expect(material).to.exist

        var entities: BaseEntity[] = Array.from(parcelScene.context.entities.values())

        expect(entities[0].uuid).to.eq('0', 'root entity')

        const entity = entities[1]

        expect(entity).to.exist

        expect(material.entities.has(entity)).eq(true)

        ensureNoErrors()
      })
    })

    testScene(
      -100,
      100,
      ({ logs }) => {
        it('locate camera', async () => {
          await positionCamera({
            from: [-99.5, 1, 100.0],
            lookAt: [-99.5, 1, 101.0]
          })
          vrCamera!.rotationQuaternion.copyFrom(BABYLON.Quaternion.Identity())
          expect(logs.length).to.eq(0)
        })
        it('clicks in the middle of the screen', async () => {
          const canvas = scene.getEngine().getRenderingCanvas()

          interactWithScene('pointerDown', canvas!.width / 2, canvas!.height / 2, 1)
          await sleep(100)
          expect(logs.filter($ => $[1] === 'event').length).to.eq(1, 'event must have been triggered once')
          interactWithScene('pointerUp', canvas!.width / 2, canvas!.height / 2, 1)
          await sleep(100)

          expect(logs.length).to.gt(0)
          expect(logs.filter($ => $[1] === 'cubeClick').length).to.eq(1, 'cubeClick must have been triggered')
          expect(logs.filter($ => $[1] === 'event').length).to.eq(2, 'event must have been triggered twice')
          logs.length = 0
        })
        it('clicks in the sky must trigger pointer events', async () => {
          interactWithScene('pointerDown', 1, 1, 1)
          await sleep(100)
          expect(logs.filter($ => $[1] === 'event').length).to.eq(1, 'event must have been triggered once')
          interactWithScene('pointerUp', 1, 1, 1)
          await sleep(100)
          expect(logs.filter($ => $[1] === 'event').length).to.eq(2, 'event must have been triggered twice')
          logs.length = 0
        })
      },
      true
    )

    testScene(-100, 104, ({ parcelScenePromise, sceneHost, logs }) => {
      it('locate camera', async () => {
        vrCamera!.position.set(-995, 1, 1040)
        vrCamera!.rotationQuaternion.copyFrom(BABYLON.Quaternion.Identity())
      })

      it('changing a material must work', async () => {
        const parcelScene = await parcelScenePromise
        let entity: BaseEntity

        parcelScene.context.entities.forEach($ => {
          if ($.position.x == 5 && $.position.z == 5) {
            entity = $
          }
        })
        expect(!!entity!).to.eq(true)
        const mesh = entity!.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh
        expect(!!mesh).to.eq(true)

        const materials = getMaterials(parcelScene.context)

        expect(materials.length).to.gte(2)

        const [M1, M2] = materials

        expect(mesh.material === M1.material).to.eq(true, 'M1 must be set')

        sceneHost.fireEvent({ type: 'TEST_TRIGGER' })

        await sleep(1000)
        const newLogs = logs.map($ => $[1])

        expect(newLogs).to.include(`setting ${entity!.uuid} <- ${M2.uuid}`)

        const newMesh = entity!.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh
        expect(!!newMesh).to.eq(true)
        expect(newMesh == mesh).to.eq(true, 'mesh == oldmesh')

        expect(newMesh.material === M2.material).to.eq(true, 'M2 must be set')
      })
    })

    testScene(-101, 100, ({ parcelScenePromise, sceneHost, logs }) => {
      let scene: WebGLParcelScene
      it('locate camera', async () => {
        scene = await parcelScenePromise
        vrCamera!.position.set(-995, 1, 1000)
        vrCamera!.rotationQuaternion.copyFrom(BABYLON.Quaternion.Identity())
      })

      function doTest() {
        let entity: BaseEntity
        let shape: BasicShape<any>
        let materialComponent: PBRMaterial

        it('must start without entities and disposable components', async () => {
          scene.context.entities.forEach(($, id) => {
            expect(id.startsWith('E')).to.eq(false, `entity ${id} exists`)
          })
          expect(scene.context.disposableComponents.size).to.eq(0)
          expect(scene.context.metrics.entities).to.eq(0, 'there must be 0 entities')
        })

        it('must create a entity', async () => {
          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          scene.context.entities.forEach(($, id) => {
            if (entity) {
              expect(id.startsWith('E')).to.eq(false, `entity ${id} exists`)
            } else {
              if (id.startsWith('E')) {
                entity = $
              }
            }
          })

          expect(scene.context.metrics.entities).to.eq(1, 'there must be 1 entity')
          expect(scene.context.metrics.triangles).to.eq(0, 'triangles must be 0')
          expect(scene.context.metrics.geometries).to.eq(0, 'geometries must be 0')
          expect(scene.context.metrics.bodies).to.eq(0, 'bodies must be 0')

          expect(!!entity).to.eq(true)
        })

        it('must create a shape', async () => {
          expect(scene.setOfEntitiesOutsideBoundaries.size).eq(
            0,
            'scene must start without entitites out of the fences'
          )

          expect(scene.context.metrics.geometries).eq(0, 'counters must start at 0')

          expect(!!entity.getObject3D(BasicShape.nameInEntity)).to.eq(false)

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect(scene.context.disposableComponents.size).to.eq(1)

          const [[, value]] = scene.context.disposableComponents

          expect(!!entity.getObject3D(BasicShape.nameInEntity)).to.eq(true)

          expect(value instanceof BoxShape).to.eq(true)
          shape = value as BoxShape
          expect(value.entities.has(entity)).to.eq(true, 'box shape must have the entity')

          expect(scene.setOfEntitiesOutsideBoundaries.size).eq(1, 'The cube starts outside of the parcel')
          const [entityOutside] = scene.setOfEntitiesOutsideBoundaries
          expect(entityOutside).eq(entity, 'The entity outside the parcel must be our entity')
          expect(scene.context.metrics.geometries).eq(1, 'geometry counters must have been updated to 1')
          // TODO: test outsideFences events
        })

        it('must set the transform', async () => {
          expect('transform' in entity.components).to.eq(false)

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect('transform' in entity.components).to.eq(true)

          expect(scene.setOfEntitiesOutsideBoundaries.size).eq(
            0,
            'The cube must have been moved to the center of the parcel'
          )

          // TODO: test outsideFences events
        })

        it('must set the material', async () => {
          expect(scene.context.metrics.materials).eq(0, 'material counters must start with 0')

          const mesh = entity.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh
          expect(!!mesh).to.eq(true, 'mesh must exist')
          const originalMaterial = mesh.material
          expect(scene.context.disposableComponents.size).to.eq(1)

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect(scene.context.disposableComponents.size).to.eq(2)

          const [, [, newMaterialComponent]] = scene.context.disposableComponents
          materialComponent = newMaterialComponent as PBRMaterial

          expect(mesh.material != originalMaterial).eq(
            true,
            'the new material must be different than the older material'
          )

          expect(mesh.material == materialComponent.material).eq(true, 'the shape must have the new material')
          expect(scene.context.metrics.materials).eq(1, 'material counters must have been updated to 1')
        })

        it('must set the material color', async () => {
          expect(materialComponent.material.albedoColor.toHexString()).eq('#7F7F7F')

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect(materialComponent.material.albedoColor.toHexString()).eq('#00FF00')
        })

        it('must remove the material', async () => {
          const initialMetrics = { ...scene.context.metrics }

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          const mesh = entity.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh

          expect(mesh.material != materialComponent.material).eq(true, 'the material shape must have been removed')
          expect(materialComponent.entities.has(entity)).eq(
            false,
            'the entity must have been removed from the material'
          )
          expect(scene.context.disposableComponents.size).to.eq(1, 'the material must have been removed from the scene')
          expect(scene.context.metrics.materials).eq(
            initialMetrics.materials - 1,
            'material counters must have been updated'
          )
        })

        it('must remove the shape', async () => {
          expect(scene.context.metrics.geometries).eq(1, 'geometry counters must be 1')

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          const mesh = entity.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh

          expect(!!mesh).eq(false, 'the shape must have been removed from the entity')

          expect(shape.entities.has(entity)).eq(false, 'the entity must have been removed from the shape')

          expect(scene.context.disposableComponents.size).to.eq(0, 'the shape must have been removed from the scene')

          expect(scene.context.metrics.geometries).eq(0, 'geometry counters must have been updated to 0')
        })

        it('must remove the transform', async () => {
          expect('transform' in entity.components).to.eq(true)

          expect(entity.position.x).eq(5)
          expect(entity.position.y).eq(5)
          expect(entity.position.z).eq(5)
          expect(entity.scaling.x).eq(1.1)
          expect(entity.scaling.y).eq(1.1)
          expect(entity.scaling.z).eq(1.1)

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect('transform' in entity.components).to.eq(false)

          expect(entity.position.x).eq(0, 'position must have been restored to 0')
          expect(entity.position.y).eq(0, 'position must have been restored to 0')
          expect(entity.position.z).eq(0, 'position must have been restored to 0')
          expect(entity.scaling.x).eq(1, 'scale must have been restored to 1')
          expect(entity.scaling.y).eq(1, 'scale must have been restored to 1')
          expect(entity.scaling.z).eq(1, 'scale must have been restored to 1')
        })

        it('must remove the entity', async () => {
          const initialEntityCounter = scene.context.entities.size

          sceneHost.fireEvent({ type: 'TEST_TRIGGER' })
          await sleep(100)

          expect(scene.context.entities.size).to.eq(initialEntityCounter - 1)
        })
      }

      doTest()
      doTest()
    })

    function getMaterials(context: SharedSceneContext) {
      const materials: PBRMaterial[] = []

      for (let [, x] of context.disposableComponents) {
        if (x instanceof PBRMaterial) {
          materials.push(x)
        }
      }

      return materials
    }

    testScene(-100, 104, ({ parcelScenePromise, sceneHost, logs }) => {
      it('locate camera', async () => {
        vrCamera!.position.set(-995, 1, 1040)
        vrCamera!.rotationQuaternion.copyFrom(BABYLON.Quaternion.Identity())
      })

      it('changing a material must work', async () => {
        const parcelScene = await parcelScenePromise
        let entity: BaseEntity

        parcelScene.context.entities.forEach($ => {
          if ($.position.x == 5 && $.position.z == 5) {
            entity = $
          }
        })

        expect(!!entity!).to.eq(true)
        const mesh = entity!.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh
        expect(!!mesh).to.eq(true)

        const materials = getMaterials(parcelScene.context)

        expect(materials.length).to.gte(2)

        const [M1, M2] = materials

        expect(mesh.material === M1.material).to.eq(true, 'M1 must be set')

        sceneHost.fireEvent({ type: 'TEST_TRIGGER' })

        await sleep(1000)
        const newLogs = logs.map($ => $[1])

        expect(newLogs).to.include(`setting ${entity!.uuid} <- ${M2.uuid}`)

        const newMesh = entity!.getObject3D(BasicShape.nameInEntity) as BABYLON.AbstractMesh
        expect(!!newMesh).to.eq(true)
        expect(newMesh == mesh).to.eq(true, 'mesh == oldmesh')

        expect(newMesh.material === M2.material).to.eq(true, 'M2 must be set')
      })
    })

    testScene(-200, 100, ({ parcelScenePromise, sceneHost, logs }) => {
      let scene: WebGLParcelScene
      let screenSpace: UIScreenSpace | null = null
      const insideCamera: PlayerCamera = { from: [-199.9, 1.6, 100.1], lookAt: [-199.5, 1, 100.5] }
      const outsideCamera: PlayerCamera = { from: [-201.0, 1.6, 101.5], lookAt: [-200.0, 1, 102.0] }

      it('locate camera', async () => {
        vrCamera!.position.set(-1999, 1, 1001)
        vrCamera!.rotationQuaternion.copyFrom(BABYLON.Quaternion.Identity())
      })

      it('should have an UIScreenSpace component', async () => {
        const parcelScene = await parcelScenePromise
        scene = parcelScene

        while (scene.context.disposableComponents.size < 8) {
          await sleep(100)
        }

        scene.checkBoundaries()

        expect(scene.context.disposableComponents.size).eq(8)
        expect(scene.context.entities.size).eq(4)

        screenSpace = scene.context.disposableComponents.values().next().value as any

        expect(screenSpace).to.exist
        expect(screenSpace).to.be.instanceOf(UIScreenSpace)
      })

      wait(100)

      it('should be enabled', async () => {
        await positionCamera(insideCamera)

        scene.checkBoundaries()
        await sleep(500)
        scene.checkUserInPlace()

        expect(screenSpace!.isEnabled).to.eq(true, 'screen space must be enabled')

        await sleep(100)
        sceneHost.fireEvent({ type: 'TEST_TRIGGER' })

        await sleep(500)

        expect(screenSpace!.data.visible).to.eq(true, 'screen space must be visible')
      })

      wait(100)

      it('should not be enabled', async () => {
        await positionCamera(outsideCamera)

        scene.checkBoundaries()
        await sleep(500)
        scene.checkUserInPlace()

        await sleep(100)

        expect(screenSpace!.isEnabled).to.eq(false)
        expect(screenSpace!.data.visible).to.eq(true)
      })
    })

    describe('sound', () => {
      let audioClips: AudioClip[] = []
      let audioSources: AudioSource[] = []

      loadTestParcel('test unload', -200, 2, function(_root, futureScene, futureWorker) {
        it('must have two audio clips', async () => {
          const scene = await futureScene
          scene.context.disposableComponents.forEach($ => {
            if ($ instanceof AudioClip) {
              audioClips.push($)
            }
          })

          expect(audioClips.length).to.eq(2)
        })

        it('must have two audio sources', async () => {
          const scene = await futureScene
          scene.context.entities.forEach($ => {
            for (let i in $.components) {
              if ($.components[i] instanceof AudioSource) {
                audioSources.push($.components[i] as any)
              }
            }
          })

          expect(audioSources.length).to.eq(2)
        })
      })

      describe('after finalizing', () => {
        it('must have stopped AudioClips', () => {
          for (let clip of audioClips) {
            expect(clip.entities.size).eq(0)
          }
        })

        it('must have stopped AudioSources', async () => {
          for (let source of audioSources) {
            expect(source.sound.isPending).eq(true)
          }
        })
      })
    })

    describe('gltf animations', () => {
      let gltf: GLTFShape[] = []
      let animators: Animator[] = []

      loadTestParcel('test animatios', -100, 111, function(_root, futureScene, futureWorker) {
        it('must have one gltf', async () => {
          const scene = await futureScene
          scene.context.disposableComponents.forEach($ => {
            if ($ instanceof GLTFShape) {
              gltf.push($)
            }
          })

          expect(gltf.length).to.eq(1)
        })

        it('must have two animators', async () => {
          const scene = await futureScene
          scene.context.entities.forEach($ => {
            for (let i in $.components) {
              if ($.components[i] instanceof Animator) {
                animators.push($.components[i] as Animator)
              }
            }
          })

          expect(animators.length).to.eq(2)
        })

        it('wait some seconds', async () => {
          await sleep(1000)
        })
      })

      describe('after', () => {
        it('must have no entities in the shapes', () => {
          for (let shape of gltf) {
            expect(shape.assetContainerEntity.size).to.eq(0, 'asset container')
            expect(shape.entities.size).to.eq(0, 'entities')
          }
        })
      })
    })
  })

  describe('math', () => {
    it('should correctly convert between euler and quaternions', () => {
      for (let i = 0; i < 500; i++) {
        const euler = new Vector3(
          Math.round(Math.random() * 1000) - Math.round(Math.random() * 1000),
          Math.round(Math.random() * 1000) - Math.round(Math.random() * 1000),
          Math.round(Math.random() * 1000) - Math.round(Math.random() * 1000)
        )
        const quat = new Quaternion()
        quat.eulerAngles = euler

        expect(anglesAreEqual(quat.eulerAngles, euler)).to.eq(true)
      }
    })
  })

  describe('resolve urls', () => {
    testScene(-1, 73, ({ parcelScenePromise }) => {
      it('should have a transform component', async () => {
        const parcelScene = await parcelScenePromise
        const texture = await parcelScene.context.getTexture('img #7 @ $1.png')
        expect(texture.isReady()).to.eq(true)
      })

      saveScreenshot(`material-billboard.png`, {
        from: [-1, 1.6, 73],
        lookAt: [-0.5, 1.6, 73.5]
      })

      wait(100)
    })
  })
})

function anglesAreEqual(e1: Vector3, e2: Vector3) {
  const angle = Quaternion.Angle(Quaternion.Euler(e1.x, e1.y, e1.z), Quaternion.Euler(e2.x, e2.y, e2.z))
  return Math.abs(angle) < 1e-3
}
