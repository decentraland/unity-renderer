// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { expect } from 'chai'
// import { BoxEntity } from 'engine/entities/BoxEntity'

// import { enableVisualTests, saveScreenshot } from '../testHelpers'
// import { future } from 'fp-future'
// import { ScriptableScene, MemoryTransport, createElement } from 'decentraland-api/src'
// import { SystemEntity } from 'engine/entities/SystemEntity'
// import { scene } from 'engine/renderer'

// describe('clicks', () => {
//   enableVisualTests('Click interaction', function(root) {
//     let box: BoxEntity

//     it('create test scene', () => {
//       const id = 'interactive'
//       const rotation = { x: 0, y: 0, z: 0 }

//       box = new BoxEntity('http://localhost:8080/', {})
//       box.setAttributes({ id, rotation, position: { x: 2, y: 2, z: 5 } })

//       expect(box.attrs.rotation).to.deep.equal({ x: 0, y: 0, z: 0 })

//       box.addEventListener('click', e => {
//         box.setAttributes({ rotation: { x: 45, y: 0, z: 45 } })
//       })

//       box.setParent(root)
//     })

//     saveScreenshot(`click-interaction-before.png`, {
//       from: [1, 1.5, 0],
//       lookAt: [1, 2, 5]
//     })

//     it('click event should change color of BoxEntity', () => {
//       box.dispatchClick(1)
//       expect(box.attrs.rotation).to.deep.eq({ x: 45, y: 0, z: 45 })
//     })

//     saveScreenshot(`click-interaction-after.png`, {
//       from: [1, 1.5, 0],
//       lookAt: [1, 2, 5]
//     })
//   })

//   describe('scene lifecycle hooks', () => {
//     const sceneDidMount = future()
//     const receivedProps = future()

//     const didTriggerFirstHandler = future()
//     const didTriggerSecondHandler = future()

//     const sceneWillUnmount = future()

//     let updateCount = 0
//     let renderCount = 0

//     class TestScene extends ScriptableScene {
//       async sceneDidMount() {
//         sceneDidMount.resolve(true)
//       }

//       shouldSceneUpdate(props) {
//         receivedProps.resolve(props)
//         return !!props.shouldRender
//       }

//       async sceneDidUpdate() {
//         updateCount++
//       }

//       sceneWillUnmount() {
//         sceneWillUnmount.resolve(true)
//       }

//       async render() {
//         renderCount++

//         if (renderCount === 1) {
//           return <box onClick={x => didTriggerFirstHandler.resolve(x)} />
//         } else {
//           return <box onClick={x => didTriggerSecondHandler.resolve(x)} />
//         }
//       }
//     }

//     const transport = MemoryTransport()

//     it('creates the instance', () => {
//       // tslint:disable-next-line:no-unused-expression
//       new TestScene(transport.client)
//     })

//     let systemEntity: SystemEntity
//     let oldId = null

//     it('starts the system', async () => {
//       systemEntity = new SystemEntity('system', '', {})
//       const scriptingHost = await systemEntity.startSystem(transport.server)

//       systemEntity.system.resolve(scriptingHost)

//       // keep this to avoid regressions
//       await systemEntity.system

//       scene.addTransformNode(systemEntity)
//     })

//     describe('hooks', () => {
//       it('sceneDidMount', async () => {
//         await sceneDidMount
//         expect(updateCount).to.eq(1, 'Update counter')
//         expect(renderCount).to.eq(1, 'Render counter')
//       })

//       it('awaits for the first click', async () => {
//         const box: BoxEntity = systemEntity.getDescendants(false).find($ => $ instanceof BoxEntity) as any

//         expect(typeof box.attrs.onClick).to.eq('string')

//         oldId = box.attrs.onClick

//         box.dispatchClick(1)

//         expect((await didTriggerFirstHandler).pointerId).to.eq(1)

//         const sentProps = {
//           shouldRender: true,
//           obj: { x: 3, y: 1, z: 0 }
//         }

//         systemEntity.setAttributes(sentProps)
//       })

//       it('awaits for the second click', async () => {
//         const box: BoxEntity = systemEntity.getDescendants(false).find($ => $ instanceof BoxEntity) as any

//         expect(typeof box.attrs.onClick).to.eq('string')
//         expect(box.attrs.onClick).to.not.eq(oldId)

//         box.dispatchClick(2)

//         expect((await didTriggerSecondHandler).pointerId).to.eq(2)
//       })

//       it('unmounts the scene', async () => {
//         systemEntity.dispose()
//         await sceneWillUnmount
//       })
//     })
//   })
// })
