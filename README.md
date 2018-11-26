# unity-client

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

- Be able to avoid loading on start by default (for remotely-fetched models that need to take some time to download)
- Have a 'finished loading asset callback' providing the time it took to load the GLTF asset (initially used for measuring loading times)

2. SpecGlossMap.cs and MetalRoughMap.cs were adapted to use "Lightweight Render Pipeline/Simple Lit" and "Lightweight Render Pipeline/Lit" shaders respectively (the original PbrMetallicRoughness and PbrSpecularGlossiness don't work with the Lightweight Render Pipeline)
