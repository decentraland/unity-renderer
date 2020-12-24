import BigNumber from 'bignumber.js'
import { fromWei, toHex, toWei } from 'eth-connect'
import { future, IFuture } from 'fp-future'
import { defaultLogger } from 'shared/logger'
import { RPCSendableMessage } from 'shared/types'
import { getERC20 } from './ERC20'
import { getERC721 } from './ERC721'
import { requestManager, getUserAccount as getUserAccountPrime, awaitWeb3Approval } from './provider'

export interface MessageDict {
  [key: string]: string
}

export interface MarketPlaceOrder {
  id: string
  address: string
  price: number
  expiresAt: number
  currency: string
  erc20addr: string
}

const messageMap = new Map<number, IFuture<any>>()

let lastSentId = 100000

const whitelist = [
  'eth_sendTransaction',
  'eth_getTransactionReceipt',
  'eth_estimateGas',
  'eth_call',
  'eth_getBalance',
  'eth_getStorageAt',
  'eth_blockNumber',
  'eth_gasPrice',
  'eth_protocolVersion',
  'net_version',
  'web3_sha3',
  'web3_clientVersion',
  'eth_getTransactionCount',
  'eth_getBlockByNumber',
  'eth_signTypedData_v4'
]

function isWhitelistedRPC(msg: RPCSendableMessage) {
  return msg.method && whitelist.includes(msg.method)
}

export async function getUserAccount(): Promise<string | undefined> {
  await awaitWeb3Approval()
  return getUserAccountPrime()
}

export async function getNetwork(): Promise<string> {
  return requestManager.net_version()
}

export async function sendAsync(message: RPCSendableMessage): Promise<any> {
  if (
    typeof (message as any) !== 'object' ||
    typeof (message.id as any) !== 'number' ||
    typeof (message.method as any) !== 'string' ||
    (message.jsonrpc as any) !== '2.0'
  ) {
    throw new Error('Invalid JSON-RPC message')
  }

  if (!isWhitelistedRPC(message)) {
    throw new Error(`The Ethereum method "${message.method}" is blacklisted on Decentraland Provider`)
  }

  lastSentId += 1

  const theFuture = future<any>()

  const originalId = message.id

  message.id = lastSentId

  messageMap.set(message.id, theFuture)

  // TODO: Sanitize and filter messages

  requestManager.provider.sendAsync(message, (error: Error, res: { id: number }) => {
    if (error) {
      theFuture.reject(error)
    } else {
      res.id = originalId
      theFuture.resolve(res)
    }
    messageMap.delete(message.id)
  })

  return theFuture
}

/**
 * Gets ERC20 balance for a given address
 * @param  {string} [address] - account address
 * @param  {string} [tokenAddress] - ERC20 token address
 * @param  {string} [inWei] - true if returns in balance in wei
 * @return {string} - account balance
 */
export async function getERC20Balance(
  address: string,
  tokenAddress: string,
  inWei: boolean = false
): Promise<BigNumber> {
  const contract = await getERC20(requestManager, tokenAddress)

  const balance = await contract.balanceOf(address)

  if (inWei) {
    return balance
  }

  return fromWei(balance as any, 'ether') as any
}

/**
 * Check a ERC721 contract for ownership status
 * @param  {string} [ownerAddress] - namespace to resolve
 * @param  {string} [tokenId] - tokenId in the registry contract
 * @param  {string} [registryAddress] - address of the ERC721 DAR.
 * @return {string} - true if provided address is the owner of the asset.
 */
export async function getERC721Owner(ownerAddress: string, tokenId: string, registryAddress: string): Promise<boolean> {
  const contract = await getERC721(requestManager, registryAddress)

  const owner = await contract.ownerOf(tokenId)

  return owner.toLowerCase() === ownerAddress.toLowerCase()
}

/**
 * Requires a generic payment in ETH or ERC20.
 * @param  {string} [toAddress] - NFT asset id.
 * @param  {string} [amount] - Exact amount of the order.
 * @param  {string} [currency] - ETH or ERC20 supported token symbol
 */
export async function requirePayment(toAddress: string, amount: number, currency: string): Promise<any> {
  try {
    let fromAddress = await getUserAccount()
    if (!fromAddress) {
      throw new Error(`Not a web3 game session`)
    }

    let result: Promise<any>

    if (currency === 'ETH') {
      result = requestManager.eth_sendTransaction({
        to: toAddress,
        from: fromAddress,
        // TODO: fix this in eth-connect
        value: toWei(amount as any, 'ether') as any,
        data: null as any
      })
    } else {
      const supportedTokens: Record<string, string> = {} // a TODO: getNetworkConfigurations(network).paymentTokens

      if (currency in supportedTokens === false) {
        // tslint:disable:no-console
        console.warn(`WARNING! Using a non-supported coin. Skipping operation! Please use one of the next coins 'ETH'`)
        return false
      }

      const contractInstance = await getERC20(requestManager, supportedTokens[currency])

      // Check this account has enough tokens for the transaction
      const balance = await getERC20Balance(fromAddress, contractInstance.address)
      if (balance.lt(amount)) {
        throw new Error(`Not enought ${currency} balance for this transaction`)
      }

      // TODO: fix this
      result = contractInstance.transfer(toAddress, amount as any, {
        from: fromAddress,
        gas: 3000000
      })
    }
    return result
  } catch (err) {
    defaultLogger.error('Error in EthereumController#requirePayment', err)
    throw new Error(err)
  }
}

/**
 * Method for converting dictionary to string with DCL signed message header.
 * @param  {dict} [MessageDict] - Message in an object format.
 * @return {object} - Promise of message and signature in an object.
 */
export async function messageToString(dict: MessageDict) {
  const header = `# DCL Signed message\n`
  const payload = Object.entries(dict)
    .map(([key, value]) => `${key}: ${value}`)
    .join('\n')

  return header.concat(payload)
}

/**
 * Takes a dictionary, converts it to string with correct format and signs it.
 * @param  {messageToSign} [MessageDict] - Message in an object format.
 * @return {object} - Promise of message, hashed message and signature in an object.
 */
export async function signMessage(messageDict: MessageDict) {
  const signerAccount = await getUserAccount()
  if (!signerAccount) {
    throw new Error(`Not a web3 game session`)
  }

  const messageToSign = await messageToString(messageDict)

  if (messageToSign.indexOf('# DCL Signed message') === -1) {
    throw new Error(`Message is not in a right format.`)
  }

  const hexEncodedMessage = toHex(messageToSign)

  try {
    const signature = await requestManager.personal_sign(hexEncodedMessage, signerAccount, '')
    return { message: messageToSign, hexEncodedMessage, signature }
  } catch (err) {
    throw new Error(err)
  }
}

/**
 * Takes a message string, parses it and converts to object.
 * @param  {message} [string] - Message in a string format.
 * @return {object} - Promise of message as a MessageDict.
 */
export async function convertMessageToObject(message: string): Promise<MessageDict> {
  let parsedMessage = message

  // Remove `# DCL Signed message` header
  if (message.indexOf('# DCL Signed message') === 0) {
    parsedMessage = message.slice(21)
  }
  // First, split the string parts into nested array
  const arr = parsedMessage
    .split('\n')
    .map((m) => m.split(':'))
    .map(([key, value]) => [key, value.trim()])

  // convert the array into object of type MessageDict
  return arr.reduce((o, [key, value]) => ({ ...o, [key]: value }), {})
}
