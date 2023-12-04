import { ETHEREUM_NETWORK } from 'config'
import { requestManager } from './provider'

export async function getEthereumNetworkFromProvider(): Promise<ETHEREUM_NETWORK> {
  try {
    const web3Network = await requestManager.net_version()
    const chainId = parseInt(web3Network, 10)
    const web3net = chainId === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.SEPOLIA
    return web3net
  } catch (err) {
    return ETHEREUM_NETWORK.MAINNET
  }
}
