using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class BuildGame {
    public static void BuildWebGL(){
        JumpTests.Run();
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue=1;settings.ApplyModifiedPropertiesWithoutUndo();
        if(!AssetDatabase.IsValidFolder("Assets/Resources"))AssetDatabase.CreateFolder("Assets","Resources");
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Resources/Renderer.asset");
        if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Resources/Renderer.asset");}
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Resources/Pipeline.asset");
        if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Resources/Pipeline.asset");}
        pipeline.supportsHDR=false;pipeline.msaaSampleCount=2;
        GraphicsSettings.defaultRenderPipeline=pipeline;
        for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
        if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Surface.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/Resources/Surface.mat");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Jump Jump").AddComponent<JumpGame>();EditorSceneManager.SaveScene(scene,"Assets/JumpJump.unity");
        PlayerSettings.productName="Jump Jump";PlayerSettings.companyName="Pocket Arcade";PlayerSettings.bundleVersion="1.0.0";
        PlayerSettings.defaultScreenWidth=1100;PlayerSettings.defaultScreenHeight=750;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.template="PROJECT:Itch";
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
        AssetDatabase.SaveAssets();
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/JumpJump.unity"},"Build/WebGL",BuildTarget.WebGL,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("WebGL build failed");
        Debug.Log("BUILD_AND_TESTS_PASSED");
    }
}
