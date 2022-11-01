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

        if (isEssentialsOnlyUpdate)
        {
            ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl", "PBRForwardPass.hlsl");
            ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl" , "DepthOnlyPass.hlsl");
            ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl" , "DepthNormalsOnlyPass.hlsl");
            ReplaceText(endFilePath , "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl" , "ShadowCasterPass.hlsl");
            
            ReplaceText(endFilePath , "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl" , "../ShaderGraph/Includes/SampleTexture.hlsl");
            
            ReplaceText(endFilePath , "CBUFFER_START(UnityPerMaterial)" , "CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];");
            
            //ReplaceText(endFilePath , "" , "");
            //ReplaceText(endFilePath , "" , "");
        }
        
    }
    
    // Replace the Text in the file
    void ReplaceText(string filePath, string oldText, string newText)
    {
        string text = File.ReadAllText(filePath);
        text = text.Replace(oldText, newText);
        File.WriteAllText(filePath, text);
    }
}
