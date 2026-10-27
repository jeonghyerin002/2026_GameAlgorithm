using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoLesson6.Editor
{
    [InitializeOnLoad]
    public static class AlgorithmPokerSceneBuilder
    {
        private const string ScenePath = "Assets/Lesson6/Scenes/Lesson06_AlgorithmPoker.unity";
        static AlgorithmPokerSceneBuilder() => EditorApplication.delayCall += () => { if (!File.Exists(ScenePath)) Build(false); };

        [MenuItem("AlgoLab/Lesson 6/Rebuild Algorithm Poker Scene")]
        public static void Rebuild() => Build(true);

        private static void Build(bool openAfter)
        {
            Directory.CreateDirectory("Assets/Lesson6/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateRoot("01_ENVIRONMENT_CardTable");
            CreateRoot("02_GAMEPLAY_Cards_Runtime");
            CreateRoot("03_INTERACTION_CardSelection");
            GameObject cameraObject = CreateRoot("04_CAMERA");
            Camera camera = cameraObject.AddComponent<Camera>(); cameraObject.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f, .055f, .055f); camera.orthographic = true;
            CreateRoot("05_AUDIO");
            CreateRoot("06_UI_AlgorithmPoker");
            GameObject manager = CreateRoot("07_GAME_MANAGER"); manager.AddComponent<AlgorithmPokerGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            if (openAfter) EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Lesson 6 scene created: " + ScenePath);
        }

        private static GameObject CreateRoot(string name) { return new GameObject(name); }
    }
}
