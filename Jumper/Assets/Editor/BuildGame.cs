using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.TextCore.LowLevel;
public static class BuildGame {
    public static void BuildWebGL(){Build(BuildTarget.WebGL,"Build/WebGL");}
    public static void BuildDesktop(){
        string architecture=System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()=="Arm64"?"ARM64":"x64";
        EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture",architecture);
        Build(BuildTarget.StandaloneOSX,"Build/Jump Jump.app");
    }
    public static void BuildMacOS(){
        string previous=EditorUserBuildSettings.GetPlatformSettings("OSXUniversal","Architecture");
        try {EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture","x64ARM64");Build(BuildTarget.StandaloneOSX,"Build/macOS/Jump Jump.app");}
        finally {EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture",previous);}
    }
    public static void BuildWindows(){Build(BuildTarget.StandaloneWindows64,"Build/Windows/Jump Jump.exe");}
    public static void BuildLinux(){Build(BuildTarget.StandaloneLinux64,"Build/Linux/Jump Jump.x86_64");}
    static void PrepareFont(string name){
        string output="Assets/Resources/Fonts/"+name+"SDF.asset";
        if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(output))return;
        var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/"+name+".ttf");
        var asset=TMP_FontAsset.CreateFontAsset(source,64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
        asset.name=name+"SDF";
        string chars="";for(int i=32;i<127;i++)chars+=(char)i;chars+="•—";
        if(!asset.TryAddCharacters(chars,out var missing))throw new Exception("Font lacks glyphs: "+missing);
        asset.atlasPopulationMode=AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(asset,output);
        foreach(var texture in asset.atlasTextures)AssetDatabase.AddObjectToAsset(texture,asset);
        AssetDatabase.AddObjectToAsset(asset.material,asset);
        EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();
        Debug.Log("FONT_ATLAS_READY: "+name);
    }
    static void Build(BuildTarget target,string path){
        JumpTests.Run();
        if(!Resources.Load<TMP_Settings>("TMP Settings"))throw new Exception("Missing committed TextMeshPro essential resources.");
        PrepareFont("Display");PrepareFont("Body");
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue=1;settings.ApplyModifiedPropertiesWithoutUndo();
        if(!AssetDatabase.IsValidFolder("Assets/Resources"))AssetDatabase.CreateFolder("Assets","Resources");
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Resources/Renderer.asset");
        if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Resources/Renderer.asset");}
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Resources/Pipeline.asset");
        if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Resources/Pipeline.asset");}
        pipeline.supportsHDR=false;pipeline.msaaSampleCount=2;pipeline.shadowDistance=45;var pipelineSettings=new SerializedObject(pipeline);pipelineSettings.FindProperty("m_MainLightShadowsSupported").boolValue=true;pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
        GraphicsSettings.defaultRenderPipeline=pipeline;
        for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
        if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Surface.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/Resources/Surface.mat");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Jump Jump").AddComponent<JumpGame>();EditorSceneManager.SaveScene(scene,"Assets/JumpJump.unity");
        PlayerSettings.productName="Jump Jump";PlayerSettings.companyName="Pocket Arcade";
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
        PlayerSettings.defaultScreenWidth=1100;PlayerSettings.defaultScreenHeight=750;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.template="PROJECT:Itch";
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
        if(target!=BuildTarget.WebGL)
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        Debug.Log($"Building {target}, version {PlayerSettings.bundleVersion}, output {path}");
        AssetDatabase.SaveAssets();
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/JumpJump.unity"},path,target,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception(target+" build failed");
        Debug.Log("BUILD_AND_TESTS_PASSED");
    }
}
