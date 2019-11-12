// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { inject, EthereumController, createElement, ScriptableScene } from 'decentraland-api/src'
// import { toHex } from 'eth-connect'

// const messageWithHeader = `# DCL Signed message
// Attacker: 4
// Defender: 387
// Timestamp: 1531404397`

// export default class SignMessage extends ScriptableScene<any, any> {
//   @inject('EthereumController')
//   eth: EthereumController | null = null

//   state = {
//     isEqual: false,
//     convertedMessage: {},
//     message: '',
//     hexEncodedMessage: '',
//     signature: '',
//     messageToSign: `Attacker: 4
// Defender: 387
// Timestamp: 1531404397`
//   }

//   async sceneDidMount() {
//     this.subscribeTo('click', async e => {
//       if (e.entityId === 'button-sign') {
//         await this.signMessage()
//       }
//     })
//   }

//   async signMessage() {
//     const convertedMessage = await this.eth!.convertMessageToObject(this.state.messageToSign)
//     const { message, hexEncodedMessage, signature } = await this.eth!.signMessage(convertedMessage)

//     const hexOfExpectedMessage = toHex(messageWithHeader)

//     const isEqual = hexEncodedMessage === hexOfExpectedMessage && message === messageWithHeader

//     this.setState({ convertedMessage, message, hexEncodedMessage, signature, isEqual })
//   }

//   async render() {
//     return (
//       <scene position={{ x: 5, y: 0, z: 5 }}>
//         <text
//           position={{ x: -3, y: 1.7, z: -3 }}
//           color="red"
//           value={this.state.messageToSign}
//           fontSize={30}
//           width={3}
//         />
//         <entity position={{ x: -3, y: 1.4, z: -3 }}>
//           <plane id="button-sign" scale={{ x: 0.8, y: 0.2, z: 1 }} color="#bada55" />
//           <text value="Sign message" fontSize={60} color="black" />
//         </entity>

//         <text
//           id="tx-status"
//           position={{ x: -1, y: 2.5, z: 3 }}
//           color="red"
//           value={JSON.stringify(this.state.convertedMessage, null, 2)}
//           fontSize={30}
//           width={3}
//         />

//         <text
//           position={{ x: -1, y: 2, z: 3 }}
//           color="black"
//           value={`Message should be \n"0x232044434c205369676e6564206d6573736167650a41747461636b65723a20340a446566656e6465723a203338370a54696d657374616d703a2031353331343034333937"`}
//           fontSize={30}
//           width={6}
//         />
//         <text
//           position={{ x: -1, y: 1.8, z: 3 }}
//           color="red"
//           value={this.state.hexEncodedMessage}
//           fontSize={30}
//           width={6}
//         />
//         <text
//           position={{ x: -1, y: 1.7, z: 3 }}
//           color="red"
//           value={`Is the original message and signed message equal? ${this.state.isEqual}`}
//           fontSize={25}
//           width={6}
//         />
//         <text
//           position={{ x: -1, y: 1.3, z: 3 }}
//           color="black"
//           value={`Signature should be \n"0xc1899fa658a3fcb493c9af2917cb782676fbc1e6df2cddf1d74df1d9ff4bcdca0f47aebf469c1db6ad561beba96566c2bba21461bc0d0c9060a07ca015d0eac21c"`}
//           fontSize={30}
//           width={6}
//         />
//         <text position={{ x: -1, y: 1.1, z: 3 }} color="red" value={this.state.signature} fontSize={30} width={6} />
//       </scene>
//     )
//   }
// }
