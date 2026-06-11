using UnityEngine;

namespace BirdGame.Runtime
{
    public sealed class ScrollingSprite : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private float wrapAtX = -18f;
        [SerializeField] private float restartX = 18f;

        public void Configure(float moveSpeed, float wrapX, float resetX)
        {
            speed = moveSpeed;
            wrapAtX = wrapX;
            restartX = resetX;
        }

        private void Update()
        {
            transform.position += Vector3.left * (speed * Time.deltaTime);

            if (transform.position.x <= wrapAtX)
            {
                transform.position = new Vector3(restartX, transform.position.y, transform.position.z);
            }
        }
    }
}
