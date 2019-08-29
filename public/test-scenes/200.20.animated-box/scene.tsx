// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { Script, inject, EntityController, createElement } from 'decentraland-api/src'

// export default class AudioPlayer extends Script {
//   @inject('EntityController') entityController: EntityController | null = null

//   boxSteps = [
//     {
//       position: { x: 5, y: 3, z: 5 },
//       rotation: { x: 45, y: 0, z: 0 },
//       color: 'red'
//     },
//     {
//       position: { x: 3, y: 3, z: 3 },
//       rotation: { x: 45, y: 45, z: 0 },
//       color: 'green'
//     },
//     {
//       position: { x: 3, y: 5, z: 3 },
//       rotation: { x: 45, y: 45, z: 45 },
//       color: 'blue'
//     },
//     {
//       position: { x: 5, y: 5, z: 5 },
//       rotation: { x: 0, y: 0, z: 0 },
//       color: 'yellow'
//     }
//   ]

//   platformSteps = [
//     {
//       position: { x: 0, y: 0, z: 0 },
//       color: 'yellow'
//     },
//     {
//       position: { x: 0, y: 2, z: 0 },
//       color: 'yellow'
//     },
//     {
//       position: { x: 0, y: 5, z: 0 },
//       color: 'blue'
//     },
//     {
//       position: { x: 0, y: 1, z: 0 },
//       color: 'yellow'
//     }
//   ]

//   currentStep = 0

//   async systemDidEnable() {
//     await this.render()

//     setInterval(async () => {
//       await this.render()
//     }, 1000)
//   }

//   async render() {
//     this.currentStep++
//     this.currentStep = this.currentStep % this.boxSteps.length

//     const boxState = this.boxSteps[this.currentStep]
//     const platformState = this.platformSteps[this.currentStep]

//     await this.entityController!.render(
//       <scene>
//         <box
//           position={boxState.position}
//           color={boxState.color}
//           rotation={boxState.rotation}
//           transition={{
//             position: {
//               duration: 1000,
//               timing: 'ease-in'
//             },
//             color: {
//               duration: 500
//             },
//             rotation: {
//               duration: 1000,
//               timing: 'circular-inout'
//             }
//           }}
//         />
//         <box
//           position={platformState.position}
//           color={platformState.color}
//           scale={{ x: 10, y: 0.1, z: 10 }}
//           transition={{
//             position: { duration: 1000, timing: 'circular-inout' },
//             color: { duration: 1000 },
//             rotation: { duration: 1000 }
//           }}
//         />
//       </scene>
//     )
//   }
// }
