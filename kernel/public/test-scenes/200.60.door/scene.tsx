// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene } from 'decentraland-api/src'

// const loopDuration = 3
// const RENDER_HZ = 6

// const interval = 1000 / RENDER_HZ

// const radius = 2

// function componentToHex(c: number) {
//   let hex = ((Math.max(Math.min(1, c), 0) * 255) | 0).toString(16)
//   return hex.length === 1 ? '0' + hex : hex
// }

// function rgbToHex(r: number, g: number, b: number) {
//   return '#' + componentToHex(r) + componentToHex(g) + componentToHex(b)
// }

// const getSphere = (time: number, id: number, count: number) => {
//   const ix = (id * Math.PI) / count
//   const a = (time * 2 * Math.PI) / loopDuration
//   const x = radius * Math.cos(a + ix)
//   const y = 0
//   const z = 0
//   const s = 0.5 + 0.5 * Math.sin(a + (id * 2 * Math.PI) / count)

//   const scale = 0.5 + 0.5 * s
//   const rotationY = (id * 360) / count
//   const rotationZ = a + (id * 180) / count

//   const r = Math.sin(ix * 2)
//   const g = Math.cos(ix * 2)
//   const b = Math.cos(ix * 2) * Math.sin(ix * 2) * 2

//   return (
//     <entity
//       rotation={{ x: 0, y: rotationY, z: rotationZ }}
//       key={id.toString()}
//       transition={{ rotation: { duration: interval } }}
//     >
//       <box
//         position={{ x, y, z }}
//         scale={{ x: 0.15 * scale, y: 0.3 * scale, z: 0.3 * scale }}
//         color={rgbToHex(r, g, b)}
//         transition={{
//           position: { duration: interval },
//           scale: { duration: interval },
//           rotation: { duration: interval }
//         }}
//       />
//     </entity>
//   )
// }

// export default class FlyingSpheres extends ScriptableScene<any, { time: number }> {
//   state = { time: 0 }

//   timeout = setInterval(() => {
//     this.setState({
//       time: (0.001 * (performance.now() - this.state.time)) % loopDuration
//     })
//   }, interval)

//   async render() {
//     const { time } = this.state

//     const count = 30
//     const spheres = new Array(count).fill(null).map((_, i) => getSphere(time, i, count))

//     const rotationY = (time * 360) / loopDuration

//     return (
//       <scene
//         position={{ x: 5, y: 2, z: 5 }}
//         rotation={{ x: 0, y: rotationY, z: 0 }}
//         transition={{ rotation: { duration: interval } }}
//       >
//         {spheres}
//       </scene>
//     )
//   }
// }
