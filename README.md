# unity-client

### Reading Guide
1. [Push Request naming standards](https://github.com/decentraland/standards/blob/master/standards/git-usage.md)
2. [Unity client architecture overview](https://docs.google.com/document/d/1_lzi3V5IDaVRJbTKNsNEcaG0L21VPydiUx5uamiyQnY/edit)
3. [Coding guidelines](code-guidelines.md)


### Setup the Explorer project
1. Run `npm install` in the [Explorer](https://github.com/decentraland/explorer) cloned repo root directory
2. Run `make watch` in the [Explorer](https://github.com/decentraland/explorer) cloned repo root directory and wait for the make script to complete
3. Once the explorer automatically opens you a browser tab, you may close it and continue with Unity 
3. Run the Initial Scene in the Unity editor
4. Run the build by accessing **[http://localhost:8080/tetra.html?DEBUG&position=-103%2C99&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl](http://localhost:8080/tetra.html?DEBUG&position=-103%2C99&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl)** in any webbrowser

### Build Unity Artifact

1. Build unity WASM with its name as "unity" into [Explorer](https://github.com/decentraland/explorer) cloned repo **root/static/** directory
2. Run the command `make watch`
2. Run the build by accessing **[http://localhost:8080/tetra.html?DEBUG&position=-101%2C99](http://localhost:8080/tetra.html?DEBUG&position=-101%2C99)** in any webbrowser

### Unity Assembly Definition Files

To be able to use the Test libraries for unit testing, we are using several [Assembly Definition Files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html):

#### MainScripts.asmdef

This assembly contains all of our custom classes and scripts (which must stay inside its directory or in its hierarchy below).

#### Other .asmdef files

On every component directory there is a /test folder with an assembly definition file marked as a test assembly so that it gets stripped when building the project and references the test libraries for assertions. Also, these .asmdef files reference the MainScripts.asmdef to be able to have our own classes available at unit tests code.

### Component implementation high-level guidelines

Before implementing a component on unity's side, it's recommended to check in the [CLIENT](https://github.com/decentraland/client) repo for the same component and verify whether it is a Disposable/Shared component or not, as their implementation pipeline differs.
Every component declaration should be under the **/packages/decentraland-ecs/src/decentraland/** directory (Be aware that your IDE may find the component also in a ".ts" script, dismiss those declarations as they correspond to interfaces for the components in typescript).

#### Entity/Non-Shared/Non-Disposable component:

-   Make sure the corresponding CLASS_ID_COMPONENT value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
-   Create the component script and prefab in MainScripts/DCL/Components/[Corresponding Folder]/
-   From the Unity editor, update the MainScripts/DCL/Factory/DCLComponentFactory scriptable object adding the new element in the Factory List with the correct CLASS_ID_COMPONENT value and its prefab reference

#### Shared/Disposable component:

-   Make sure the corresponding CLASS_ID value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
-   Create the component script in MainScripts/DCL/Components/[Corresponding Folder]/
-   Edit the SharedComponentCreate() method in MainScripts/DCL/Controllers/Scene/ParcelScene.cs to make sure it instantiates the new shared component.

### Shaders Scene-Forced PreWarm

To avoid extremely slow building times due to the Lightweight Render Pipeline shader variants compilation (LWRP shaders can't be packed in a 'shadervariants' file), we are using scene-instanced objects under the Environment/ShadersPrewarm/ prefab to force unity to pre-load them and be ready for using them on the fly. Please do not delete these objects.

### GLTF Dynamic Loading

We are using [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) as a Dynamic GLTF/GLB loader for unity to handle GLTF models.

#### Local changes made to UnityGLTF plugin

##### NOTE: UnityGLTF plugin update is discouraged until Unity WebGL supports multi-threading

1. GLTFComponent.cs has been adapted to:

-   Be able to avoid loading on start by default (for remotely-fetched models that need to take some time to download)
-   Have a 'finished loading asset callback' providing the time it took to load the GLTF asset (initially used for measuring loading times)
-   StartCoroutine has been replaced by StartThrowingCoroutine so we catch the invalid GLTF assets gracefully.

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

### Known Issues

-   Regarding Basic Materials: If the **alphaTest** value is set on a basic material and the **Assets/Resources/Materials/BasicShapeMaterial** is viewed in the Inspector (be it by selecting the material in the Project tab or by expanding its details from a renderer in the Inspector), its "Alpha Clip" toggle gets untoggled (and the material file modified). We suspect a Unity bug.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
