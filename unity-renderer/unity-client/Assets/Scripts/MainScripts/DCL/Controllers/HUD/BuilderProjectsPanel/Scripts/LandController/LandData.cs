using System;

[Serializable]    
internal class LandData : ParcelData
{
    public bool isEstate;
    public bool isParcel;
    public bool isOwner;
    public bool isOperator;
    public ParcelData[] parcels;
    public int size;
    public string name;
    public string description;
    public string thumbnailURL;
}
