## How to generate this shader cheat-sheet:

- Go to `Master_ToonShader`, look at the inspector and click on "View Generated Shader"
- Overwrite `ToonShaderCompiled.shader` contents with the generated shader.
- Rename the shader to `DCL/Toon Shader`
- Look for the passes with `UniversalPipeline` tag, there should be two of them.

### For each "UniversalPipeline" pass:

#### Replace everything under "Render State" comment for the following:

```
    Cull [_Cull]
    Blend [_SrcBlend] [_DstBlend]
    ZTest LEqual
    ZWrite [_ZWrite]
```

> NOTE: If this is not done, transparent wearables will not work.
---
### Tags cleanup

Remove all the tags, the only tag left should be "UniversalPipeline". The tags for each pass should look like this:

```
Tags
{
  "LightMode" = "UniversalForward"
}
```

> NOTE: If the tags aren't removed, the avatars will be rendered on the Transparent stage of the pipeline. That's bad!.
---
### Change forward pass for custom one

Do a search of replace of "PBRForwardPass.hlsl". You will note that the include takes the file from the URP package. The
custom pass in this project should be used instead.

To achieve this, you must replace all the includes of this file for just:

    #include "PBRForwardPass.hlsl"

For the GPU skinning we need to do the same for the following passes:
```    
#include "DepthOnlyPass.hlsl"
#include "DepthNormalsOnlyPass.hlsl"
#include "ShadowCasterPass.hlsl"
```
> NOTE: If this is not done, avatars aren't going to look toon.
---
### Replace the absolutes paths to relative paths

Search
```
#include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"
```

and replace it for

```
#include "../ShaderGraph/Includes/SampleTexture.hlsl"
```

> NOTE: If this is not done, the shader will fail on the project `explorer-desktop`.

---
### CBuffer fix for Macos and maybe others

Search
```
            CBUFFER_START(UnityPerMaterial)
```
and replace for
```
            CBUFFER_START(UnityPerMaterial)
            float4x4 _WorldInverse;
            float4x4 _Matrices[100];
            float4x4 _BindPoses[100];
```
> NOTE: If this is not done, GPU Skinning will stop working

---
## You're done!.

Yes, the project needs a tool for automating this.  
