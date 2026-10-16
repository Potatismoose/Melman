using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Melman.Editor {
[InitializeOnLoad] public static class ProjectSetup {
 static ProjectSetup(){EditorApplication.delayCall+=EnsureScene;}
 static void EnsureScene(){if(!System.IO.File.Exists("Assets/Melman/Scenes/Melman.unity"))CreateScene();}
 [MenuItem("Melman/Create or open game scene")] public static void CreateScene(){
  const string path="Assets/Melman/Scenes/Melman.unity";
  if(System.IO.File.Exists(path)){EditorSceneManager.OpenScene(path);return;}
  System.IO.Directory.CreateDirectory("Assets/Melman/Scenes");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("Melman").AddComponent<MelmanApp>();
  EditorSceneManager.SaveScene(scene,path);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
  PlayerSettings.productName="Melman";PlayerSettings.companyName="Melman";
 }
 public static void BuildLinux(){CreateScene();var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Linux/Melman",BuildTarget.StandaloneLinux64,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Melman build failed");}
}
}
