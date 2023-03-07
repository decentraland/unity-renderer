import { ETHEREUM_NETWORK } from 'config'
import { requestManager } from 'shared/ethereum/provider'

declare let window: Window & {
  ethereum: any
}

export async function getNetworkFromProvider(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await requestManager.net_version()
  const chainId = parseInt(web3Network, 10)
  const web3net = chainId === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.GOERLI
  return web3net
}

/**
 * Register to any change in the configuration of the wallet to reload the app and avoid wallet changes in-game.
 * TODO: move to explorer-website
 */
export function registerProviderNetChanges() {
  if (window.ethereum && typeof window.ethereum.on === 'function') {
    window.ethereum.on('chainChanged', () => location.reload())
  }
}
