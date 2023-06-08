using System;
using System.Collections.Generic;

[Serializable]
public class OutfitsResponse
{
    public List<OutfitItem> elements;
    public OutfitsResponse(List<OutfitItem> elements)
    {
        this.elements = elements;
    }

    public OutfitsResponse() { }
}
