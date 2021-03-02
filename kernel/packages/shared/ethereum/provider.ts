import { RequestManager } from 'eth-connect'
import { future } from 'fp-future'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'
import { Eth } from 'web3x/eth'
import { ProviderType } from 'decentraland-connect'
import { EthereumConnector } from './EthereumConnector'
import { LegacyProviderAdapter } from 'web3x/providers'
import { EDITOR } from 'config'

let ethConnector: EthereumConnector
export const providerFuture = future()
export const requestManager = new RequestManager((window as any).ethereum ?? null)

export const loginCompleted = future<void>()
  ; (window as any).loginCompleted = loginCompleted

export function createEth(provider: any = null) {
  return ethConnector.createEth(provider)
}

// This function creates a Web3x eth object without the need of having initiated sign in / sign up. Used when requesting the catalysts
export function createEthWhenNotConnectedToWeb3(): Eth {
  const ethereum = (window as any).ethereum
  if (ethereum) {
    // If we have a web3 enabled browser, we can use that
    return new Eth(new LegacyProviderAdapter((window as any).ethereum))
  } else {
    // If not, we use infura
    return new Eth(EthereumConnector.createWeb3xWebsocketProvider())
  }
}

export function getEthConnector(): EthereumConnector {
  if (!ethConnector) {
    ethConnector = new EthereumConnector()
  }
  return ethConnector
}

export async function requestProvider(type: ProviderType | null) {
  try {
    const { provider } = await ethConnector.connect(type)
    requestManager.setProvider(provider)
    providerFuture.resolve({
      successful: !isGuest(),
      provider: provider,
      localIdentity: isGuest() ? Account.create() : undefined
    })
    return provider
  } catch (e) {
    defaultLogger.log('Could not get a wallet connection', e)
    requestManager.setProvider(null)
  }
  return null
}

export function isGuest(): boolean {
  return ethConnector.isGuest()
}

export function getProviderType() {
  return ethConnector.getType()
}

export async function awaitApproval(): Promise<void> {
  if (EDITOR) {
    await requestProvider(null)
  }
  return providerFuture
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(returnChecksum: boolean = false): Promise<string | undefined> {
  try {
    const eth = createEth()

    if (!eth) return undefined

    const accounts = await eth.getAccounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return returnChecksum ? accounts[0].toJSON() : accounts[0].toJSON().toLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}

export async function getUserEthAccountIfAvailable(returnChecksum: boolean = false): Promise<string | undefined> {
  if (!isGuest()) {
    return getUserAccount(returnChecksum)
  }
}
