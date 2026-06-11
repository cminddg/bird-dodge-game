using BirdGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BirdGame.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text hintText;
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject restartButton;

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score {score}";
            }
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Birds {lives}/3";
            }
        }

        public void UpdateBestScore(int bestScore)
        {
            if (bestScoreText != null)
            {
                bestScoreText.text = $"Best: {bestScore}";
            }
        }

        public void SetState(GameState state)
        {
            switch (state)
            {
                case GameState.Ready:
                    if (stateText != null)
                    {
                        stateText.text = "Press Space or Click to Start";
                    }

                    if (hintText != null)
                    {
                        hintText.text = "Space / Click";
                    }

                    SetGameOverVisible(false);
                    SetStartVisible(true);
                    SetRestartVisible(false);
                    break;
                case GameState.Running:
                    if (stateText != null)
                    {
                        stateText.text = string.Empty;
                    }

                    if (hintText != null)
                    {
                        hintText.text = string.Empty;
                    }

                    SetGameOverVisible(false);
                    SetStartVisible(false);
                    SetRestartVisible(false);
                    break;
                case GameState.GameOver:
                    if (stateText != null)
                    {
                        stateText.text = "Game Over";
                    }

                    if (hintText != null)
                    {
                        hintText.text = "R Restart";
                    }

                    SetGameOverVisible(true);
                    SetStartVisible(false);
                    SetRestartVisible(true);
                    break;
            }
        }

        public void ShowGameOver(int finalScore, int bestScore)
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
                gameOverText.text = $"Final Score: {finalScore}";
            }

            UpdateBestScore(bestScore);
        }

        private void SetGameOverVisible(bool visible)
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(visible);
            }
        }

        private void SetRestartVisible(bool visible)
        {
            if (restartButton != null)
            {
                restartButton.SetActive(visible);
            }
        }

        private void SetStartVisible(bool visible)
        {
            if (startButton != null)
            {
                startButton.SetActive(visible);
            }
        }
    }
}
