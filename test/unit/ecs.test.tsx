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
import { loadTestParcel, testScene, saveScreenshot, wait } from '../testHelpers'
import { sleep } from 'atomicHelpers/sleep'
import { BaseEntity } from 'engine/entities/BaseEntity'

describe('ECS', () => {
  describe('unit', () => {
    {
      let entityFuture = future<BaseEntity>()
      testScene(-100, 234, ({ parcelScenePromise }) => {
        it('should have a transform component', async () => {
          const parcelScene = await parcelScenePromise

          const entity = parcelScene.context.entities.get('__test_id__') // should be the first child
          expect(!!entity).to.eq(true, 'entity should exist')
          expect(entity.position.x).to.eq(5)
          expect(entity.position.z).to.eq(5)
          entityFuture.resolve(entity)
        })

        saveScreenshot(`gamekit-gltf.png`, {
          from: [-1000, 1.6, 2340],
          lookAt: [-995, 1, 2345]
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

          while (sceneHost.devToolsAdapter.exceptions.length < 1) {
            await sleep(100)
          }

          expect(sceneHost.devToolsAdapter.exceptions.length).eq(1)

          expect(sceneHost.devToolsAdapter.exceptions[0].toString()).to.include('onUpdate works')
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

        while (parcelScene.context.disposableComponents.size < 2) {
          await sleep(100)
        }

        expect(parcelScene.context.disposableComponents.size).eq(2)
        expect(parcelScene.context.entities.size).eq(2)

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
      -200,
      236,
      ({ parcelScenePromise, sceneHost, ensureNoErrors }) => {
        it('should have a transform component', async () => {
          // TODO: ECS
          // sceneHost.update(0)
          // expect(L.logs[0]).to.eq('start')
          // let graph = L.ret.getCachedGraph()
          // sceneHost.update(0)
          // graph = L.ret.getCachedGraph()
          // const box = graph.children.find(node => node.tag === 'box')
          // expect(!!box).to.eq(true, 'Box must exist')
          // expect(typeof box.attrs.onClick).to.eq('string', 'onClick must be a string')
          // expect((box.attrs.onClick as string).startsWith('#')).to.eq(false, 'onclick must not start with #')
          // const nonce = Math.random()
          // L.ret.fireEvent({
          //   type: 'uuidEvent',
          //   data: {
          //     uuid: box.attrs.onClick,
          //     payload: { nonce }
          //   }
          // })
          // expect(L.logs[1]).to.eq(JSON.stringify({ nonce }))
          // expect(L.logs.length).to.eq(2, 'We must have two logs only')
          // L.ensureNoErrors()
        })
      },
      true
    )

    testScene(
      -100,
      108,
      ({ parcelScenePromise, sceneHost, ensureNoErrors }) => {
        it('should have a transform component', async () => {
          // TODO: ECS
          // sceneHost.update(0)
          // const camera = L.ret.dcl['camera']
          // L.ret.fireEvent({
          //   type: 'rotationChanged',
          //   data: {
          //     quaternion: { x: 1, y: 1, z: 1 }
          //   }
          // })
          // sceneHost.update(1)
          // expect(camera.rotation.x).to.eq(1)
          // expect(camera.rotation.y).to.eq(1)
          // expect(camera.rotation.z).to.eq(1)
        })
      },
      true
    )

    testScene(
      -100,
      109,
      ({ parcelScenePromise, sceneHost, ensureNoErrors }) => {
        it('should have a transform component', async () => {
          // TODO: ECS
          // sceneHost.update(0)
          // const input = L.ret.dcl['input']
          // const clickFuture = future()
          // input.subscribe('BUTTON_A_DOWN', e => {
          //   clickFuture.resolve(e)
          // })
          // L.ret.fireEvent({
          //   type: 'pointerDown',
          //   data: {
          //     from: {},
          //     direction: {},
          //     pointerId: 1,
          //     length: 0
          //   }
          // })
          // sceneHost.update(1)
          // const data = await clickFuture
          // expect(data.from.toString()).to.eq('{ x: 0, y: 0, z: 0 }')
          // expect(data.direction.toString()).to.eq('{ x: 0, y: 0, z: 0 }')
          // expect(data.length).to.eq(0)
          // expect(data.pointerId).to.eq('PRIMARY')
          // expect(input.state[Pointer.PRIMARY].BUTTON_A_DOWN).to.eq(true)
        })
      },
      true
    )
  })

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
        entity.add(new TheDispoCompo())
        expect(entity.get(TheDispoCompo)).to.be.instanceOf(TheDispoCompo)
        expect(entity.components[componentName]).to.be.instanceOf(TheDispoCompo)
        expect(entity.components[componentName]).to.eq(entity.get(TheDispoCompo))
        entity.remove(entity.get(TheDispoCompo))
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
      it('should have symbol in the class as long as the instance', () => {
        const inst = new TheCompo()

        expect(inst).to.be.instanceOf(TheCompo)
        expect(getComponentName(inst)).to.eq(componentName)
        expect(getComponentName(TheCompo)).to.eq(componentName)
      })

      it('should add the component to an entity using the indicated component name', () => {
        const entity = new Entity()
        entity.add(new TheCompo())
        expect(entity.get(TheCompo)).to.be.instanceOf(TheCompo)
        expect(entity.components[componentName]).to.be.instanceOf(TheCompo)
        expect(entity.components[componentName]).to.eq(entity.get(TheCompo))
        entity.remove(entity.get(TheCompo))
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
        const transform = entity.getOrCreate(Transform)
        expect(transform).to.be.instanceOf(Transform)
        expect(Object.values(entity.components)).to.contain(transform)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.remove(transform)
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
        entity.set(component)
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
        entity.set(component)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        entity.remove(component)

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
        entity.set(component)
        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        entity.set(component2)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        engine.removeEntity(entity)
        expect(group.entities.length).to.eq(0)
      })

      it('adding the 2nd component after the entity already has one should add it to the ComponentGroup', () => {
        const entity = new Entity()
        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.set(component)
        expect(group.entities.length).to.eq(0)
        engine.addEntity(entity)
        entity.set(component2)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.remove(component)
        expect(group.entities.length).to.eq(0)
        engine.removeEntity(entity)
      })

      it('a entity in the engine should modify the groups when changing components', () => {
        const entity = new Entity()
        engine.addEntity(entity)

        expect(group.entities.length).to.eq(0)

        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.set(component)
        entity.set(component2)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        engine.removeEntity(entity)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group')

        entity.remove(component2)

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
        entity.set(component)
        engine.addEntity(entity)
        entity.set(component2)
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
        entity.set(component)
        engine.addEntity(entity)
        entity.set(component2)
        const group = engine.getComponentGroup(AComponent, AnotherComponent)
        expect(group.entities.length).to.eq(1)
        expect(group.entities[0]).to.eq(entity)
        entity.remove(component)
        expect(group.entities.length).to.eq(0)
        engine.removeEntity(entity)
        engine.removeComponentGroup(group)
      })

      it('a entity in the engine should modify the groups when changing components', () => {
        const entity = new Entity()
        engine.addEntity(entity)

        const component = new AComponent()
        const component2 = new AnotherComponent()
        entity.set(component)
        entity.set(component2)

        const group = engine.getComponentGroup(AComponent, AnotherComponent)

        expect(group.entities.length).to.eq(1, 'the entity should be added to the group')
        expect(group.entities[0]).to.eq(entity)

        engine.removeEntity(entity)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group')

        entity.remove(component2)

        expect(group.entities.length).to.eq(0, 'the entity should be removed from the group 2')

        engine.removeComponentGroup(group)
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
      ent.set(model)
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
      ent.set(model)
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
        engine.removeEntity(engine['_entities'][key], true)
      }

      const ent1 = new Entity()
      const ent2 = new Entity()
      const ent3 = new Entity()

      ent2.setParent(ent1)
      ent3.setParent(ent2)

      engine.addEntity(ent1)
      engine.addEntity(ent2)
      engine.addEntity(ent3)

      engine.removeEntity(ent1, true)

      expect(Object.keys(engine['_entities']).length).to.eq(0)
    })
  })

  loadTestParcel('must fail', -200, 238, function(_root, _futureScene, futureWorker) {
    it('must fail and that must appear in the logs', async () => {
      const worker = await futureWorker
      const scriptingHost = await worker.system
      scriptingHost.unmounted
    })
  })

  describe('ecs events', () => {
    it('should call event only once when event is added before adding entity', async () => {
      const uuidEventFuture = future()
      const uuidEventFutureComponent = future()

      let counter = 0

      const entity = new Entity()

      const clicker = new OnClick(event => {
        counter += 1
        uuidEventFutureComponent.resolve(event)
      })

      entity.set(clicker)

      engine.addEntity(entity)

      engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
        uuidEventFuture.resolve(event.uuid)
      })

      const event = new UUIDEvent()

      event.uuid = clicker.uuid
      // event.type = 'OnClick'
      event.payload = { data: { from: {}, direction: {}, pointerId: 1, length: 0 } }

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

      entity.set(clicker)

      engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
        uuidEventFuture.resolve(event.uuid)
      })

      const event = new UUIDEvent()

      event.uuid = clicker.uuid
      // event.type = 'OnClick'
      event.payload = { data: { from: {}, direction: {}, pointerId: 1, length: 0 } }

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

      entity.set(clicker)

      engine.addEntity(entity)

      entity.remove(clicker)

      engine.eventManager.addListener(UUIDEvent, uuidEventFuture, event => {
        uuidEventFuture.resolve(event.uuid)
      })

      const event = new UUIDEvent()

      event.uuid = clicker.uuid

      engine.eventManager.fireEvent(event)

      expect(await uuidEventFuture).to.eq(clicker.uuid, 'event should be fired')

      setTimeout(() => uuidEventFutureComponent.resolve(0), 100)

      await uuidEventFutureComponent
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
})

function anglesAreEqual(e1: Vector3, e2: Vector3) {
  const angle = Quaternion.Angle(Quaternion.Euler(e1.x, e1.y, e1.z), Quaternion.Euler(e2.x, e2.y, e2.z))
  console['log']('angle between', e1, e2, 'is', angle)
  return Math.abs(angle) < 1e-3
}
