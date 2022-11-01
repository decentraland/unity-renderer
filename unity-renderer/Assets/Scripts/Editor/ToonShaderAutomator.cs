using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEditor;

public class ToonShaderAutomator : EditorWindow
{
    string toonShader = "ToonShaderCompiledOutline_R2_v2_2_RT";
    string toonShaderPath = "Assets/Rendering/Shaders/Toon/Compiled/";
    
    bool isFullUpdate = false;
    private bool isEssentialsOnlyUpdate = true;
    private bool isRenderStateOnlyUpdate = true;
    
    [MenuItem("Decentraland/TD/Shaders/Toon Shader Automator")]
    
    // Show window
    public static void ShowWindow()
    {
        GetWindow(typeof(ToonShaderAutomator));      //GetWindow is a method inherited from the EditorWindow class
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Toon Shader Automator", EditorStyles.boldLabel);

        toonShader = EditorGUILayout.TextField("Toon shader Name", toonShader);
        // add space
        GUILayout.Space(10);
        isFullUpdate = EditorGUILayout.Toggle("Full Update", isFullUpdate);
        // add space
        GUILayout.Space(4);
        isEssentialsOnlyUpdate = EditorGUILayout.Toggle("Essentials Only Update", isEssentialsOnlyUpdate);
        isRenderStateOnlyUpdate = EditorGUILayout.Toggle("Render States Only Update", isRenderStateOnlyUpdate);

        if (GUILayout.Button("Update Toon"))
        {
            PrepareFile();
        }
    }

    // basic replace text function
    void PrepareFile()
    {
        string endFilePath = toonShaderPath + toonShader + ".shader";
        string text = File.ReadAllText(toonShaderPath + toonShader + ".shader");

        bool checkShaderResult = CheckShader(text);

        if (checkShaderResult == true)
        {
            if (isRenderStateOnlyUpdate || isFullUpdate)
            {
                // Cull[_Cull]
                // Blend[_SrcBlend][_DstBlend]
                // ZTest LEqual
                // ZWrite[_ZWrite]
            
                ReplaceText(endFilePath , "Cull Back" , "Cull[_Cull]");
                ReplaceText(endFilePath , "Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha" , "Blend[_SrcBlend][_DstBlend]");
                
                
                ReplaceText(endFilePath , "ZWrite Off" , "ZWrite[_ZWrite]");
                
                //ReplaceText(endFilePath , "" , "");
            
                Debug.Log("Render State has been updated");
            }
        
            if (isEssentialsOnlyUpdate || isFullUpdate)
            {
                ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl", "PBRForwardPass.hlsl");
                ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl" , "DepthOnlyPass.hlsl");
                ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl" , "DepthNormalsOnlyPass.hlsl");
                ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl" , "ShadowCasterPass.hlsl");
            
                ReplaceText(endFilePath , "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl" , "../ShaderGraph/Includes/SampleTexture.hlsl");
            
                ReplaceText(endFilePath , "CBUFFER_START(UnityPerMaterial)" , "CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];");
            
                Debug.Log("Essentials have been updated");
                
                //ReplaceText(endFilePath , "" , "");
                
            }
        
            Debug.Log("Toon shader has been updated");
        }
        else
        {
            Debug.Log("Toon shader has not been updated");
        }

    }

    private bool CheckShader(string text)
    {
        if (text != null)
        {
            Debug.Log("Shader Found");
            return true;
        }
        else
        {
            Debug.Log("Shader Not Found");  
        }
        return false;
    }
    
    // Replace the Text in the file
    void ReplaceText(string filePath, string oldText, string newText)
    {
        string text = File.ReadAllText(filePath);
        text = text.Replace(oldText, newText);
        File.WriteAllText(filePath, text);
    }
}
