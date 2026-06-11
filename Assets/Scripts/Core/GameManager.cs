using BirdGame.Obstacles;
using BirdGame.Player;
using BirdGame.Scoring;
using BirdGame.UI;
using UnityEngine;

namespace BirdGame.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const string BestScoreKey = "BirdGameBestScore";

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CollisionLifeSystem collisionLifeSystem;
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private HudController hudController;

        [Header("Difficulty")]
        [SerializeField] private float difficultyStepSeconds = 15f;

        public GameState CurrentState { get; private set; }
        public int DifficultyLevel { get; private set; }
        public float ElapsedRunTime { get; private set; }
        public int BestScore { get; private set; }

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            if (collisionLifeSystem == null)
            {
                collisionLifeSystem = FindObjectOfType<CollisionLifeSystem>();
            }

            if (obstacleSpawner == null)
            {
                obstacleSpawner = FindObjectOfType<ObstacleSpawner>();
            }

            if (scoreSystem == null)
            {
                scoreSystem = FindObjectOfType<ScoreSystem>();
            }

            if (hudController == null)
            {
                hudController = FindObjectOfType<HudController>();
            }

            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            if (playerController != null)
            {
                playerController.Initialize(this);
            }

            if (collisionLifeSystem != null)
            {
                collisionLifeSystem.Initialize(this);
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.Initialize(this);
            }

            EnterReadyState();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartRun();
                return;
            }

            if (CurrentState != GameState.Running)
            {
                return;
            }

            ElapsedRunTime += Time.deltaTime;
            DifficultyLevel = Mathf.FloorToInt(ElapsedRunTime / difficultyStepSeconds);
            RefreshHud();
        }

        public void StartRun()
        {
            if (CurrentState != GameState.Ready)
            {
                return;
            }

            ElapsedRunTime = 0f;
            DifficultyLevel = 0;

            if (scoreSystem != null)
            {
                scoreSystem.ResetRun();
                scoreSystem.StartRun();
            }

            if (collisionLifeSystem != null)
            {
                collisionLifeSystem.ResetLives();
            }

            if (playerController != null)
            {
                playerController.BeginRun();
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.StartSpawning();
            }

            CurrentState = GameState.Running;

            if (hudController != null)
            {
                hudController.SetState(CurrentState);
            }

            RefreshHud();
        }

        public void LoseLife()
        {
            if (CurrentState != GameState.Running || collisionLifeSystem == null)
            {
                return;
            }

            var stillAlive = collisionLifeSystem.ConsumeLife();

            if (playerController != null)
            {
                playerController.RecoverAfterHit();
            }

            RefreshHud();

            if (!stillAlive)
            {
                EndRun();
            }
        }

        public void AddScore(int amount)
        {
            if (CurrentState != GameState.Running || scoreSystem == null)
            {
                return;
            }

            scoreSystem.AddObstacleScore(amount);
            RefreshHud();
        }

        public void EndRun()
        {
            if (CurrentState != GameState.Running)
            {
                return;
            }

            CurrentState = GameState.GameOver;

            if (obstacleSpawner != null)
            {
                obstacleSpawner.StopSpawning();
            }

            if (playerController != null)
            {
                playerController.DisableForGameOver();
            }

            if (scoreSystem != null)
            {
                scoreSystem.StopRun();
            }

            var finalScore = scoreSystem == null ? 0 : scoreSystem.CurrentScore;
            if (finalScore > BestScore)
            {
                BestScore = finalScore;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
                PlayerPrefs.Save();
            }

            if (hudController != null)
            {
                hudController.SetState(CurrentState);
                hudController.ShowGameOver(finalScore, BestScore);
            }

            RefreshHud();
        }

        private void RestartRun()
        {
            EnterReadyState();
            StartRun();
        }

        public void RestartFromUi()
        {
            RestartRun();
        }

        public void StartFromUi()
        {
            StartRun();
        }

        private void EnterReadyState()
        {
            CurrentState = GameState.Ready;
            ElapsedRunTime = 0f;
            DifficultyLevel = 0;

            if (scoreSystem != null)
            {
                scoreSystem.ResetRun();
            }

            if (collisionLifeSystem != null)
            {
                collisionLifeSystem.ResetLives();
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ResetSpawner();
            }

            if (playerController != null)
            {
                playerController.PrepareForReady();
            }

            if (hudController != null)
            {
                hudController.SetState(CurrentState);
            }

            RefreshHud();
        }

        private void RefreshHud()
        {
            if (hudController == null)
            {
                return;
            }

            hudController.UpdateScore(scoreSystem == null ? 0 : scoreSystem.CurrentScore);
            hudController.UpdateLives(collisionLifeSystem == null ? 0 : collisionLifeSystem.RemainingLives);
            hudController.UpdateBestScore(BestScore);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
