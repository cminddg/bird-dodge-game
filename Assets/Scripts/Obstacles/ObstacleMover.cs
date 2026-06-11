using UnityEngine;

namespace BirdGame.Obstacles
{
    public sealed class ObstacleMover : MonoBehaviour
    {
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float despawnX = -14f;
        private bool moving = true;

        public void Configure(float moveSpeed, float removeWhenPastX)
        {
            speed = moveSpeed;
            despawnX = removeWhenPastX;
        }

        public void SetMoving(bool enabled)
        {
            moving = enabled;
        }

        private void Update()
        {
            if (!moving)
            {
                return;
            }

            transform.position += Vector3.left * (speed * Time.deltaTime);

            if (transform.position.x <= despawnX)
            {
                Destroy(gameObject);
            }
        }
    }
}
