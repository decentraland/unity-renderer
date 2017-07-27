// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { testScene } from '../testHelpers'
// import { future } from 'fp-future'
// import { EntityEvent } from 'engine/entities/utils/EntityEvent'
// import { assert } from 'chai'
// import { createElement, ScriptableScene } from 'decentraland-api/src'
// import { engineMicroQueue } from 'engine/renderer'
// import sinon from 'sinon'

// const sceneWillUnmount = future()
// const didRender = future()
// const didRender2 = future()

// class TestComponent extends ScriptableScene<{ complexity: number }> {
//   sceneWillUnmount() {
//     sceneWillUnmount.resolve(true)
//   }

//   sceneDidUpdate() {
//     didRender.resolve(true)
//   }
//   async render() {
//     if (this.props.complexity === 1000) {
//       didRender2.resolve(true)
//       return (
//         <scene>
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//           <cylinder segmentsRadial={this.props.complexity || 1} />
//         </scene>
//       )
//     }

//     return (
//       <scene>
//         <cylinder segmentsRadial={this.props.complexity || 1} />
//       </scene>
//     )
//   }
// }

// testScene('verify parcel limits', TestComponent, (_, { parcelScenePromise }) => {
//   const didTriggerLimits = future()

//   let spy: sinon.SinonSpy
//   let spyFlush: sinon.SinonSpy

//   before('hooks to the microtask execution request', () => {
//     spy = sinon.spy(engineMicroQueue, 'requestFlushMicroTaskQueue')
//     spyFlush = sinon.spy(engineMicroQueue, 'flushMicroTaskQueue')
//   })

//   after('release the spy', () => {
//     spy.restore()
//     spyFlush.restore()
//   })

//   it('check limits', async () => {
//     engineMicroQueue.flushMicroTaskQueue()

//     const systemEntity = await parcelScenePromise

//     assert(systemEntity.entityController.renderEntity, 'it has a render entity')

//     systemEntity.addEventListener('limitsExceeded', (evt: EntityEvent) => {
//       didTriggerLimits.resolve(evt.detail)
//     })

//     spy.resetHistory()
//     spyFlush.resetHistory()

//     systemEntity.setAttributes({ complexity: 1000 })

//     await didRender2
//   })

//   it('should trigger limitsExceeded', async () => {
//     await didTriggerLimits
//   })

//   it.skip('requestFlushMicroTaskQueue should have been called', async () => {
//     assert(spy.called, "wasn't called")
//   })

//   it('flushMicroTaskQueue should have been called', () => {
//     assert(spyFlush.called, "wasn't called")
//   })

//   it('shoud have been removed due exceeded limits', async () => {
//     await sceneWillUnmount
//     const systemEntity = await parcelScenePromise
//     assert(!systemEntity.entityController.renderEntity, 'render entity was removed')
//     systemEntity.dispose()
//   })
// })
