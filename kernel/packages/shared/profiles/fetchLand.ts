import { Fetcher } from 'dcl-catalyst-commons'
import { decentralandConfigurations, getDefaultTLD } from 'config'
import { LandRole, ParcelsWithAccess } from 'decentraland-ecs/src'

const getLandQuery = () => `
  query Land($address: Bytes) {
    ownerParcels: parcels(first: 1000, where: { estate: null, owner: $address }) {
      ...parcelFields
    }
    ownerEstates: estates(first: 1000, where: { owner: $address }) {
      ...estateFields
    }
    updateOperatorParcels: parcels(first: 1000, where: { updateOperator: $address }) {
      ...parcelFields
    }
    updateOperatorEstates: estates(first: 1000, where: { updateOperator: $address }) {
      ...estateFields
    }
    ownerAuthorizations: authorizations(first: 1000, where: { owner: $address, type: "UpdateManager" }) {
      operator
      isApproved
      tokenAddress
    }
    operatorAuthorizations: authorizations(first: 1000, where: { operator: $address, type: "UpdateManager" }) {
      owner {
        address
        parcels(where: { estate: null }) {
          ...parcelFields
        }
        estates {
          ...estateFields
        }
      }
      isApproved
      tokenAddress
    }
  }
  fragment parcelFields on Parcel {
    x
    y
    tokenId
    owner {
      address
    }
    updateOperator
    data {
      name
      description
    }
  }
  fragment estateFields on Estate {
    id
    owner {
      address
    }
    updateOperator
    size
    parcels(first: 1000) {
      x
      y
      tokenId
    }
    data {
      name
      description
    }
  }
`

type LandQueryResult = {
  ownerParcels: ParcelFields[]
  ownerEstates: EstateFields[]
  updateOperatorParcels: ParcelFields[]
  updateOperatorEstates: EstateFields[]
  ownerAuthorizations: { operator: string; isApproved: boolean; tokenAddress: string }[]
  operatorAuthorizations: {
    owner: { address: string; parcels: ParcelFields[]; estates: EstateFields[] }
    isApproved: boolean
    tokenAddress: string
  }[]
}

const coordsToId = (x: string | number, y: string | number) => x + ',' + y

enum LandType {
  PARCEL = 'parcel',
  ESTATE = 'estate'
}

const fromParcel = (parcel: ParcelFields, role: LandRole) => {
  const id = coordsToId(parcel.x, parcel.y)

  const result: Land = {
    id,
    name: (parcel.data && parcel.data.name) || `Parcel ${id}`,
    type: LandType.PARCEL,
    role,
    description: (parcel.data && parcel.data.description) || null,
    x: parseInt(parcel.x, 10),
    y: parseInt(parcel.y, 10),
    owner: parcel.owner.address,
    operators: []
  }

  if (parcel.updateOperator) {
    result.operators.push(parcel.updateOperator)
  }

  return result
}

const fromEstate = (estate: EstateFields, role: LandRole) => {
  const id = estate.id

  const result: Land = {
    id,
    name: (estate.data && estate.data.name) || `Estate ${id}`,
    type: LandType.ESTATE,
    role,
    description: (estate.data && estate.data.description) || null,
    size: estate.size,
    parcels: estate.parcels.map((parcel) => ({
      x: parseInt(parcel.x, 10),
      y: parseInt(parcel.y, 10),
      id: coordsToId(parcel.x, parcel.y)
    })),
    owner: estate.owner.address,
    operators: []
  }

  if (estate.updateOperator) {
    result.operators.push(estate.updateOperator)
  }

  return result
}

const isZero = (addr: string) => {
  return /^0x(0)+$/.test(addr)
}

type Land = {
  id: string
  type: LandType
  role: LandRole
  x?: number
  y?: number
  parcels?: { x: number; y: number; id: string }[]
  size?: number
  name: string
  description: string | null
  owner: string
  operators: string[]
}

type ParcelFields = {
  x: string
  y: string
  tokenId: string
  owner: {
    address: string
  }
  updateOperator: string | null
  data: {
    name: string | null
    description: string | null
  } | null
}

type EstateFields = {
  id: string
  owner: {
    address: string
  }
  updateOperator: string | null
  size: number
  parcels: Pick<ParcelFields, 'x' | 'y' | 'tokenId'>[]
  data: {
    name: string | null
    description: string | null
  } | null
}

/**
 * This function returns all Lands (parcel or estates) that the address has access to.
 * If the address has access to 2 parcels and 2 estates (which each if composed of 3 parcels), 4 Land will be returned.
 *
 * IMPORTANT: based on a restriction in the graph, estates that will only return 1000 parcels they are composed of. This might become a problem in the future.
 */
async function fetchLand(_address: string): Promise<Land[]> {
  const address = _address.toLowerCase()
  const fetcher = new Fetcher()

  const landManagerUrl =
    getDefaultTLD() === 'org'
      ? 'https://api.thegraph.com/subgraphs/name/decentraland/land-manager'
      : 'https://api.thegraph.com/subgraphs/name/decentraland/land-manager-ropsten'

  const data = await fetcher.queryGraph<LandQueryResult>(landManagerUrl, getLandQuery(), { address })

  const lands: Land[] = []
  const landUpdateManagers = new Set<string>()
  const estateUpdateManagers = new Set<string>()

  // parcels and estates that I own
  for (const parcel of data.ownerParcels) {
    lands.push(fromParcel(parcel, LandRole.OWNER))
  }
  for (const estate of data.ownerEstates) {
    lands.push(fromEstate(estate, LandRole.OWNER))
  }

  // parcels and estates that I operate
  for (const parcel of data.updateOperatorParcels) {
    lands.push(fromParcel(parcel, LandRole.OPERATOR))
  }
  for (const estate of data.updateOperatorEstates) {
    lands.push(fromEstate(estate, LandRole.OPERATOR))
  }

  // addresses I gave UpdateManager permission are operators of all my lands
  for (const authorization of data.ownerAuthorizations) {
    const { operator, isApproved, tokenAddress } = authorization
    switch (tokenAddress) {
      case decentralandConfigurations.LANDProxy: {
        if (isApproved) {
          landUpdateManagers.add(operator)
        } else {
          landUpdateManagers.delete(operator)
        }
        break
      }
      case decentralandConfigurations.EstateProxy: {
        if (isApproved) {
          estateUpdateManagers.add(operator)
        } else {
          estateUpdateManagers.delete(operator)
        }
        break
      }
    }
  }

  // I'm operator of all the lands from addresses that gave me UpdateManager permission
  for (const authorization of data.operatorAuthorizations) {
    const { owner } = authorization
    for (const parcel of owner.parcels) {
      const land = fromParcel(parcel, LandRole.OPERATOR)
      land.operators.push(address)
      // skip if already owned or operated
      if (!lands.some((_land) => _land.id === land.id)) {
        lands.push(land)
      }
    }
    for (const estate of owner.estates) {
      if (estate.parcels.length > 0) {
        const land = fromEstate(estate, LandRole.OPERATOR)
        land.operators.push(address)
        // skip if already owned or operated
        if (!lands.some((_land) => _land.id === land.id)) {
          lands.push(land)
        }
      }
    }
  }

  return (
    lands
      // remove empty estates
      .filter((land) => land.type === LandType.PARCEL || land.parcels!.length > 0)
      // remove duplicated and zero address operators
      .map((land) => {
        land.operators = Array.from(new Set(land.operators)).filter((address) => !isZero(address))
        return land
      })
  )
}

export async function fetchParcelsWithAccess(_address: string): Promise<ParcelsWithAccess> {
  const result: ParcelsWithAccess = []
  const lands = await fetchLand(_address)
  lands.forEach((land) => {
    if (land.type === LandType.PARCEL) {
      result.push({ x: land.x!, y: land.y!, role: land.role })
    } else {
      result.push(...land.parcels!.map(({ x, y }) => ({ x, y, role: land.role })))
    }
  })
  return result
}
