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
  getUserAccount(): Promise<string>
}

@registerAPI('EthereumController')
export class EthereumController extends ExposableAPI implements IEthereumController {
  @exposeMethod
  async requirePayment(toAddress: string, amount: number, currency: string): Promise<any> {
    return requirePayment(toAddress, amount, currency)
  }

  @exposeMethod
  async signMessage(message: MessageDict) {
    return signMessage(message)
  }

  @exposeMethod
  async convertMessageToObject(message: string): Promise<MessageDict> {
    return convertMessageToObject(message)
  }

  @exposeMethod
  async sendAsync(message: RPCSendableMessage): Promise<any> {
    return sendAsync(message)
  }

  @exposeMethod
  async getUserAccount(): Promise<string> {
    return getUserAccount()
  }
}
