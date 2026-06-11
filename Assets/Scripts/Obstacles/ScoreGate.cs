using UnityEngine;

namespace BirdGame.Obstacles
{
    public sealed class ScoreGate : MonoBehaviour
    {
        [SerializeField] private int scoreAmount = 10;
        private bool consumed;

        public int ScoreAmount => scoreAmount;

        public void Configure(int newScoreAmount)
        {
            scoreAmount = Mathf.Max(0, newScoreAmount);
            consumed = false;
        }

        public bool TryConsume()
        {
            if (consumed)
            {
                return false;
            }

            consumed = true;
            return true;
        }
    }
}
