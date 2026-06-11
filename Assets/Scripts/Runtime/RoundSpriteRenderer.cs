using UnityEngine;

namespace BirdGame.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RoundSpriteRenderer : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = RuntimeSpriteFactory.WhiteCircle;
        }
    }
}
