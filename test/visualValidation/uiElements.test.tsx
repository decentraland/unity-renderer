// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { saveScreenshot, testScene } from '../testHelpers'
// import { future } from 'fp-future'
// import { createEntity } from 'engine/entities'
// import { assert } from 'chai'
// import { createElement, ScriptableScene } from 'decentraland-api/src'

// const didRender = future()
// const ATLAS_PATH = '/images/ui-profile.png'

// export default class UIScene extends ScriptableScene<any, any> {
//   // tslint:disable-next-line:prefer-function-over-method
//   sceneDidUpdate() {
//     didRender.resolve(true)
//   }
//   // tslint:disable-next-line:prefer-function-over-method
//   async render() {
//     didRender.resolve(true)
//     return <scene />
//   }
// }

// testScene('UI elements', UIScene, (root, { parcelScenePromise, classPromise }) => {
//   let jsx = null
//   let entity = null

//   it('check if scene rendered', async () => {
//     const systemEntity = await parcelScenePromise

//     Object.assign(systemEntity.context, {
//       isInternal: true,
//       hideAxis: true
//     })

//     assert(systemEntity.entityController.renderEntity, 'it has a render entity')

//     await didRender
//   })

//   it('make scene internal and hide axis', async () => {
//     const systemEntity: any = await parcelScenePromise

//     assert(systemEntity.context.isInternal, 'it is an internal scene')
//     assert(systemEntity.context.hideAxis, 'axis is hidden')
//   })

//   it('render UIText', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui position={{ x: 0, y: 1, z: 1 }}>
//           <ui-text
//             color="green"
//             value="Testing <ui-text> UI element"
//             fontSize={35}
//             fontWeight="bold"
//             outlineWidth={5}
//             outlineColor="white"
//           />
//         </ui>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-text.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove UIText', () => {
//     entity.dispose()
//   })

//   it('render UIInputText', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui position={{ x: 0, y: 1, z: 1 }}>
//           <ui-input color="white" background="black" placeholder="Type something..." fontSize={25} thickness={1} />
//         </ui>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-input.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove UIInputText', () => {
//     entity.dispose()
//   })

//   it('render UISlider', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui position={{ x: 0, y: 1, z: 1 }}>
//           <ui-slider height="30px" width="400px" minimum={0} maximum={10} value={3} />
//         </ui>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-slider.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove UISlider', () => {
//     entity.dispose()
//   })

//   it('render UIImage', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui position={{ x: 0, y: 1, z: 1 }}>
//           <ui-image
//             id="avatar"
//             width="130px"
//             height="120px"
//             source={ATLAS_PATH}
//             sourceLeft="347px"
//             sourceTop="1px"
//             sourceWidth="145px"
//             sourceHeight="130px"
//             top="-80px"
//           />
//         </ui>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-image.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove UIImage', () => {
//     entity.dispose()
//   })

//   it('render complex UI with rect and stack containers', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui position={{ x: 0, y: 1, z: 1 }}>
//           <ui-container-rect width="300px" height="400px" cornerRadius={40} background="white">
//             <ui-image
//               id="avatar"
//               width="130px"
//               height="120px"
//               source={ATLAS_PATH}
//               sourceLeft="347px"
//               sourceTop="1px"
//               sourceWidth="145px"
//               sourceHeight="130px"
//               top="-80px"
//             />
//             <ui-text color="#000" value="Evaristo" fontSize={24} />
//             <ui-text color="#000" value="😀 😈 😎" fontSize={24} top="40px" />
//             <ui-text color="#555" value="Some short description for Evaristo" fontSize={16} top="80px" />
//             <ui-container-rect width="450px" top="150px" left="-100px">
//               <ui-image
//                 id="wink"
//                 width="50px"
//                 height="50px"
//                 source={ATLAS_PATH}
//                 sourceLeft="347px"
//                 sourceTop="132px"
//                 sourceWidth="48px"
//                 sourceHeight="48px"
//               />
//               <ui-image
//                 id="friend"
//                 width="50px"
//                 height="50px"
//                 source={ATLAS_PATH}
//                 sourceLeft="396px"
//                 sourceTop="132px"
//                 sourceWidth="48px"
//                 sourceHeight="48px"
//                 left="55px"
//               />
//               <ui-container-rect left="130px">
//                 <ui-image
//                   id="mute"
//                   width="60px"
//                   height="50px"
//                   source={ATLAS_PATH}
//                   sourceLeft="347px"
//                   sourceTop="181px"
//                   sourceWidth="52px"
//                   sourceHeight="48px"
//                 />
//                 <ui-image
//                   id="block"
//                   width="60px"
//                   height="50px"
//                   source={ATLAS_PATH}
//                   sourceLeft="400px"
//                   sourceTop="181px"
//                   sourceWidth="52px"
//                   sourceHeight="48px"
//                   left="65px"
//                 />
//               </ui-container-rect>
//             </ui-container-rect>
//           </ui-container-rect>
//         </ui>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-complex.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove complex UI', () => {
//     entity.dispose()
//   })

//   it('render complex UI with fullscreen texture', async () => {
//     const systemEntity = await parcelScenePromise

//     jsx = (
//       <scene>
//         <ui-fullscreen>
//           <ui-container-rect width="300px" height="400px" cornerRadius={40} background="white">
//             <ui-image
//               id="avatar"
//               width="130px"
//               height="120px"
//               source={ATLAS_PATH}
//               sourceLeft="347px"
//               sourceTop="1px"
//               sourceWidth="145px"
//               sourceHeight="130px"
//               top="-80px"
//             />
//             <ui-text color="#000" value="Evaristo" fontSize={24} />
//             <ui-text color="#000" value="😀 😈 😎" fontSize={24} top="40px" />
//             <ui-text color="#555" value="Some short description for Evaristo" fontSize={16} top="80px" />
//             <ui-container-rect width="450px" top="150px" left="-100px">
//               <ui-image
//                 id="wink"
//                 width="50px"
//                 height="50px"
//                 source={ATLAS_PATH}
//                 sourceLeft="347px"
//                 sourceTop="132px"
//                 sourceWidth="48px"
//                 sourceHeight="48px"
//               />
//               <ui-image
//                 id="friend"
//                 width="50px"
//                 height="50px"
//                 source={ATLAS_PATH}
//                 sourceLeft="396px"
//                 sourceTop="132px"
//                 sourceWidth="48px"
//                 sourceHeight="48px"
//                 left="55px"
//               />
//               <ui-container-rect left="130px">
//                 <ui-image
//                   id="mute"
//                   width="60px"
//                   height="50px"
//                   source={ATLAS_PATH}
//                   sourceLeft="347px"
//                   sourceTop="181px"
//                   sourceWidth="52px"
//                   sourceHeight="48px"
//                 />
//                 <ui-image
//                   id="block"
//                   width="60px"
//                   height="50px"
//                   source={ATLAS_PATH}
//                   sourceLeft="400px"
//                   sourceTop="181px"
//                   sourceWidth="52px"
//                   sourceHeight="48px"
//                   left="65px"
//                 />
//               </ui-container-rect>
//             </ui-container-rect>
//           </ui-container-rect>
//         </ui-fullscreen>
//       </scene>
//     )

//     entity = createEntity(jsx, '/', systemEntity.context).setParent(root)
//   })

//   saveScreenshot(`ui-fullscreen-complex.png`, { from: [0, 1, 0], lookAt: [0, 1, 1] })

//   it('remove complex UI with fullsreen texture', () => {
//     entity.dispose()
//   })
// })
