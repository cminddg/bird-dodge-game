using BirdGame.Core;
using BirdGame.Obstacles;
using UnityEngine;

namespace BirdGame.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class CollisionLifeSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BirdLifeView[] birdLives = new BirdLifeView[3];
        [SerializeField] private Vector2 targetVisualSize = new Vector2(1.05f, 0.82f);
        [SerializeField] private float hitInvulnerabilitySeconds = 0.75f;
        [SerializeField] private Color invulnerableTint = new Color(1f, 1f, 1f, 0.55f);

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D hitCollider;
        private int currentLifeIndex;
        private int currentFlapFrameIndex;
        private float invulnerableTimer;
        private Color baseTint;

        public int TotalLives => birdLives == null ? 0 : birdLives.Length;
        public int RemainingLives => Mathf.Max(0, TotalLives - currentLifeIndex);

        public void Initialize(GameManager manager)
        {
            gameManager = manager;
        }

        public void ResetLives()
        {
            currentLifeIndex = 0;
            currentFlapFrameIndex = 0;
            invulnerableTimer = 0f;
            ApplyCurrentLifeView();
        }

        public void AdvanceFlapFrame()
        {
            if (TotalLives == 0)
            {
                return;
            }

            var activeIndex = Mathf.Clamp(currentLifeIndex, 0, TotalLives - 1);
            var frames = birdLives[activeIndex].flapFrames;
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            currentFlapFrameIndex = (currentFlapFrameIndex + 1) % frames.Length;
            ApplySprite(frames[currentFlapFrameIndex]);
        }

        public bool ConsumeLife()
        {
            if (TotalLives == 0)
            {
                return false;
            }

            var nextIndex = currentLifeIndex + 1;
            if (nextIndex >= TotalLives)
            {
                currentLifeIndex = TotalLives;
                return false;
            }

            currentLifeIndex = nextIndex;
            currentFlapFrameIndex = 0;
            invulnerableTimer = hitInvulnerabilitySeconds;
            ApplyCurrentLifeView();
            return true;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitCollider = GetComponent<BoxCollider2D>();
            baseTint = spriteRenderer.color;

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void Update()
        {
            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= Time.deltaTime;
                spriteRenderer.color = invulnerableTint;
            }
            else if (spriteRenderer.color != baseTint)
            {
                spriteRenderer.color = baseTint;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (gameManager == null || gameManager.CurrentState != GameState.Running)
            {
                return;
            }

            var scoreGate = other.GetComponent<ScoreGate>() ?? other.GetComponentInParent<ScoreGate>();
            if (scoreGate != null)
            {
                if (scoreGate.TryConsume())
                {
                    gameManager.AddScore(scoreGate.ScoreAmount);
                }

                return;
            }

            if (invulnerableTimer > 0f)
            {
                return;
            }

            var obstacleDamage = other.GetComponent<ObstacleDamage>() ?? other.GetComponentInParent<ObstacleDamage>();
            if (obstacleDamage != null)
            {
                gameManager.LoseLife();
            }
        }

        private void ApplyCurrentLifeView()
        {
            if (TotalLives == 0)
            {
                return;
            }

            var activeIndex = Mathf.Clamp(currentLifeIndex, 0, TotalLives - 1);
            var view = birdLives[activeIndex];

            var sprite = view.sprite;
            if (view.flapFrames != null && view.flapFrames.Length > 0)
            {
                sprite = view.flapFrames[Mathf.Clamp(currentFlapFrameIndex, 0, view.flapFrames.Length - 1)];
            }

            ApplySprite(sprite);

            if (view.colliderSize != Vector2.zero)
            {
                var scale = Mathf.Max(0.001f, transform.localScale.x);
                hitCollider.size = view.colliderSize / scale;
            }

            spriteRenderer.color = baseTint;
        }

        private void ApplySprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            FitSpriteToWorldSize(sprite);
        }

        private void FitSpriteToWorldSize(Sprite sprite)
        {
            var bounds = sprite.bounds.size;
            if (bounds.x <= 0f || bounds.y <= 0f)
            {
                return;
            }

            var scale = Mathf.Min(targetVisualSize.x / bounds.x, targetVisualSize.y / bounds.y);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
