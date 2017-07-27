// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { Script, inject, EntityController, createElement } from 'decentraland-api/src'
// import { EventSubscriber } from 'decentraland-rpc'

// let isScaled = false

// export default class InteractiveText extends Script {
//   @inject('EntityController') entityController: EntityController | null = null
//   eventSubscriber: EventSubscriber | null = null

//   steps = [
//     {
//       color: '#ff0000',
//       fontFamily: 'Arial'
//     },
//     {
//       color: '#00ff00',
//       fontFamily: 'Times New Roman'
//     },
//     {
//       color: '#0000ff',
//       fontFamily: 'Arial'
//     },
//     {
//       color: '#FFFF00',
//       fontFamily: 'Times New Roman'
//     }
//   ]

//   currentStep = 0

//   async systemDidEnable() {
//     this.eventSubscriber = new EventSubscriber(this.entityController!)
//     await this.render()

//     this.eventSubscriber.on('interactiveText_click', async (evt: any) => {
//       isScaled = !isScaled
//       await this.render()
//     })

//     setInterval(async () => {
//       await this.render()
//     }, 1000)
//   }

//   async render() {
//     this.currentStep++
//     this.currentStep %= this.steps.length

//     const data = this.steps[this.currentStep]

//     await this.entityController!.render(
//       <scene>
//         <text
//           id="interactiveText"
//           position={{ x: 1, y: 1, z: 1 }}
//           value="Hello world!"
//           scale={isScaled ? 3 : 1}
//           {...data}
//         />
//       </scene>
//     )
//   }
// }
