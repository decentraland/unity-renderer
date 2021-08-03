public static class TheGraphQueries
{
    public static readonly string getLandQuery = @"
  query Land($address: Bytes) {
    ownerParcels: parcels(first: [AMOUNT], where: { estate: null, owner: $address }) {
      ...parcelFields
    }
    ownerEstates: estates(first: [AMOUNT], where: { owner: $address }) {
      ...estateFields
    }
    updateOperatorParcels: parcels(first: [AMOUNT], where: { updateOperator: $address }) {
      ...parcelFields
    }
    updateOperatorEstates: estates(first: [AMOUNT], where: { updateOperator: $address }) {
      ...estateFields
    }
    ownerAuthorizations: authorizations(first: [AMOUNT], where: { owner: $address, type: \""UpdateManager\"" }) {
      operator
      isApproved
      tokenAddress
    }
    operatorAuthorizations: authorizations(first: [AMOUNT], where: { operator: $address, type: \""UpdateManager\"" }) {
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
    parcels(first: [AMOUNT]) {
      x
      y
      tokenId
    }
    data {
      name
      description
    }
  }
";

    public static readonly string getPolygonManaQuery = @"
    query MANA($address: ID){
        accounts(where: {id:$address}){
            id,
            mana
        }
    }
";

    public static readonly string getNftCollectionsQuery = @"
    query WearablesCollections($address: ID){
        nfts(where: { owner: $address }) {
            urn,
            collection {
                id
            }
            tokenId
        }
    }
";
}