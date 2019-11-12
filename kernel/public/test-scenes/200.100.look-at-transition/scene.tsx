// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene, Vector3Component } from 'decentraland-api/src'
// import { Axis } from 'dcl/entities/debug/AxisEntity'

// export interface IState {
//   position: Vector3Component
//   timer: any
// }

// export default class BirdsScene extends ScriptableScene<any, IState> {
//   constructor(props: any) {
//     super(props)
//     const timer = setInterval(() => this.update(), 500)
//     this.state = { position: { x: 0, y: 2, z: 0 }, timer }
//   }

//   update() {
//     const { x, y, z } = this.state.position
//     if (x === 5) {
//       clearInterval(this.state.timer)
//       return
//     }
//     this.setState({ position: { x: x + 1, y, z: z + 1 } })
//   }

//   async render() {
//     const { position } = this.state
//     return (
//       <scene>
//         <Axis
//           position={position}
//           lookAt={position}
//           transition={{ position: { duration: 500 }, lookAt: { duration: 500 } }}
//         />
//       </scene>
//     )
//   }
// }
