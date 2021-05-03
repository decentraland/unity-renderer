using System;

[Serializable]
public class BuilderProjectsPanelLandDataMock
{
    [Serializable]
    public class ParcelData
    {
        public int x;
        public int y;
        public string id;
    }
    
    public int x;
    public int y;
    public string id;
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
