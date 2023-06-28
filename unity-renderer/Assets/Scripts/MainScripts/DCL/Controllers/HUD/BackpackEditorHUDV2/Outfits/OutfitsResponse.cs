using DCL.Backpack;
using System;

[Serializable]
public class OutfitsResponse
{
    public Metadata metadata;
    public OutfitsResponse(Metadata metadata)
    {
        this.metadata = metadata;
    }

    [Serializable]
    public class Metadata
    {
        public OutfitItem[] outfits;
    }

    public OutfitsResponse() { }
}
