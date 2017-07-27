// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene } from 'decentraland-api/src'

// const RENDER_HZ = 6
// const interval = 1000 / RENDER_HZ

// export default class RollerCoaster extends ScriptableScene<any, { time: number }> {
//   state = { time: 0 }

//   timeout = setInterval(() => {
//     this.setState({
//       time: performance.now() * 0.0001
//     })
//   }, interval)

//   sceneWillUnmount() {
//     clearInterval(this.timeout)
//   }

//   async render() {
//     const { time } = this.state

//     const size = 5

//     const x = Math.cos(time) * Math.cos(time) * size
//     const y = Math.cos(time) * Math.sin(time) * size
//     const z = Math.sin(time) * size * 8

//     return (
//       <scene position={{ x: 5, y: 4, z: 30 }}>
//         <entity
//           id="train"
//           position={{ x, y, z }}
//           rotation={{ x: Math.cos(time) * 40, y: Math.sin(time) * 40, z: 0 }}
//           transition={{
//             position: { duration: interval },
//             rotation: { duration: interval }
//           }}
//         >
//           <box position={{ x: 0, y: -1, z: 0 }} color="#000000" scale={{ x: 3, y: 0.4, z: 5 }} />
//           <box position={{ x: 1.5, y: 0, z: 0 }} color="#ff0000" scale={{ x: 0.2, y: 1, z: 5 }} />
//           <box position={{ x: -1.5, y: 0, z: 0 }} color="#ffff00" scale={{ x: 0.2, y: 1, z: 5 }} />

//           <box position={{ x: 0, y: 0, z: 2.5 }} color="#00ff00" scale={{ x: 3, y: 1, z: 0.2 }} />
//           <box position={{ x: 0, y: 0, z: -2.5 }} color="#0000ff" scale={{ x: 3, y: 1, z: 0.2 }} />
//         </entity>
//       </scene>
//     )
//   }
// }
