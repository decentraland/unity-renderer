using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Models;
using UnityEngine;

public class NFTShapePlugin : IPlugin
{
    private IRuntimeComponentFactory factory => DCL.Environment.i.world.componentFactory;

    public NFTShapePlugin()
    {
        // TODO(Brian): Move all the NFTShape files to the plugin's directory.
        factory.RegisterBuilder((int) CLASS_ID.NFT_SHAPE, BuildComponent);
    }

    NFTShape BuildComponent()
    {
        return new NFTShape(new NFTInfoRetriever(), new NFTAssetRetriever());
    }

    public void Dispose()
    {
        factory.UnregisterBuilder((int) CLASS_ID.NFT_SHAPE);
    }
}