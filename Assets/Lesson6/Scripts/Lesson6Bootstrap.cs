using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlgoLesson6
{
    public static class Lesson6Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGame()
        {
            if (SceneManager.GetActiveScene().name != "Lesson06_AlgorithmPoker") return;
            if (Object.FindFirstObjectByType<AlgorithmPokerGame>() != null) return;
            GameObject manager = new GameObject("07_GAME_MANAGER_AlgorithmPoker");
            manager.AddComponent<AlgorithmPokerGame>();
        }
    }
}
