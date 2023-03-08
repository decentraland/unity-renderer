import { queryGraph } from './queryGraph'

export type GraphResponse = {
  nfts: {
    ens: {
      subdomain: string
    }
  }[]
}

export async function fetchOwnedENS(theGraphBaseUrl: string, ethAddress: string): Promise<string[]> {
  const query = `
query GetNameByBeneficiary($beneficiary: String) {
  nfts(where: { owner: $beneficiary, category: ens }) {
    ens {
      labelHash
      beneficiary
      caller
      subdomain
      createdAt
    }
  }
}`

  const variables = { beneficiary: ethAddress.toLowerCase() }

  try {
    const jsonResponse: GraphResponse = await queryGraph(theGraphBaseUrl, query, variables)
    return jsonResponse.nfts.map((nft) => nft.ens.subdomain)
  } catch (e) {
    // do nothing
  }
  return []
}
