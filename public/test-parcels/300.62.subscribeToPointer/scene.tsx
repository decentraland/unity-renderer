// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene, ISimplifiedNode, MessageBusClient, PointerEvent } from 'decentraland-api/src'

// export default class BoxFollower extends ScriptableScene {
//   state = { elements: [] as ISimplifiedNode[] }
//   messageBus!: MessageBusClient<any>

//   addElement(elem: ISimplifiedNode) {
//     elem.attrs.id = this.state.elements.length

//     this.setState({ elements: [...this.state.elements, elem] })
//   }

//   async sceneDidMount() {
//     this.messageBus = await MessageBusClient.acquireParcelSceneChannel(this)

//     this.messageBus.on('pointerDown', (e: PointerEvent) => {
//       const position = {
//         x: e.from.x + e.direction.x,
//         y: e.from.y + e.direction.y,
//         z: e.from.z + e.direction.z
//       }
//       this.addElement(<box position={position} scale={0.1} />)
//     })

//     this.subscribeTo('pointerDown', e => {
//       this.messageBus.emit('pointerDown', e)
//     })
//   }

//   async render() {
//     return <scene>{this.state.elements}</scene>
//   }
// }
