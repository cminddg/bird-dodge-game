using UnityEngine;

namespace BirdGame.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SolidSpriteRenderer : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = RuntimeSpriteFactory.WhiteSquare;
        }
    }
}
