using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BirdGame.Bootstrap
{
    public sealed class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "Game";

        private IEnumerator Start()
        {
            if (SceneManager.GetActiveScene().name == targetSceneName)
            {
                yield break;
            }

            yield return SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        }
    }
}
