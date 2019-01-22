import './apis'
import { ensureLocalStorageStructure } from './comms/profile'
import { initializeUrlPositionObserver } from './world/positionThings'
import { ETHEREUM_NETWORK, MOBILE_DEBUG, networkConfigurations, PREVIEW } from '../config'
import { getERC721 } from './ethereum/ERC721'
import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { connect } from './comms'
import { info, error } from '../engine/logger'
import { requestManager, awaitWeb3Approval } from './ethereum/provider'

async function grantAccess(address: string | null, net: ETHEREUM_NETWORK) {
  if (MOBILE_DEBUG || PREVIEW) {
    return true
  }

  let isWhitelisted = location.hostname === 'localhost' || navigator.userAgent.includes('Oculus')

  if (!isWhitelisted) {
    const contract = await getERC721(requestManager, networkConfigurations[net].invite)

    const balance = await contract.balanceOf(address)

    isWhitelisted = balance.gt(0)

    if (!isWhitelisted) {
      throw new Error(`Unauthorized ${address} [${net}]`)
    }
  }

  return isWhitelisted
}

function getNetworkFromDomain(): ETHEREUM_NETWORK {
  const domain = window.location.host
  switch (domain) {
    case 'client.decentraland.org':
    case 'client.decentraland.today':
      return ETHEREUM_NETWORK.MAINNET
    case 'client.decentraland.zone':
      return ETHEREUM_NETWORK.ROPSTEN
    default:
      return null
  }
}

async function getAddressAndNetwork() {
  try {
    await awaitWeb3Approval()
    const address = await getUserAccount()
    const web3Network = await getNetwork()
    const web3net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN

    const net = getNetworkFromDomain() || web3net

    if (web3net && net !== web3net) {
      // TODO @fmiras show an HTML error if web3 networks differs from domain network and do not load client at all
      error(`Switch to network ${net}`)
    }

    info('Using ETH network: ', net)

    return {
      net,
      address
    }
  } catch (e) {
    if (PREVIEW) {
      error('Could not get Ethereum address, WebRTC will be disabled.')
      info(e)

      return {
        net: getNetworkFromDomain() || ETHEREUM_NETWORK.MAINNET,
        address: '0x0000000000000000000000000000000000000000'
      }
    } else {
      throw e
    }
  }
}

export async function initShared() {
  const { address, net } = await getAddressAndNetwork()

  const isWhitelisted = await grantAccess(address, net)

  if (isWhitelisted) {
    connect(
      address,
      net
    )
  } else {
    throw new Error(`The address ${address} is not whitelisted`)
  }

  ensureLocalStorageStructure()
  initializeUrlPositionObserver()

  return {
    net,
    address
  }
}
