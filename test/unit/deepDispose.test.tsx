// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { testScene } from '../testHelpers'
// import { ScriptableScene, createElement } from 'decentraland-api/src'
// import { future } from 'fp-future'
// import { BoxEntity } from 'engine/entities/BoxEntity'
// import { expect } from 'chai'
// import { scene } from 'engine/renderer'

// const boxId = 'aLonelyBox'

// class Game extends ScriptableScene {
//   async render() {
//     return (
//       <scene>
//         <material id="testMaterial" albedoColor="#ff0000" />
//         <box position={{ x: 3, y: 0.5, z: 5 }} material="#testMaterial" id={boxId} />
//       </scene>
//     )
//   }
// }

// testScene("releasing a system's EntityComponent", Game, (_root, { parcelScenePromise }) => {
//   const materialDisposedFuture = future()
//   let boxEntity: BoxEntity
//   let boxMesh: BABYLON.TransformNode

//   it('getObject3D(content) == entityController.renderEntity', async () => {
//     const parcelScene = await parcelScenePromise
//     expect(parcelScene.getObject3D('content')).to.eq(parcelScene.entityController.renderEntity)
//   })

//   it('parcelScene is in the scene', async () => {
//     const parcelScene = await parcelScenePromise
//     expect(scene.getTransformNodesByID(parcelScene.id).length).to.eq(1)
//   })

//   it('box is in the scene', async () => {
//     const boxQuery = scene.getTransformNodesByID(boxId)
//     expect(boxQuery.length).to.eq(1)
//     boxEntity = boxQuery[0] as any
//     boxMesh = boxEntity.getObject3D('mesh')
//     expect(scene.meshes.find($ => $ === boxMesh)).to.eq(boxMesh, 'box mesh is in the scene')
//   })

//   it('material is not disposed', async () => {
//     const parcelScene = await parcelScenePromise

//     const material = parcelScene.context.getMaterial('#testMaterial')
//     expect(!!material).to.eq(true, 'material not found')
//     material.onDisposeObservable.add(() => materialDisposedFuture.resolve(material))
//   })

//   it('parcelSceneEntity.dispose()', async () => {
//     const parcelScene = await parcelScenePromise
//     parcelScene.dispose()
//   })

//   it('parcelScene is not in the scene anymore', async () => {
//     const parcelScene = await parcelScenePromise
//     expect(scene.getTransformNodesByID(parcelScene.id).length).to.eq(0)
//   })

//   it('box is not in the scene anymore', async () => {
//     const boxQuery = scene.getTransformNodesByID(boxId)
//     expect(boxQuery.length).to.eq(0)

//     expect(!boxEntity.getObject3D('mesh')).to.eq(true)
//     expect(scene.meshes.find($ => $ === boxMesh)).to.eq(undefined, 'box mesh not is in the scene')
//   })

//   it('material got disposed', async () => {
//     await materialDisposedFuture
//   })

//   it('getObject3D(content) == entityController.renderEntity', async () => {
//     const parcelScene = await parcelScenePromise
//     expect(!parcelScene.getObject3D('content')).to.eq(true)
//     expect(!parcelScene.entityController.renderEntity).to.eq(true)
//   })
// })
