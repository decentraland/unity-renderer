// TODO: ECS
// tslint:disable

// import { Script, inject, EntityController } from 'decentraland-api/src'
// import { EventSubscriber } from 'decentraland-rpc'

// export default class Video extends Script {
//   @inject('EntityController') entityController: EntityController | null = null
//   eventSubscriber: EventSubscriber | null = null

//   localPlaying: boolean = false
//   externalPlaying: boolean = false
//   volume: number = 50

//   async systemDidEnable() {
//     this.eventSubscriber = new EventSubscriber(this.entityController!)

//     this.eventSubscriber.on('video-local_click', async (evt: any) => {
//       await this.handleLocalVidPlayback(evt.data.entityId)
//     })

//     this.eventSubscriber.on('video-external_click', async (evt: any) => {
//       await this.handleExtVidPlayback(evt.data.entityId)
//     })

//     // Volume UI
//     this.eventSubscriber.on('volume-plus_click', async (evt: any) => {
//       await this.addVolume('video-local')
//     })
//     this.eventSubscriber.on('volume-minus_click', async (evt: any) => {
//       await this.decreaseVolume('video-local')
//     })

//     // Playback UI
//     this.eventSubscriber.on('play_click', async (evt: any) => {
//       await this.play('video-local')
//       await this.updatePlaybackUI()
//     })
//     this.eventSubscriber.on('pause_click', async (evt: any) => {
//       await this.pause('video-local')
//       await this.updatePlaybackUI()
//     })
//   }

//   async addVolume(videoId: string) {
//     if (this.volume < 100) {
//       this.volume += 10
//     }

//     // Set volume on video entity
//     await this.entityController!.setEntityAttributes(videoId, {
//       volume: this.volume.toString()
//     })

//     // Update volume value in UI
//     await this.entityController!.setEntityAttributes('volume-value', {
//       value: this.volume.toString()
//     })
//   }

//   async decreaseVolume(videoId: string) {
//     if (this.volume > 0) {
//       this.volume -= 10
//     }

//     // Set volume on video entity
//     await this.entityController!.setEntityAttributes(videoId, {
//       volume: this.volume.toString()
//     })

//     // Update volume value in UI
//     await this.entityController!.setEntityAttributes('volume-value', {
//       value: this.volume.toString()
//     })
//   }

//   async handleLocalVidPlayback(videoId: string) {
//     if (this.localPlaying) {
//       await this.pause(videoId)
//     } else {
//       await this.play(videoId)
//     }

//     await this.updatePlaybackUI()
//   }

//   async handleExtVidPlayback(videoId: string) {
//     if (this.externalPlaying) {
//       this.externalPlaying = false
//       await this.entityController!.setEntityAttributes(videoId, {
//         play: false
//       })
//     } else {
//       this.externalPlaying = true
//       await this.entityController!.setEntityAttributes(videoId, {
//         play: true
//       })
//     }
//   }

//   async play(videoId: string) {
//     this.localPlaying = true

//     await this.entityController!.setEntityAttributes(videoId, {
//       play: true
//     })
//   }

//   async pause(videoId: string) {
//     this.localPlaying = false

//     await this.entityController!.setEntityAttributes(videoId, {
//       play: false
//     })
//   }

//   async updatePlaybackUI() {
//     if (this.localPlaying) {
//       // Hide play button, show pause button
//       await this.entityController!.setEntityAttributes('play', {
//         visible: false
//       })
//       await this.entityController!.setEntityAttributes('pause', {
//         visible: true
//       })
//     } else {
//       // Hide pause button, show play button
//       await this.entityController!.setEntityAttributes('play', {
//         visible: true
//       })
//       await this.entityController!.setEntityAttributes('pause', {
//         visible: false
//       })
//     }
//   }
// }
