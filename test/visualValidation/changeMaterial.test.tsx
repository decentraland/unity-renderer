// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { future } from 'fp-future'
// import { testScene, saveScreenshot } from '../testHelpers'
// import { createElement, ScriptableScene } from 'decentraland-api/src'
// import { expect } from 'chai'

// const didSetMaterial = future()
// class TestComponent extends ScriptableScene<{ material: string }> {
//   async render() {
//     if (this.props.material) {
//       didSetMaterial.resolve(this.props.material)
//     }
//     return (
//       <scene>
//         <material id="mat1" albedoColor={'#0000ff'} metallic={0.0} roughness={1} emissiveColor={'#000000'} />
//         <material id="mat2" albedoColor={'#cccccc'} metallic={0.5} roughness={0.5} emissiveColor={'#ffeeaa'} />
//         <sphere position={{ x: 2, y: 1, z: 2 }} material={this.props.material || '#mat1'} />
//       </scene>
//     )
//   }
// }

// describe('change material wrapper', () => {
//   testScene('change material', TestComponent, (root, { parcelScenePromise, classPromise }) => {
//     saveScreenshot('changeMaterial.0.png', { lookAt: [2, 1, 4], from: [0, 0, 0] })

//     it('changes the color of a material', async () => {
//       const newMaterialName = '#mat2'
//       const script = await parcelScenePromise
//       script.setAttributes({ material: newMaterialName })
//       const settedMaterial = await didSetMaterial
//       expect(settedMaterial).to.eq(newMaterialName)
//     })

//     saveScreenshot('changeMaterial.1.png', { lookAt: [2, 1, 4], from: [0, 0, 0] })
//   })
// })
