// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { createElement, ScriptableScene } from 'decentraland-api/src'
// import { RequestManager } from 'eth-connect'

// export default class EthereumProvider extends ScriptableScene {
//   async sceneDidMount() {
//     const provider = await this.getEthereumProvider()
//     const requestManager = new RequestManager(provider)

//     const block = await requestManager.eth_getBlockByNumber(48, true)

//     // tslint:disable-next-line:no-console
//     console.log('Eth block (from scene)', JSON.stringify(block))
//   }

//   async render() {
//     return <scene />
//   }
// }
