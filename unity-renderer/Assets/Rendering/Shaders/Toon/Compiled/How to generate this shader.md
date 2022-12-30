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

Search & Replace
```
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

        #include "PBRForwardPass.hlsl"
```
        

For the GPU skinning we need to do the same for the following passes:

Search & Replace
```
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        #include "DepthOnlyPass.hlsl"
```

```
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

        #include "DepthNormalsOnlyPass.hlsl"
```

```
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

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
## You have updated unity or URP Package?

> You can skip this part if you didn't

> If you did and this is not done, there may be some shaders that dont compile or may have visual glitches

Replace the contents of `PBRForwardPass.hlsl`

With the contents of `Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl`

Check **_VERY CAREFULY_** the changes from your DIFF tool, you should keep the custom changes that were made based on the git history.

> This step is very important, if not done, GPU Skinning will not work

Do the same with the following files:

- `DepthOnlyPass.hlsl`

- `DepthNormalsOnlyPass.hlsl`

- `ShadowCasterPass.hlsl`


## Apply Outliner pass
Add to both subshaders the outliner pass, you can find it in `OutlinerPass.txt`. 
This allows the render feature to reuse the Avatar material and avoid copying the huge matrices for GPU Skinning.
If not present, the outliner will fallback to a compatible material at a performance cost 


---
## You're done!.

Yes, the project needs a tool for automating this.  
