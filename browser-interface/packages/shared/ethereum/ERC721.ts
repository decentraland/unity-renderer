import { ContractFactory, RequestManager, Contract, Address } from 'eth-connect'
import { BigNumber } from 'eth-connect'

const ERC721Abi = [
  {
    constant: true,
    inputs: [
      {
        name: '_tokenId',
        type: 'uint256'
      }
    ],
    name: 'ownerOf',
    outputs: [
      {
        name: 'owner',
        type: 'address'
      }
    ],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [
      {
        name: '_owner',
        type: 'address'
      }
    ],
    name: 'balanceOf',
    outputs: [
      {
        name: 'balance',
        type: 'uint256'
      }
    ],
    payable: false,
    type: 'function'
  }
]

export interface IERC721 extends Contract {
  ownerOf(_tokenId: Address): Promise<Address>
  balanceOf(_of: Address): Promise<BigNumber>
}

// TODO: Cache contracts by address+requestManager, contracts are immutable

export async function getERC721(requestManager: RequestManager, address: Address): Promise<IERC721> {
  return (await new ContractFactory(requestManager, ERC721Abi).at(address)) as IERC721
}
