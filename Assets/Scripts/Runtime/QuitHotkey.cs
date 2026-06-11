using System.Collections;
using UnityEngine;

namespace BirdGame.Runtime
{
    public sealed class QuitHotkey : MonoBehaviour
    {
        private bool quitting;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitNow();
            }
        }

        private void OnGUI()
        {
            var current = Event.current;
            if (current != null && current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                QuitNow();
            }
        }

        private void QuitNow()
        {
            if (quitting)
            {
                return;
            }

            quitting = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(0);
            StartCoroutine(ForceQuitIfNeeded());
#endif
        }

#if !UNITY_EDITOR
        private IEnumerator ForceQuitIfNeeded()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
#endif
    }
}
