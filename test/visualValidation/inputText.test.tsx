// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { enableVisualTests, saveScreenshot } from '../testHelpers'
// import { createEntity } from 'engine/entities'
// import { createElement } from 'decentraland-api/src'

// enableVisualTests('InputText visual validation', function(root) {
//   it('creates a test scene', async () => {
//     const jsx = (
//       <scene>
//         <input-text
//           width={1}
//           height={0.4}
//           placeholder="Type something..."
//           fontSize={24}
//           background="#FFFFFF"
//           focusedBackground="#FFFFFF"
//           color="#000000"
//           position={{ x: 0, y: 2, z: 0 }}
//         />

//         <input-text
//           width={1}
//           height={0.4}
//           placeholder="Type something..."
//           fontSize={24}
//           background="#FFFFFF"
//           focusedBackground="#FFFFFF"
//           color="#000000"
//           position={{ x: 0, y: 1.6, z: 0 }}
//           value="test"
//         />
//       </scene>
//     )

//     createEntity(jsx, '/', null).setParent(root)
//   })

//   saveScreenshot(`inpuText.png`, {
//     from: [0, 2, -2],
//     lookAt: [0, 2, 2]
//   })
// })
