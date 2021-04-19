public static class TheGraphQueries
{
    public static readonly string getLandQuery = @"
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
    ownerAuthorizations: authorizations(first: 1000, where: { owner: $address, type: \""UpdateManager\"" }) {
      operator
      isApproved
      tokenAddress
    }
    operatorAuthorizations: authorizations(first: 1000, where: { operator: $address, type: \""UpdateManager\"" }) {
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
";
}