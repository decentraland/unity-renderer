import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import {
  MessageDict,
  requirePayment,
  sendAsync,
  convertMessageToObject,
  signMessage,
  getUserAccount
} from 'shared/ethereum/EthereumService'
import { ExposableAPI } from './ExposableAPI'
import { RPCSendableMessage } from 'shared/types'
import { PREVIEW, ENABLE_WEB3 } from 'config/index'

export interface IEthereumController {
  /**
   * Requires a generic payment in ETH or ERC20.
   * @param  {string} [toAddress] - NFT asset id.
   * @param  {number} [amount] - Exact amount of the order.
   * @param  {string} [currency] - ETH or ERC20 supported token symbol
   */
  requirePayment(toAddress: string, amount: number, currency: string): Promise<any>

  /**
   * Takes a dictionary, converts it to string with correct format and signs it.
   * @param  {messageToSign} [MessageDict] - Message in an object format.
   * @return {object} - Promise of message and signature in an object.
   */
  signMessage(message: MessageDict): Promise<{ message: string; hexEncodedMessage: string; signature: string }>

  /**
   * Takes a message string, parses it and converts to object.
   * @param  {message} [string] - Message in a string format.
   * @return {object} - Promise of message as a MessageDict.
   * @internal
   */
  convertMessageToObject(message: string): Promise<MessageDict>

  /**
   * Used to build a Ethereum provider
   */
  sendAsync(message: RPCSendableMessage): Promise<any>

  /**
   * Gets the user's public key
   */
  getUserAccount(): Promise<string | undefined>
}

@registerAPI('EthereumController')
export class EthereumController extends ExposableAPI implements IEthereumController {
  @exposeMethod
  async requirePayment(toAddress: string, amount: number, currency: string): Promise<any> {
    logWeb3WarningIfNecessary()
    return requirePayment(toAddress, amount, currency)
  }

  @exposeMethod
  async signMessage(message: MessageDict) {
    logWeb3WarningIfNecessary()
    return signMessage(message)
  }

  @exposeMethod
  async convertMessageToObject(message: string): Promise<MessageDict> {
    logWeb3WarningIfNecessary()
    return convertMessageToObject(message)
  }

  @exposeMethod
  async sendAsync(message: RPCSendableMessage): Promise<any> {
    logWeb3WarningIfNecessary()
    return sendAsync(message)
  }

  @exposeMethod
  async getUserAccount(): Promise<string | undefined> {
    logWeb3WarningIfNecessary()
    return getUserAccount()
  }
}

function logWeb3WarningIfNecessary() {
  if (PREVIEW && !ENABLE_WEB3) {
    // tslint:disable:no-console
    console.warn(
      `WARNING! Accessing EthereumController/web3-provider but web3 is not enabled! Please make sure ENABLE_WEB3 query parameter is used in the URL
For more details: https://docs.decentraland.org/blockchain-integration/scene-blockchain-operations`
    )
  }
}
