public static class TheGraphQueries
{
    private const int MAX_LAND_QUERY_AMOUNT = 1000;
    public static string getLandQuery => landQuery.Replace("$amount", MAX_LAND_QUERY_AMOUNT.ToString());

    private static readonly string landQuery = @"
  query Land($address: Bytes) {
    ownerParcels: parcels(first: $amount, where: { estate: null, owner: $address }) {
      ...parcelFields
    }
    ownerEstates: estates(first: $amount, where: { owner: $address }) {
      ...estateFields
    }
    updateOperatorParcels: parcels(first: $amount, where: { updateOperator: $address }) {
      ...parcelFields
    }
    updateOperatorEstates: estates(first: $amount, where: { updateOperator: $address }) {
      ...estateFields
    }
    ownerAuthorizations: authorizations(first: $amount, where: { owner: $address, type: \""UpdateManager\"" }) {
      operator
      isApproved
      tokenAddress
    }
    operatorAuthorizations: authorizations(first: $amount, where: { operator: $address, type: \""UpdateManager\"" }) {
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
    parcels(first: $amount) {
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