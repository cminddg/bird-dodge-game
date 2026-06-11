using UnityEngine;

namespace BirdGame.Scoring
{
    public sealed class ScoreSystem : MonoBehaviour
    {
        [SerializeField] private float survivalPointsPerSecond = 0f;

        private float survivalScore;
        private int obstacleScore;
        private bool running;

        public int CurrentScore => Mathf.FloorToInt(survivalScore) + obstacleScore;

        public void ResetRun()
        {
            survivalScore = 0f;
            obstacleScore = 0;
            running = false;
        }

        public void StartRun()
        {
            running = true;
        }

        public void StopRun()
        {
            running = false;
        }

        public void AddObstacleScore(int amount)
        {
            obstacleScore += Mathf.Max(0, amount);
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            survivalScore += Time.deltaTime * survivalPointsPerSecond;
        }
    }
}
