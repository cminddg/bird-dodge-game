using BirdGame.Core;
using BirdGame.Runtime;
using UnityEngine;

namespace BirdGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AudioController audioController;
        [SerializeField] private CollisionLifeSystem collisionLifeSystem;
        [SerializeField] private float flapVelocity = 5.75f;
        [SerializeField] private float maxFallVelocity = -11f;
        [SerializeField] private float recoverClampY = 3.8f;

        private Rigidbody2D body;
        private Vector3 spawnPosition;
        private bool controlsEnabled = true;

        public void Initialize(GameManager manager)
        {
            gameManager = manager;
        }

        public void PrepareForReady()
        {
            controlsEnabled = true;
            transform.position = spawnPosition;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        public void BeginRun()
        {
            controlsEnabled = true;
            transform.position = spawnPosition;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = true;
        }

        public void DisableForGameOver()
        {
            controlsEnabled = false;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        public void RecoverAfterHit()
        {
            transform.position = new Vector3(spawnPosition.x, Mathf.Clamp(transform.position.y, -recoverClampY, recoverClampY), transform.position.z);
            body.velocity = Vector2.zero;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spawnPosition = transform.position;

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (audioController == null)
            {
                audioController = FindObjectOfType<AudioController>();
            }

            if (collisionLifeSystem == null)
            {
                collisionLifeSystem = FindObjectOfType<CollisionLifeSystem>();
            }
        }

        private void Start()
        {
            body.simulated = false;
        }

        private void Update()
        {
            if (gameManager == null || !IsFlapInput())
            {
                return;
            }

            if (gameManager.CurrentState == GameState.Ready)
            {
                gameManager.StartRun();
                Flap();
            }
            else if (gameManager.CurrentState == GameState.Running && controlsEnabled)
            {
                Flap();
            }
        }

        private void FixedUpdate()
        {
            if (!body.simulated)
            {
                return;
            }

            if (body.velocity.y < maxFallVelocity)
            {
                body.velocity = new Vector2(body.velocity.x, maxFallVelocity);
            }
        }

        private bool IsFlapInput()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        }

        private void Flap()
        {
            body.velocity = new Vector2(body.velocity.x, flapVelocity);

            if (collisionLifeSystem != null)
            {
                collisionLifeSystem.AdvanceFlapFrame();
            }

            if (audioController != null)
            {
                audioController.PlayFlapChirp();
            }
        }
    }
}
