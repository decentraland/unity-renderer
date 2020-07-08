# Decentraland Explorer

## Contributing

**Please read the [contribution guidelines](.github/CONTRIBUTING.md)**

### Before you start

1. [Pull Request Naming Standards](https://github.com/decentraland/standards/blob/master/standards/git-usage.md)
2. [Architecture Overview](https://docs.google.com/document/d/1_lzi3V5IDaVRJbTKNsNEcaG0L21VPydiUx5uamiyQnY/edit)
3. [Coding Guidelines](code-guidelines.md)

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ and the latest version of GNU make, install it using `brew install make`
If you are using Windows 10 we recommend you to enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make...

## Running the kernel

Make sure you have the following dependencies:  
- Node v10 or compatible installed 
- yarn installed globally via `npm install yarn -g` 

Build the project:

    cd kernel
    npm install
    make build-essentials

To run and watch a server with the kernel build, run:

    make watch

Optionally, you can build the test scenes which are used in `debug` mode:

    make test-scenes

To run the Unity interface:

1. Download and install Unity 2019.1.14f1 or a later 2019.1 version (note that 2019.2 does not work!)
2. Open the Initial Scene
3. Run the Initial Scene in the Unity editor!

To run the client in `debug` mode append the following query parameter to the URL:

    http://localhost:8080/?DEBUG_MODE

To run the client in first person perspective append the following query parameter to the URL:

    http://localhost:8080/?DEBUG_MODE&fps

To spawn in a specific set of coordinates append the following query paramter:

    http://localhost:8080/?DEBUG_MODE&fps&position=10,10

## Running tests

To see test logs/errors directly in the browser, run:

    make watch

Now, navigate to [http://localhost:8080/test](http://localhost:8080/test)

### Visual tests

Visual tests are meant to work in a similar way as `snapshot tests`. Each time a test parcel changes the author is required to commit new screenshots along the other changes. These screenshots are then validated to detect regressions at the time of the pull request. To generate new snapshot images to compare run `npm run test:dry` (it requires docker)

### Test parcels

It is possible to define new parcels inside this repo for testing purposes. To do so, create a new folder in `public/test-scenes`. There are several conventions to be followed regarding the name of these folders and the positions of the parcels, these can be found in the [README](https://github.com/decentraland/client/blob/master/public/test-scenes/README.md) file.

To edit and make sure that `make watch` is rebuilding the scene when you are hacking on a new feature of the kernel, make sure to modify `targets/scenes/basic-scenes.json` and point to the scene you're working on.

All test parcels can be accessed inside visual tests:

```ts
import { loadTestParcel } from 'test/testHelpers'

describe('My example test', function() {
  loadTestParcel(200, 10)
  // ...
```

### Unity Editor debugging with dcl scene in "preview mode"

1. Run 'dcl start' to open the scene in preview mode. Leave the server running and close the newly-opened browser tab
2. In Unity Editor, in the "InitialScene" scene, select the WSSController gameobject and untoggle the "Open Browser When Start", and toggle the "Use client debug mode". (Make sure the SceneController has the "Debug Scenes" toggle OFF)
3. In Unity Editor hit PLAY
4. In a new browser tab go to http://localhost:8000/?UNITY_ENABLED=true&DEBUG_MODE&LOCAL_COMMS&position=0%2C-1&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl
5. Go back to unity and the scene should start loading in there almost immediately

### DCL scene in preview mode using a local explorer version

1. In the explorer repo directory, run a `make watch` or `make dev-watch` once
2. Kill the server and run `make initialize-ecs-npm-link`
3. In the scene directory run `npm link decentraland-ecs`
4. In the scene directory run `dcl start` and it should already be using the local version of the client

### Unity Assembly Definition Files

To be able to use the Test libraries for unit testing, we are using several [Assembly Definition Files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html):

#### MainScripts.asmdef

This assembly contains all of our custom classes and scripts (which must stay inside its directory or in its hierarchy below).

#### Other .asmdef files

On every component directory there is a /test folder with an assembly definition file marked as a test assembly so that it gets stripped when building the project and references the test libraries for assertions. Also, these .asmdef files reference the MainScripts.asmdef to be able to have our own classes available at unit tests code.

### Component implementation high-level guidelines

Before implementing a component on unity's side, it's recommended to check in the [explorer](https://github.com/decentraland/explorer) repo for the same component and verify whether it is a Disposable/Shared component or not, as their implementation pipeline differs.
Every component declaration should be under the **/packages/decentraland-ecs/src/decentraland/** directory (Be aware that your IDE may find the component also in a ".ts" script, dismiss those declarations as they correspond to interfaces for the components in typescript).

#### Entity/Non-Shared/Non-Disposable component:

- Make sure the corresponding CLASS_ID_COMPONENT value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
- Create the component script and prefab in MainScripts/DCL/Components/[Corresponding Folder]/
- From the Unity editor, update the MainScripts/DCL/Factory/DCLComponentFactory scriptable object adding the new element in the Factory List with the correct CLASS_ID_COMPONENT value and its prefab reference

#### Shared/Disposable component:

- Make sure the corresponding CLASS_ID value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
- Create the component script in MainScripts/DCL/Components/[Corresponding Folder]/
- Edit the SharedComponentCreate() method in MainScripts/DCL/Controllers/Scene/ParcelScene.cs to make sure it instantiates the new shared component.

### Shaders Scene-Forced PreWarm

To avoid extremely slow building times due to the Lightweight Render Pipeline shader variants compilation (LWRP shaders can't be packed in a 'shadervariants' file), we are using scene-instanced objects under the Environment/ShadersPrewarm/ prefab to force unity to pre-load them and be ready for using them on the fly. Please do not delete these objects.

### GLTF Dynamic Loading

We are using [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) as a Dynamic GLTF/GLB loader for unity to handle GLTF models.

#### Local changes made to UnityGLTF plugin

##### NOTE: UnityGLTF plugin update is discouraged until Unity WebGL supports multi-threading

1. GLTFComponent.cs has been adapted to:

- Be able to avoid loading on start by default (for remotely-fetched models that need to take some time to download)
- Have a 'finished loading asset callback' providing the time it took to load the GLTF asset (initially used for measuring loading times)
- StartCoroutine has been replaced by StartThrowingCoroutine so we catch the invalid GLTF assets gracefully.

2. SpecGlossMap.cs and MetalRoughMap.cs were adapted to use "Lightweight Render Pipeline/Simple Lit" and "Lightweight Render Pipeline/Lit" shaders respectively (the original PbrMetallicRoughness and PbrSpecularGlossiness don't work with the Lightweight Render Pipeline)

3. Several files were modified to replce Tasks (multi-threading) with Coroutines as Unity WebGL build doesn't support multi-threading.

4. Animation curve processing methods were adapted to be spread through many frames.

5. GameObject reparenting is made as soon the root GLTF loading object is created, so a big mesh can be seen in place before the loading finished.

### Visual Tests Pipeline

#### How to create them

1. Create a new test class that inherits from VisualTestsBase
2. After the `InitScene()` call, initialize the visual tests using `VisualTestHelpers.InitVisualTestsScene(string)` passing the test name as parameter
3. Setup your scene as wanted and call `TestHelpers.TakeSnapshot(Vector3)`
4. Tag the method with the attribute `[VisualTest]`. This isn't used yet but will be used to streamline the baseline images creation.

The pngs will be named automatically using the `InitVisualTestsScene` parameter.

Example:

```
public class VisualTests : VisualTestsBase
{
    [UnityTest][VisualTest]
    public IEnumerator VisualTestStub()
    {
        yield return InitScene();
        yield return VisualTestHelpers.InitVisualTestsScene("VisualTestStub");

        // Set up scene

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(10f, 10f, 0f));
    }
}
```

#### How to create visual tests baseline images

1. Create a new test inside the same class of the desired visual test. Give it the same name followed by `_Generate`.
2. call `VisualTestHelpers.GenerateBaselineForTest(IEnumerator)` inside the method, passing the actual test method as parameter.
3. remember to make the test `[Explicit]` or the test will give false positives

Example:

```
[UnityTest][Explicit]
public IEnumerator VisualTestStub_Generate()
{
    yield return VisualTestHelpers.GenerateBaselineForTest(VisualTestStub());
}
```

### Making a manual build

1. Build unity WASM with its name as `unity` into the folder `/static`. **It's very important that the folder/build name is `unity`**.
2. Checkout the file named `static/unity/Build/DCLUnityLoader.js`. Unity deletes anything on this folder as part of the build process and we need that.

```
git checkout -- static/unity/Build/DCLUnityLoader.js
```

4. Checkout the modifications in the `static/unity/Build/unity.json` file. Unity deletes our custom changes as part of the build process.

```
git checkout -- static/unity/Build/unity.json
```

5. Run `make watch`.
6. Testing how your new build performs:

- Open **[http://localhost:8080/?DEBUG_MODE&LOCAL_COMMS&position=-100,100](http://localhost:8080/?DEBUG_MODE&LOCAL_COMMS&position=-100,100)** to go to an area with a high density of test parcels.
- Open **[http://localhost:8080/?DEBUG_MODE&LOCAL_COMMS&ENV=org&position=10,0](http://localhost:8080/?DEBUG_MODE&LOCAL_COMMS&ENV=org&position=10,0)** to open an area with real-life deployments (but without communicating with other users).
- Open **[http://localhost:8080/?ENV=org&position=0,90](http://localhost:8080/?ENV=org&position=0,90)** to open the explorer near the Decentraland Museum

### Builder Integration

The following layers were created for builder functionality:
-Ground: used to facilitate ground raycasting and objects movement-
-Gizmo: used to identify the gizmos and effects.
-Selected: used for selected object effects.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
