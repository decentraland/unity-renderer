export const catalystABI = [
  {
    constant: true,
    inputs: [],
    name: 'catalystCount',
    outputs: [{ name: '', type: 'uint256' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'bytes32' }],
    name: 'catalystIndexById',
    outputs: [{ name: '', type: 'uint256' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '_id', type: 'bytes32' }],
    name: 'catalystOwner',
    outputs: [{ name: '', type: 'address' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '_id', type: 'bytes32' }],
    name: 'catalystDomain',
    outputs: [{ name: '', type: 'string' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'uint256' }],
    name: 'catalystIds',
    outputs: [{ name: '', type: 'bytes32' }],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  },
  {
    constant: true,
    inputs: [{ name: '', type: 'bytes32' }],
    name: 'catalystById',
    outputs: [
      { name: 'id', type: 'bytes32' },
      { name: 'owner', type: 'address' },
      { name: 'domain', type: 'string' },
      { name: 'startTime', type: 'uint256' },
      { name: 'endTime', type: 'uint256' }
    ],
    payable: false,
    stateMutability: 'view',
    type: 'function'
  }
]
