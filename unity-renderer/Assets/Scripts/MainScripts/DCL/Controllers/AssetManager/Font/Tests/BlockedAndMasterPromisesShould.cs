using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Font_Tests
{
    // NOTE: Inheritance from TestBase_APK will include all the test with generics 
    public class BlockedAndMasterPromisesShould : TestsBase_APK<AssetPromiseKeeper_Font,
        AssetPromise_Font,
        Asset_Font,
        AssetLibrary_RefCounted<Asset_Font>>
    {
    }
}