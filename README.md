# unity-client

### Live server communication build process

1. run "npm install" in the [CLIENT](https://github.com/decentraland/client) cloned repo root directory
2. run "make watch" in the CLIENT cloned repo root directory and wait for the make script to complete
3. Build unity wasm with its name as "unity" into CLIENT cloned repo **root/static/** directory
4. Run the build by accessing **http://localhost:8080/tetra.html?DEBUG&position=-101%2C99** in any webbrowser

### Run client in unity

1. run "npm install" in the [CLIENT](https://github.com/decentraland/client) cloned repo root directory
2. run "make watch" in the CLIENT cloned repo root directory and wait for the make script to complete
3. Run the Initial Scene in the editor
4. Run the build by accessing **http://localhost:8080/tetra.html?DEBUG&position=-103%2C99&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl** in any webbrowser

### Unity Assembly Definition Files

To be able to use the Test libraries for unit testing, we are using several [Assembly Definition Files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html):

#### MainScripts.asmdef

This assembly contains all of our custom classes and scripts (which must stay inside its directory or in its hierarchy below).

#### PlayModeTests.asmdef and EditModeTests.asmdef

These assemblies are marked as test assemblies so that they get stripped when building the project. Also, PlayModeTests.asmdef references MainScripts.asmdef to be able to have our own classes available at unit tests code.

### Shaders Scene-Forced PreWarm

To avoid extremely slow building times due to the Lightweight Render Pipeline shader variants compilation (LWRP shaders can't be packed in a 'shadervariants' file), we are using scene-instanced objects under the Environment/ShadersPrewarm/ prefab to force unity to pre-load them and be ready for using them on the fly. Please do not delete these objects.

### GLTF Dynamic Loading

We are using [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) as a Dynamic GLTF loader for unity to handle GLTF models.

#### Local changes made to UnityGLTF plugin

1. GLTFComponent.cs has been adapted to:

-   Be able to avoid loading on start by default (for remotely-fetched models that need to take some time to download)
-   Have a 'finished loading asset callback' providing the time it took to load the GLTF asset (initially used for measuring loading times)

2. SpecGlossMap.cs and MetalRoughMap.cs were adapted to use "Lightweight Render Pipeline/Simple Lit" and "Lightweight Render Pipeline/Lit" shaders respectively (the original PbrMetallicRoughness and PbrSpecularGlossiness don't work with the Lightweight Render Pipeline)

### Known Issues

-   GLTF Dynamic Loading: Currently, only **GLB** format is supported with Khrono's [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) loader, GLTF dynamic loading doesn't work even in their example unity project. Soon Unity will support GLTFs natively (Unity declared that for its future 2019.1 release). When that feature gets implemented we'll remove khrono's loader from the project.

-   Regarding Basic Materials: If the **alphaTest** value is set on a basic material and the **Assets/Resources/Materials/BasicShapeMaterial** is viewed in the Inspector (be it by selecting the material in the Project tab or by expanding its details from a renderer in the Inspector), its "Alpha Clip" toggle gets untoggled (and the material file modified). We suspect a Unity bug.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
