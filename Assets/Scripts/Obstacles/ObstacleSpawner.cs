using System.Collections.Generic;
using BirdGame.Core;
using BirdGame.Runtime;
using UnityEngine;

namespace BirdGame.Obstacles
{
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;

        [Header("Spawn Timing")]
        [SerializeField] private float baseSpawnInterval = 1.8f;
        [SerializeField] private float minSpawnInterval = 1.05f;
        [SerializeField] private float spawnIntervalStepPerLevel = 0.10f;

        [Header("Obstacle Movement")]
        [SerializeField] private float baseMoveSpeed = 3.1f;
        [SerializeField] private float moveSpeedStepPerLevel = 0.38f;

        [Header("Spawn Space")]
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float despawnX = -14f;
        [SerializeField] private float gapSize = 2.85f;
        [SerializeField] private float minGapCenterY = -1.35f;
        [SerializeField] private float maxGapCenterY = 2.1f;

        [Header("Obstacle Shape")]
        [SerializeField] private float obstacleWidth = 1.05f;
        [SerializeField] private float obstacleHeight = 8f;
        [SerializeField] private Color obstacleColor = new Color(0.16f, 0.58f, 0.34f, 1f);
        [SerializeField] private Color gateLipColor = new Color(0.92f, 0.82f, 0.30f, 1f);

        [Header("Scoring")]
        [SerializeField] private int scorePerPass = 10;

        private float spawnTimer;
        private int spawnCount;
        private bool spawning;
        private readonly List<ObstacleMover> liveObstacles = new List<ObstacleMover>();

        public void Initialize(GameManager manager)
        {
            gameManager = manager;
        }

        public void ResetSpawner()
        {
            spawning = false;
            spawnTimer = 0f;
            spawnCount = 0;
            ClearLiveObstacles();
        }

        public void StartSpawning()
        {
            ClearLiveObstacles();
            spawning = true;
            spawnCount = 0;
            spawnTimer = 0.5f;
        }

        public void StopSpawning()
        {
            spawning = false;
        }

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void Update()
        {
            if (!spawning || gameManager == null || gameManager.CurrentState != GameState.Running)
            {
                return;
            }

            liveObstacles.RemoveAll(obstacle => obstacle == null);

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnObstaclePair();
                spawnTimer = GetCurrentSpawnInterval();
            }
        }

        private float GetCurrentSpawnInterval()
        {
            var interval = baseSpawnInterval - (gameManager.DifficultyLevel * spawnIntervalStepPerLevel);
            return Mathf.Max(minSpawnInterval, interval);
        }

        private float GetCurrentMoveSpeed()
        {
            return baseMoveSpeed + (gameManager.DifficultyLevel * moveSpeedStepPerLevel);
        }

        private void SpawnObstaclePair()
        {
            var pairRoot = new GameObject("ObstaclePair");
            pairRoot.transform.position = new Vector3(spawnX, 0f, 0f);
            spawnCount++;

            var mover = pairRoot.AddComponent<ObstacleMover>();
            mover.Configure(GetCurrentMoveSpeed(), despawnX);
            liveObstacles.Add(mover);

            var gapCenter = Random.Range(minGapCenterY, maxGapCenterY);
            var upperCenterY = gapCenter + (gapSize * 0.5f) + (obstacleHeight * 0.5f);
            var lowerCenterY = gapCenter - (gapSize * 0.5f) - (obstacleHeight * 0.5f);

            var currentColor = GetCurrentObstacleColor();
            CreateObstacleBlock(pairRoot.transform, upperCenterY, currentColor);
            CreateObstacleBlock(pairRoot.transform, lowerCenterY, currentColor);
            CreateGateLip(pairRoot.transform, gapCenter + (gapSize * 0.5f));
            CreateGateLip(pairRoot.transform, gapCenter - (gapSize * 0.5f));
            CreateCheckpointFlag(pairRoot.transform, gapCenter);
            CreateScoreGate(pairRoot.transform, gapCenter);
        }

        private Color GetCurrentObstacleColor()
        {
            var t = Mathf.PingPong(gameManager == null ? 0f : gameManager.DifficultyLevel * 0.22f, 1f);
            var stageTint = new Color(0.18f, 0.42f, 0.68f, 1f);
            return Color.Lerp(obstacleColor, stageTint, t);
        }

        private void CreateObstacleBlock(Transform parent, float localY, Color color)
        {
            var obstacle = new GameObject("Obstacle");
            obstacle.transform.SetParent(parent, false);
            obstacle.transform.localPosition = new Vector3(0f, localY, 0f);
            obstacle.transform.localScale = new Vector3(obstacleWidth, obstacleHeight, 1f);

            var renderer = obstacle.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            renderer.color = color;
            renderer.sortingOrder = 2;

            var collider = obstacle.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            obstacle.AddComponent<ObstacleDamage>();
        }

        private void CreateGateLip(Transform parent, float localY)
        {
            var lip = new GameObject("GateLip");
            lip.transform.SetParent(parent, false);
            lip.transform.localPosition = new Vector3(0f, localY, 0f);
            lip.transform.localScale = new Vector3(obstacleWidth + 0.38f, 0.22f, 1f);

            var renderer = lip.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            renderer.color = gateLipColor;
            renderer.sortingOrder = 3;
        }

        private void CreateCheckpointFlag(Transform parent, float localY)
        {
            if (spawnCount % 5 != 0)
            {
                return;
            }

            var flag = new GameObject("CheckpointFlag");
            flag.transform.SetParent(parent, false);
            flag.transform.localPosition = new Vector3(0f, localY + 1.7f, 0f);
            flag.transform.localScale = new Vector3(1.35f, 0.26f, 1f);

            var renderer = flag.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            renderer.color = new Color(1f, 0.42f, 0.18f, 1f);
            renderer.sortingOrder = 4;
        }

        private void CreateScoreGate(Transform parent, float localY)
        {
            var gate = new GameObject("ScoreGate");
            gate.transform.SetParent(parent, false);
            gate.transform.localPosition = new Vector3(0f, localY, 0f);
            gate.transform.localScale = new Vector3(0.5f, gapSize, 1f);

            var collider = gate.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var scoreGate = gate.AddComponent<ScoreGate>();
            scoreGate.Configure(scorePerPass);
        }

        private void ClearLiveObstacles()
        {
            for (var i = 0; i < liveObstacles.Count; i++)
            {
                var obstacle = liveObstacles[i];
                if (obstacle != null)
                {
                    Destroy(obstacle.gameObject);
                }
            }

            liveObstacles.Clear();
        }
    }
}
