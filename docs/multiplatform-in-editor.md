# Multiplaform in the Editor - How Tos

## Launching the world in Editor

The entry point to launch decentraland is `Assets/Scenes/InitialScene.unity`. There you will find the `bootstrapper` which behaviour differs between Editor and builds.

In Editor it will let you select which platform to launch (through prefab instantiation) from a dropdown. In build it will instantiate the proper prefab without further interaction.

![Bootstrapper](multiplatform-in-editor/bootstrapper.png)

## Assets Stripping

Each platform should only carry the common assets plus their specifics, Therefore the build process will strip whatever is not needed.

You can also simulate these conditions using the top menu `Decentraland/Set Platform`. The assets stripping is performed renaming the root folder of the platform following [Unity's special folder naming](https://docs.unity3d.com/Manual/SpecialFolders.html). Currently all the WebGL specifics is part of the core of the project, so we cannot strip it, but luckily Desktop (and other platforms as VRs) have been built to dealt with this. In the future we will extract all these specifics to a new path `Assets/WebGL` and the core to `Assets/_Common`. So to summarize:

- `Only WebGL`: Would delete everything under `Assets/Desktop` as the CI would do (and in the future any other platform we might have).
- `Only Desktop`: For now will restore `Assets/Desktop` and nothing else. In the future it should remove an hypothetical `Assets/WebGL` folder or any other platform.
- `Restore All`: It will restore every platform in the system, currently `Assets/Desktop`.

![TopMenu](multiplatform-in-editor/topmenu.png)