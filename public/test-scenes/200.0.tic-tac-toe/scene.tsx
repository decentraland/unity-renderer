// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { Script, inject, EventSubscriber, SoundController, EntityController, createElement } from 'decentraland-api/src'

// let tick = performance.now()

// const winingCombinations = [
//   [0, 1, 2], // 1 row
//   [3, 4, 5], // 2 row
//   [6, 7, 8], // 3 row

//   [0, 3, 6], // 1 col
//   [1, 4, 7], // 2 col
//   [2, 5, 8], // 3 col

//   // tslint:disable-next-line:no-commented-out-code
//   [0, 4, 8], // nw - se
//   // tslint:disable-next-line:no-commented-out-code
//   [6, 4, 2] // sw - ne
// ]

// type GameSymbol = 'x' | 'o' | null

// export default class AudioPlayer extends Script {
//   @inject('SoundController')
//   audio: SoundController | null = null
//   @inject('EntityController')
//   entityController: EntityController | null = null

//   eventSubscriber: EventSubscriber | null = null
//   mySymbol: GameSymbol = null
//   board: GameSymbol[] = [null, null, null, null, null, null, null, null, null]

//   getWinner() {
//     return ['x', 'o'].find($ =>
//       winingCombinations.some(combination => combination.every(position => this.board[position] === $))
//     )
//   }

//   selectMySymbol(symbol: GameSymbol) {
//     this.mySymbol = symbol
//   }

//   setAt(position: number, symbol: GameSymbol) {
//     this.board[position] = symbol
//   }

//   async systemDidEnable() {
//     this.eventSubscriber = new EventSubscriber(this.entityController!)

//     this.eventSubscriber.on(`reset_click`, async (evt: any) => {
//       this.board = this.board.map(() => null)
//       await this.render()
//       await this.audio!.playSound('sounds/sound.ogg')
//     })

//     this.selectMySymbol('x')

//     this.board.map(($, $$) =>
//       this.eventSubscriber!.on(`position-${$$}_click`, async (evt: any) => {
//         this.setAt($$, this.mySymbol)

//         if (this.getWinner()) {
//           await this.audio!.playSound('sounds/Nyan_cat.ogg')
//         }

//         this.selectMySymbol(this.mySymbol === 'x' ? 'o' : 'x')
//         await this.render()
//         await this.audio!.playSound('sounds/sound.ogg')
//       })
//     )

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
//     const game = (
//       <scene>
//         <box position={{ x: 5, y: 5, z: 5 }} color="red" id="reset" />
//         {this.board.map(($, ix) => (
//           <box
//             id={`position-${ix}`}
//             color={$ === null ? '#000000' : $ === 'x' ? '#ff0000' : '#00ff00'}
//             position={{ x: ix % 3, y: 0.2, z: Math.floor(ix / 3) }}
//             scale={{ x: 0.8, y: 0.1, z: 0.8 }}
//           />
//         ))}
//       </scene>
//     )
//     await this.entityController!.render(game)
//   }
// }
