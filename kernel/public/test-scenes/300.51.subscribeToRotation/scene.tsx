// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene } from 'decentraland-api/src'

// function Axis(props: JSX.BaseEntity) {
//   return (
//     <entity {...props}>
//       <box scale={{ x: 1, y: 0.01, z: 0.01 }} color="#ff0000" position={{ x: 0.5, y: 0, z: 0 }} />
//       <box scale={{ x: 0.01, y: 1, z: 0.01 }} color="#00ff00" position={{ x: 0, y: 0.5, z: 0 }} />
//       <box scale={{ x: 0.01, y: 0.01, z: 1 }} color="#0000ff" position={{ x: 0, y: 0, z: 0.5 }} />
//       <text value="X" color="#ff0000" position={{ x: 1, y: 0.1, z: 0 }} fontFamily="Helvetica" />
//       <text value="Y" color="#00ff00" position={{ x: 0, y: 1.1, z: 0 }} fontFamily="Helvetica" />
//       <text value="Z" color="#0000ff" position={{ x: 0, y: 0.1, z: 1 }} fontFamily="Helvetica" />
//     </entity>
//   )
// }

// export default class BoxFollower extends ScriptableScene {
//   state = {
//     rotation: { x: 0, y: 0, z: 0 }
//   }

//   async sceneDidMount() {
//     this.subscribeTo('rotationChanged', e => {
//       this.setState({ rotation: e.rotation })
//     })
//   }

//   async render() {
//     return (
//       <scene position={{ x: 5, y: 1, z: 5 }}>
//         <Axis
//           position={{ x: 1, y: 0.6, z: 0 }}
//           rotation={this.state.rotation}
//           transition={{ rotation: { duration: 100 } }}
//         />
//       </scene>
//     )
//   }
// }
