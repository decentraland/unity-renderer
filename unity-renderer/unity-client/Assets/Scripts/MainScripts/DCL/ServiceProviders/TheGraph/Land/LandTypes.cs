using System;
using System.Collections.Generic;

public enum LandType
{
    PARCEL,
    ESTATE
}

public enum LandRole
{
    OWNER,
    OPERATOR
}

public class Parcel
{
    public int x;
    public int y;
    public string id;
}

public class Land
{
    public string id;
    public LandType type;
    public LandRole role;
    public int x;
    public int y;
    public List<Parcel> parcels;
    public int size;
    public string name;
    public string description;
    public string owner;
    public List<string> operators;
}


[Serializable]
internal class LandQueryResultWrapped
{
    public LandQueryResult data;
}

[Serializable]
internal class LandQueryResult
{
    public ParcelFields[] ownerParcels;
    public EstateFields[] ownerEstates;
    public ParcelFields[] updateOperatorParcels;
    public EstateFields[] updateOperatorEstates;
    public OwnerAuthorizationData[] ownerAuthorizations;
    public OperatorAuthorizationData[] operatorAuthorizations;
}

[Serializable]
internal class ParcelData
{
    public string x;
    public string y;
    public string tokenId;
}

[Serializable]
internal class ParcelFields : ParcelData
{
    public OwnerAddressData owner;
    public string updateOperator;
    public LandData data;
}

[Serializable]
internal class EstateFields
{
    public string id;
    public OwnerAddressData owner;
    public string updateOperator;
    public int size;
    public ParcelData[] parcels;
    public LandData data;
}

[Serializable]
internal class LandData
{
    public string name;
    public string description;
}

[Serializable]
internal class OwnerAddressData
{
    public string address;
}

[Serializable]
internal class OwnerData : OwnerAddressData
{
    public ParcelFields[] parcels;
    public EstateFields[] estates;
}

[Serializable]
internal class OwnerAuthorizationData
{
    public string @operator; 
    public bool isApproved;
    public string tokenAddress;
}

[Serializable]
internal class OperatorAuthorizationData
{
    public OwnerData owner;
    public bool isApproved;
    public string tokenAddress;
}