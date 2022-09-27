# How to use Asset Bundle Converter

## Context

In order to serve GLTFs more efficiently using less CPU in runtime, they are converted by a background process from GLTFs to Unity Asset Bundles when a scene is deployed.

## Unity: assets conversion flow

Currently, the Asset Bundles Converter code lives inside [unity-renderer repo](https://github.com/decentraland/unity-renderer). Eventually we'll probably move it to its own repo.

The most relevant classes are:

- [unity-renderer/Assets/ABConverter/Core.cs](https://github.com/decentraland/unity-renderer/blob/master/unity-renderer/Assets/ABConverter/Core.cs)
    - Downloads assets (and their dependencies) and flags them to be converted
    - Converts downloaded assets to AB
    - Triggers visual tests check after conversion
- [unity-renderer/Assets/ABConverter/SceneClient.cs](https://github.com/decentraland/unity-renderer/blob/master/unity-renderer/Assets/ABConverter/SceneClient.cs)
    - Entrypoint for converting scenes and their assets
    - Uses the Core to trigger different type of conversions (Scene, Asset)
    - Has the batch-mode entrypoint for converting a scene (`ExportSceneToAssetBundles()`**)**
- [unity-renderer/Assets/ABConverter/WearablesCollectionClient.cs](https://github.com/decentraland/unity-renderer/blob/master/unity-renderer/Assets/ABConverter/WearablesCollectionClient.cs)
    - Entrypoint for converting wearable collections
    - Uses the Core to trigger wearable and wearable collection conversions

### Conversion flow

1. Target assets are downloaded into a folder for each asset (folder names are the asset hashes), Unity editor imports each asset (constructs materials for the GLTFs, etc.) and those folders are flagged for Asset Bundle conversion (`Core.DumpAssets()`)
2. A depmap json file is created inside each folder and then the flagged folders are all converted in one pass (`Core.BuildAssetBundles()`) (In a previous version of the converter we generated the depmaps as an external json file "assethash.depmap", we don't do this anymore but the depmaps loading system has a fallback to keep working with those old depmaps for old asset bundles)
3. Target assets are visually-tested against their asset bundle version (`VisualTests.TestConvertedAssets()`)
    1. GLTF assets are instantiated in the current scene (disabled)
    2. Each GLTF GameObject is enabled at a time and a snapshot is taken for each asset
    3. Asset Bundle assets are instantiated in the current scene (disabled)
    4. Each Asset Bundle GameObject is enabled at a time, taking a  snapshot for each asset and generating the "diff" image for the assets that don't look exactly the same in both asset versions. When that happens, **the failed Asset Bundle file is deleted from the output folder**, so it's not uploaded to the S3.
4. The final asset bundles will end up in the output folder at `unity-renderer/unity-renderer/AssetBundles/`

### Converter Exit Codes

```jsx
0 = SUCCESS
1 = UNDEFINED
2 = SCENE_LIST_NULL
3 = ASSET_BUNDLE_BUILD_FAIL
4 = SOME_ASSET_BUNDLES_SKIPPED
```

### Converting assets locally

For dumping/converting assets locally we use our **Decentralanad →Asset Bundle Builder** menu items.

![Screen Shot 2021-11-16 at 13.26.56.png](how-to-use-asset-bundle-coverter/Screen_Shot_2021-11-16_at_13.26.56.png)

Those menu items can be edited at `[Assets/ABConverter/AssetBundleMenuItems.cs](https://github.com/decentraland/unity-renderer/blob/master/unity-renderer/Assets/ABConverter/AssetBundleMenuItems.cs)`

Another option is to call the converter locally from the terminal in headless mode [as it’s done in the converter service](https://github.com/decentraland/unity-renderer/blob/master/convert-asset-bundles.sh) e.g. `/Applications/Unity/Hub/Editor/2020.3.0f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "/Users/pravus/git/unity-renderer/unity-renderer" -batchmode -executeMethod DCL.ABConverter.SceneClient.ExportSceneToAssetBundles -sceneCid "QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM" -output "/Users/pravus/git/unity-renderer/unity-renderer/AssetBundles"`

### Scenes

Normally for scene-related dumping we need the scene id and asset hashes, this can be found by checking the scene mappings json, for example (coords -17, -122): `[https://peer.decentraland.org/content/entities/scene/?pointer=-17,-122](https://peer.decentraland.org/content/entities/scene/?pointer=-17,-122)`

### Wearable collections

Since there are LOTS of wearable collections, right now the only way to dump them manually is by updating the  `initialCollectionIndex` and `lastCollectionIndex` variables inside `WearablesCollectionClient.DumpAllNonBodyshapeWearables()`

### Testing and confirming conversion status manually

The best way to test the ABs conversion went fine is the following sequence:

1. Check out the visual test results for each asset at `unity-renderer/unity-renderer/TestResources/VisualTests/CurrentTestImages/`
2. start a webserver at the asset bundles output folder at port `1338` for example with `npx serve --cors -l 1338` (other options could be `http-server -p 1338` or `python -m SimpleHTTPServer 1338`), to serve those newly created asset bundles
3. Go back to Unity Editor and select the DebugConfig GameObject in the InitialScene unity scene. Check the "Use Custom Content Server" toggle and make sure the custom content server url is [`http://localhost:1338/`](http://localhost:1338/)
4. Clear the editor's Asset Bundles cache by going to the menu item at **Decentraland → Clear Asset Bundles Cache**
5. Enter the world running the explorer from the editor, and make sure the pertinent loaded meshes name doesn't say "GLTF:HASH" that way you can make sure it's an asset bundle that's being loaded (the parent of the mesh GameObject may say GLTFShape but that's fine since that's the SDK component name)

## Pre-process pipeline

[Specific optimizers for explorer assets - RFC](https://rfc.decentraland.org/rfc/RFC-8)

## Loading Asset Bundles in runtime

In runtime the GLTF/AssetBundle loading pipeline goes through the `RendereableAssetLoadHelper.Load()` from there you can follow the loading flow (`RendereableAssetLoadHelper.LoadAssetBundle()` and `RendereableAssetLoadHelper.LoadGltf()` ).
