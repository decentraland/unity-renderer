// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { testScene } from '../testHelpers'
// import { future } from 'fp-future'
// import { assert, expect } from 'chai'
// import { createElement, ScriptableScene } from 'decentraland-api/src'
// import { parcelLimits } from 'config'

// const didRender = future()

// class TestComponent extends ScriptableScene<{ complexity: number }> {
//   sceneDidUpdate() {
//     didRender.resolve(true)
//   }
//   async render() {
//     return (
//       <scene>
//         <cylinder id="cylinder" segmentsRadial={this.props.complexity || 1} />
//       </scene>
//     )
//   }
// }

// testScene('query scene limits', TestComponent, (root, { parcelScenePromise, classPromise }) => {
//   it('set cylinder complexity to 10', async () => {
//     const systemEntity = await parcelScenePromise
//     assert(systemEntity.entityController.renderEntity, 'it has a render entity')

//     systemEntity.setAttributes({ complexity: 10 })

//     await didRender
//   })

//   it('check scene limits', async () => {
//     const systemEntity = await parcelScenePromise
//     const limits = await systemEntity.entityController.querySceneLimits()

//     expect(limits.entities).to.eq(parcelLimits.entities)
//     expect(limits.triangles).to.eq(parcelLimits.triangles)
//     expect(limits.bodies).to.eq(parcelLimits.bodies)
//     expect(limits.textures).to.eq(parcelLimits.textures)
//     expect(limits.materials).to.eq(parcelLimits.materials)
//   })

//   it('check remaining triangles limit', async () => {
//     const systemEntity = await parcelScenePromise
//     const metrics = await systemEntity.entityController.querySceneMetrics()
//     const limits = await systemEntity.entityController.querySceneLimits()
//     const limitsLeft = {
//       triangles: limits.triangles - metrics.triangles,
//       bodies: limits.bodies - metrics.bodies,
//       entities: limits.entities - metrics.entities,
//       materials: limits.materials - metrics.materials,
//       textures: limits.textures - metrics.textures
//     }

//     assert.isAbove(limitsLeft.triangles, 9000, 'Triangle limits not exceeded.')
//   })
// })
