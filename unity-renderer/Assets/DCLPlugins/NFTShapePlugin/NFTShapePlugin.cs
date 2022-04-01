using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Models;
using UnityEngine;

public class NFTShapePlugin : IPlugin
{
    public NFTShapePlugin()
    {
        // TODO(Brian): Move all the NFTShape files to the plugin's directory.
        var factory = DCL.Environment.i.world.componentFactory;
        factory.RegisterComponentBuilder((int) CLASS_ID.NFT_SHAPE, BuildComponent);
    }

    NFTShape BuildComponent()
    {
        return new NFTShape(new NFTInfoLoadHelper(), new NFTAssetLoadHelper());
    }

    public void Dispose()
    {
        // TODO(Brian): Unregister the component from the factory
    }
}