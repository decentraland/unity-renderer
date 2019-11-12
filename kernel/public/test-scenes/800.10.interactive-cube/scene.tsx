// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { Script, inject, EventSubscriber, SoundController } from 'decentraland-api/src'

// const colors = ['red', 'green', 'purple', 'black', 'orange', 'cyan']

// let tick = performance.now()

// export default class AudioPlayer extends Script {
//   @inject('SoundController')
//   audio: SoundController | null = null
//   @inject('EntityController')
//   entityController: EntityController | null = null
//   eventSubscriber: EventSubscriber | null = null

//   async systemDidEnable() {
//     this.eventSubscriber = new EventSubscriber(this.entityController!)

//     // TODO(dani): Create an issue for this, eventSubscriber handlers should accept async functions
//     this.eventSubscriber.on('interactiveBox_click', async (evt: any) => {
//       await this.render()
//       // TODO(dani): Fix relative resource loading for components
//       await this.audio!.playSound('sounds/sound.ogg')
//     })

//     setInterval(async () => {
//       tick = performance.now() * 0.01
//       await this.entityController!.setEntityAttributes('interactiveBox', {
//         rotation: `${Math.sin(tick * 0.01) * 180} ${Math.cos(Math.sin(tick * 0.01)) * 180} ${Math.sin(tick * 0.03) *
//           180}`
//       })
//     }, 50)

//     await this.render()
//   }

//   async render() {
//     await this.entityController!.render(
//       <scene>
//         <box
//           id="interactiveBox"
//           position={{ x: 0, y: 0, z: 0 }}
//           color={colors[Math.floor(Math.random() * colors.length)]}
//           scale={{ x: 10, y: 10, z: 10 }}
//           rotation={{
//             x: Math.sin(tick * 0.01) * 180,
//             y: Math.cos(Math.sin(tick * 0.01)) * 180,

//             z: Math.sin(tick * 0.03) * 180
//           }}
//         />
//       </scene>
//     )
//   }
// }
