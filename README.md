# Decentraland Explorer

## Contributing

**Please read the [contribution guidelines](.github/CONTRIBUTING.md)**

### Before you start

1. [Pull Request Naming Standards](https://github.com/decentraland/standards/blob/master/standards/git-usage.md)
2. [Architecture Overview](https://docs.google.com/document/d/1_lzi3V5IDaVRJbTKNsNEcaG0L21VPydiUx5uamiyQnY/edit)
3. [Coding Guidelines](unity-client/style-guidelines.md)

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ and the latest version of GNU make, install it using `brew install make`
If you are using Windows 10 we recommend you to enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make...

## Running the Explorer

Make sure you have the following dependencies:
- Node v10 or compatible installed via `sudo apt install nodejs` or [nvm](https://github.com/nvm-sh/nvm)
- yarn installed globally via `npm install yarn -g`

IMPORTANT: If your path has spaces the build process will fail. Make sure to clone this repo in a properly named path.

Build the project:

    cd website
    npm install
    cd kernel
    npm install

To run and watch a server with the kernel build, run:

    make watch

Optionally, you can build the test scenes which are used in `debug` mode:

    make test-scenes

(If you want the test scenes to be updated dynamically after the first build, you should run `make watch-only-test-scenes` in another terminal and keep it watching for changes)

Once the kernel is running, to run the Unity interface you will have to:

1. Download and install Unity 2019.4.0f1
2. Open the Initial Scene
3. Run the Initial Scene in the Unity editor

Optionally, if you want to run the client in `debug` mode, append the following query parameter to the URL:

    http://localhost:3000/?DEBUG_MODE

To spawn in a specific set of coordinates append the following query paramter:

    http://localhost:3000/?DEBUG_MODE&position=10,10

### Troubleshooting

#### Missing git lfs extension
If while trying to compile the Unity project you get an error regarding some libraries that can not be added (for instance Newtonsoft
Json.NET or Google Protobuf), please execute the following command in the root folder:

    git lfs install
    git lfs pull

Then, on the Unity editor, click on `Assets > Reimport All`

#### Missing xcrun (macOS)
If you get the "missing xcrun" error when trying to run the `make watch` command, you should download the latest command line tools for macOS, either by downloading them from https://developer.apple.com/download/more/?=command%20line%20tools or by re-installing XCode


## Running tests

To see test logs/errors directly in the browser, run:

    make watch

Now, navigate to [http://localhost:3000/test](http://localhost:3000/test)

### Kernel Visual tests

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

### DCL scene in preview mode using a local Kernel version

1. In the explorer repo directory, run a `make watch` once
2. Kill the server and run `make npm-link`
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

### Adding/Updating protobuf-compiled components (instructions with macOS Homebrew package manager)

0. Install protobuf 3.12 (if a different version is already installed you have to uninstall it first, running `brew uninstall protobuf`):
    - edit the desired version config file (use code from https://github.com/Homebrew/homebrew-core/blob/53fb074d235fe0335fa8ee293a3f639f3cdffa45/Formula/protobuf.rb) replacing everything in the following file `code $(brew --repo homebrew/core)/Formula/protobuf.rb`
    - run `HOMEBREW_NO_AUTO_UPDATE=1 brew install protobuf`

1. Edit/add desired component structure at `kernel/packages/shared/proto/engineinterface.proto`

2. In that same directory (`kernel/packages/shared/proto/engineinterface.proto`) run the following command:
`protoc --plugin=../../../node_modules/ts-protoc-gen/bin/protoc-gen-ts --js_out="import_style=commonjs,binary:." --ts_out="." engineinterface.proto`

3. In that same directory run the following command:
`protoc --csharp_out=../../../../unity-client/Assets/Scripts/MainScripts/DCL/Models/Protocol/ --csharp_opt=base_namespace=DCL.Interface engineinterface.proto`

4. Make sure to also update the custom "save/read" code we have at:
    - `kernel/packages/scene-system/scene.system.ts` in `generatePBObject()`
    - `unity-client/Assets/Scripts/MainScripts/DCL/Controllers/Scene/MessageDecoder.cs`

### Shaders Scene-Forced PreWarm

To avoid extremely slow building times due to the Lightweight Render Pipeline shader variants compilation (LWRP shaders can't be packed in a 'shadervariants' file), we are using scene-instanced objects under the Environment/ShadersPrewarm/ prefab to force unity to pre-load them and be ready for using them on the fly. Please do not delete these objects.

### GLTF Dynamic Loading

We are using a custom version of the [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) as a Dynamic GLTF/GLB loader for unity to handle GLTF models.

### Unity Visual Tests Pipeline

#### How to create them

1. Create a new test class that inherits from VisualTestsBase
2. Initialize the visual tests using `VisualTestsBase.InitVisualTestsScene(string)` passing the test name as parameter
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
        yield return InitVisualTestsScene("VisualTestStub");

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

#### Option 1
1. Run `make watch` in a terminal at `kernel/`. Wait for it to finish and leave it watching for changes. **This step must be done before replacing the build files or they may be replaced again with the latest published build files when the `make watch` process runs**
2. In Unity Editor, build unity WASM with its name as `unity` into your desired folder. **It's very important that the folder/build name is `unity`**.
3. When the building process finishes, move the 3 ".unityweb" files that will end up inside `[ChosenDestination]/unity/Build/` into `kernel/static/unity/Build/`. Just remember that if you run the `make watch` after this last step, your new files may be replaced by the latest published build files during the `make watch` process.

Just to be clear, the .unityweb files should end up in the following path
```
kernel
 /- static
    /- unity
        /- Build
            /- unity.data.unityweb
            /- unity.wasm.code.unityweb
            /- unity.wasm.framework.unityweb
            /- ...
```

#### Option 2
1. Run `make watch` in a terminal at `kernel/`. Wait for it to finish and leave it watching for changes. **This step must be done before replacing the build files or they may be replaced again with the latest published build files when the `make watch` process runs**
2. Build unity WASM with its name as `unity` into the folder `kernel/static`. **It's very important that the folder/build name is `unity`**.
3. In another terminal checkout the deletion of the file named `kernel/static/unity/Build/DCLUnityLoader.js`. Unity deletes anything on this folder as part of the build process and we need that file.

```
git checkout -- kernel/static/unity/Build/DCLUnityLoader.js
```

4. Checkout the modifications in the `kernel/static/unity/Build/unity.json` file. Unity deletes our custom changes as part of the build process.

```
git checkout -- kernel/static/unity/Build/unity.json
```

5. Testing how your new build performs:

- Open **[http://localhost:3000/?DEBUG_MODE&LOCAL_COMMS&position=-100,100](http://localhost:3000/?DEBUG_MODE&LOCAL_COMMS&position=-100,100)** to go to an area with a high density of test parcels.
- Open **[http://localhost:3000/?DEBUG_MODE&LOCAL_COMMS&ENV=org&position=10,0](http://localhost:3000/?DEBUG_MODE&LOCAL_COMMS&ENV=org&position=10,0)** to open an area with real-life deployments (but without communicating with other users).
- Open **[http://localhost:3000/?ENV=org&position=0,90](http://localhost:3000/?ENV=org&position=0,90)** to open the explorer near the Decentraland Museum

### Profiling a build with Unity Editor
1. In Unity Editor open the profiler at window -> analysis -> profiler
2. Open the build settings and toggle on "Development Build", "Autoconnect Profiler" and **make sure Deep Profiling is toggled off** before starting the build
3. Follow the steps at this readme's "Making a manual build" section
4. Open the new build (http://localhost:3000/?ENV=org) and after starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data

#### Important notes and recommendations
* The same Unity Editor instance that made the build **shouldn't be closed**, as that will make the new build useless for profiling, at least at the time of writting this, **it only sends the data to the same Unity Editor instance that built it**.

* Since the profiler data comes from the build, you can't "pause the game" and analyze the data, that should be done toggling the "record" button, keep in mind that it doesn't work as expected when re-toggled, and you probably have to toggle/untoggle it several times to start receiving the data again, in the same session in which the button was untoggled to analyze the data.

* Recommended setup: Only CPU, Rendering and Memory modules on display; Disable VSync and "Others" from CPU module.

### Builder Integration

The following layers were created for builder functionality:
-Ground: used to facilitate ground raycasting and objects movement-
-Gizmo: used to identify the gizmos and effects.
-Selected: used for selected object effects.

### Native Kernel-Renderer Interface

In some cases, using `SendMessage` is not performant enough to pass data
from Kernel to the Unity Renderer. To address that we added a new native
bridge that ends up giving JS the ability to call C# methods directly.

The following files are involved

* *kernelNativeBridge.c / .h:* Here lies the glue between C# and JS. A
  few macros have been defined to define methods that will be exposed
  from JS and C# in order to pass the function pointers that we need to
  make the direct calls.

* *EntryPoint_World.cs:* Here, the `SetCallback` methods defined in
  `kernelNativeBridge` have to be called in order to have a C# method
  recognized and callable by JS.

* *nativeMessagesBridge.ts*: In this file we are initializing the
  JS-side call methods.

#### Step by step guide to add a new direct call function

* Add the function in the `kernelNativeBridge.c` file. This just
  involves adding `EXTERNAL_CALLBACK_<signature>(<method-name>)` line to
  the file. Note that the signature has to be specified.

* If the method
  has a signature not defined yet, you have to add a new macro that
  should use a new function pointer type declared in
  `kernelNativeBridge.h`. This macro will generate the
  `SetCallback_<method-name>` and `call<method-name>` functions used
  later.

* Bind the function in `EntryPoint_World.cs`. For this you will need the
  method signature delegate. Again, if the signature is new, you'll have
  to create a new one. When you have the delegate, you have to define a
  static method, looking like this:
  ```
    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    private static void Foo(string id)
    {
        //your code
    }
  ```
  Remember, the method has to be static and has to use the
  `MonoPInvokeCallback` attribute with the proper delegate. Otherwise,
  compilation will fail and the methods will not be bound correctly.

* Put the `SetCallback_Foo` call in `World_EntryPoint` initialization
  with the method as a param. This will set the function pointer wasm
  side, finishing the "glue" between C# and JS.
  ```
    public EntryPoint_World(SceneController sceneController)
    {
        ...
        SetCallback_Foo(Foo);
  ```

*  Now you have to call the method. This example code would call the
   `Foo` method from JS. Remember that you need the `Module` instance
   that lies inside the unity instance JS side returned by the
   `UnityLoader` construct. Right now, this is already solved in
   `nativeMessagesBridge.ts`. The following example should add the `Foo`
   method to it.

   ```
   private callFoo!: (arg: string) => void
   ...
   public initNativeMessages(gameInstance: any) {
    ...
    this.callFoo = this.unityModule.cwrap('call_Foo', null, ['string'])
    ...
   }
   ```

   And later...

   ```
   this.callFoo("bar") // finally calling the function
   ```

* We did it!. Remember: this is no replacement of `SendMessage`.
  `SendMessage` is still used by our WSS debug mode. So, you'll have to
  always provide a `SendMessage` alternative and bind it correctly to
  our `WSSController`. From Kernel, you can branch between native and
  `SendMessage` calls using a snippet like this:
  ```
    if (WSS_ENABLED || FORCE_SEND_MESSAGE) {
      // SendMessage
    } else {
      // Native call
    }
  ```

* As you can see, binding native calls has a lot of maintenance costs,
  so its better to just bind calls that really need that performance
  speed up. If you need benchmarks, this approach is inspired by
  [this](https://forum.unity.com/threads/super-fast-javascript-interaction-on-webgl.382734/)
  unity forum post showing some numbers.


## Creating a typescript Worker in Kernel
You can check the files used for our simple gif-processor worker (`packages/gif-processor`):
1. Create relevant folder inside `packages/` and put a .ts file for the main thread and another .ts for the worker (gif-processor example: `processor.ts` and `worker.ts`)
2. copy the tsconfig.json file from `packages/gif-processor` into the new folder, rename and edit its "OutDir" value to point to a new folder inside `/static/`
3. Duplicate the `targets/engine/gif-processor.json` file, rename it and change its "file" value to point to your worker .ts file
4. Inside the `Makefile` file create a new path constant (like the one for `GIF_PROCESSOR`) and set the path to your compiled worker file (the folder you created in step 2 plus 'worker.ts'). Then add that new constant inside `build-essentials` like `GIF_PROCESSOR`
5. Inside the `Makefile` add the following 2 lines with your own paths:
```
static/gif-processor/worker.js: packages/gif-processor/*.ts
	@$(COMPILER) targets/engine/gif-processor.json
```
6. You should be able to import your package that uses the Worker anywhere. Beware of the [limitations when passing data to/from workers](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API/Structured_clone_algorithm) and also consider [passing the data as Transferable objects](https://developer.mozilla.org/en-US/docs/Web/API/Transferable) to improve performance.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
