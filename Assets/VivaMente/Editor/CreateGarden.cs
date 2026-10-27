using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VivaMente.Editor
{
    public static class CreateGarden
    {
        [MenuItem("VivaMente/Criar cena Sala de Vinil")]
        public static void CreateVinyl()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var room = new GameObject("VivaMente — Sala de Vinil").AddComponent<VinylRoom>();
            System.IO.Directory.CreateDirectory("Assets/VivaMente/Scenes");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/VivaMente/Scenes/Vinil.unity");
            EditorSceneManager.SaveScene(scene, path);
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Selection.activeGameObject = room.gameObject;
        }
        [MenuItem("VivaMente/Criar cena Jardim da Memória")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("VivaMente — Jardim da Memória").AddComponent<MemoryGarden>();
            System.IO.Directory.CreateDirectory("Assets/VivaMente/Scenes");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/VivaMente/Scenes/Jardim.unity");
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            Selection.activeGameObject = Object.FindFirstObjectByType<MemoryGarden>().gameObject;
        }
    }
}
