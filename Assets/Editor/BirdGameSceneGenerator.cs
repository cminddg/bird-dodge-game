using System.Collections.Generic;
using System.IO;
using System.Linq;
using BirdGame.Bootstrap;
using BirdGame.Core;
using BirdGame.Obstacles;
using BirdGame.Player;
using BirdGame.Runtime;
using BirdGame.Scoring;
using BirdGame.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BirdGame.Editor
{
    public static class BirdGameSceneGenerator
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string BirdSpriteDirectory = "Assets/Sprites/Birds";
        private static readonly string[] BirdFrameDirectories =
        {
            "Assets/Sprites/Birds/Bird1Frames",
            "Assets/Sprites/Birds/Bird2Frames",
            "Assets/Sprites/Birds/Bird3Frames"
        };

        private static readonly string[] BirdSourceDirectories =
        {
            "第一隻鳥",
            "第二隻鳥",
            "第三隻鳥"
        };
        private const string UiFontPath = "Assets/Fonts/Poppins-Regular.ttf";
        private const string UiTextMaterialPath = "Assets/Materials/BirdGameText.mat";
        private const string BackgroundMusicPath = "Assets/Audio/OwiesUkulele.mp3";
        private const string FlapChirpPath = "Assets/Audio/LightButton.mp3";

        [MenuItem("Tools/Bird Game/Generate MVP Scenes")]
        public static void GenerateMvpScenes()
        {
            EnsureFolders();
            var birdFrames = ImportBirdFrames();
            EnsureUiTextMaterial();

            CreateBootstrapScene();
            CreateGameScene(birdFrames);
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (Application.isBatchMode)
            {
                Debug.Log("Bird Game setup complete: Bootstrap and Game scenes generated.");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Bird Game Setup Complete",
                    "Bootstrap and Game scenes were generated. Open Build Settings and make sure Bootstrap is the first scene.",
                    "OK");
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(BirdSpriteDirectory);
            foreach (var frameDirectory in BirdFrameDirectories)
            {
                Directory.CreateDirectory(frameDirectory);
            }
            Directory.CreateDirectory("Assets/Materials");
        }

        private static List<List<Sprite>> ImportBirdFrames()
        {
            var birdFrames = new List<List<Sprite>>();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            for (var birdIndex = 0; birdIndex < BirdSourceDirectories.Length; birdIndex++)
            {
                var sprites = new List<Sprite>();
                var sourceDirectory = Path.Combine(projectRoot, BirdSourceDirectories[birdIndex]);
                var sourcePngFiles = Directory.Exists(sourceDirectory)
                    ? Directory.GetFiles(sourceDirectory, "*.png").OrderBy(ExtractImageNumber).ThenBy(path => path).ToArray()
                    : new string[0];

                for (var frameIndex = 0; frameIndex < sourcePngFiles.Length; frameIndex++)
                {
                    var targetPath = $"{BirdFrameDirectories[birdIndex]}/Frame{frameIndex + 1}.png";
                    var targetAbsolutePath = Path.Combine(projectRoot, targetPath);
                    File.Copy(sourcePngFiles[frameIndex], targetAbsolutePath, true);
                    AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

                    var textureImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                    if (textureImporter != null)
                    {
                        textureImporter.textureType = TextureImporterType.Sprite;
                        textureImporter.spriteImportMode = SpriteImportMode.Single;
                        textureImporter.spritePixelsPerUnit = 512f;
                        textureImporter.alphaIsTransparency = true;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.filterMode = FilterMode.Bilinear;
                        textureImporter.SaveAndReimport();
                    }

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(targetPath);
                    if (sprite != null)
                    {
                        sprites.Add(sprite);
                    }
                }

                if (sprites.Count == 0)
                {
                    Debug.LogWarning($"No PNG files found for bird {birdIndex + 1}: {sourceDirectory}");
                }

                birdFrames.Add(sprites);
            }

            return birdFrames;
        }

        private static int ExtractImageNumber(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var digits = new string(fileName.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var number))
            {
                return number;
            }

            return int.MaxValue;
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("BootstrapLoader");
            var loader = bootstrap.AddComponent<BootstrapLoader>();

            var loaderSerialized = new SerializedObject(loader);
            loaderSerialized.FindProperty("targetSceneName").stringValue = "Game";
            loaderSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void CreateGameScene(IReadOnlyList<List<Sprite>> birdFrames)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.55f, 0.78f, 0.92f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            CreateScenery();

            var gameRoot = new GameObject("GameRoot");
            var gameManager = gameRoot.AddComponent<GameManager>();
            var scoreSystem = gameRoot.AddComponent<ScoreSystem>();
            var obstacleSpawner = gameRoot.AddComponent<ObstacleSpawner>();
            var audioController = gameRoot.AddComponent<AudioController>();
            gameRoot.AddComponent<QuitHotkey>();
            ConfigureAudio(audioController);

            var player = new GameObject("Player");
            player.transform.position = new Vector3(-3f, 0f, 0f);
            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 3;
            if (birdFrames.Count > 0 && birdFrames[0].Count > 0)
            {
                spriteRenderer.sprite = birdFrames[0][0];
            }

            var rigidBody = player.AddComponent<Rigidbody2D>();
            rigidBody.gravityScale = 2.4f;
            rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidBody.freezeRotation = true;

            var hitCollider = player.AddComponent<BoxCollider2D>();
            hitCollider.size = new Vector2(0.7f, 0.55f);

            var playerController = player.AddComponent<PlayerController>();
            var collisionLifeSystem = player.AddComponent<CollisionLifeSystem>();

            ConfigureBirdLifeViews(collisionLifeSystem, birdFrames);
            ConfigureHud(gameManager, out _, out var hudController);
            EnsureEventSystem();

            SetReference(gameManager, "playerController", playerController);
            SetReference(gameManager, "collisionLifeSystem", collisionLifeSystem);
            SetReference(gameManager, "obstacleSpawner", obstacleSpawner);
            SetReference(gameManager, "scoreSystem", scoreSystem);
            SetReference(gameManager, "hudController", hudController);

            SetReference(playerController, "gameManager", gameManager);
            SetReference(playerController, "audioController", audioController);
            SetReference(playerController, "collisionLifeSystem", collisionLifeSystem);
            SetReference(collisionLifeSystem, "gameManager", gameManager);
            SetReference(obstacleSpawner, "gameManager", gameManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        private static void ConfigureBirdLifeViews(
            CollisionLifeSystem collisionLifeSystem,
            IReadOnlyList<List<Sprite>> birdFrames)
        {
            var serialized = new SerializedObject(collisionLifeSystem);
            var lifeArray = serialized.FindProperty("birdLives");
            lifeArray.arraySize = 3;

            for (var i = 0; i < 3; i++)
            {
                var slot = lifeArray.GetArrayElementAtIndex(i);
                var spriteProperty = slot.FindPropertyRelative("sprite");
                var framesProperty = slot.FindPropertyRelative("flapFrames");
                var sizeProperty = slot.FindPropertyRelative("colliderSize");

                var frames = i < birdFrames.Count ? birdFrames[i] : new List<Sprite>();
                var sprite = frames.Count > 0 ? frames[0] : null;
                spriteProperty.objectReferenceValue = sprite;
                framesProperty.arraySize = frames.Count;
                for (var frameIndex = 0; frameIndex < framesProperty.arraySize; frameIndex++)
                {
                    framesProperty.GetArrayElementAtIndex(frameIndex).objectReferenceValue = frames[frameIndex];
                }

                sizeProperty.vector2Value = new Vector2(0.72f, 0.52f);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateScenery()
        {
            CreateBlock("DistantHillsA", new Vector3(-6f, -3.5f, 4f), new Vector3(14f, 2.2f, 1f), new Color(0.35f, 0.63f, 0.55f, 1f), -4);
            CreateBlock("DistantHillsB", new Vector3(7f, -3.25f, 4f), new Vector3(12f, 1.8f, 1f), new Color(0.29f, 0.55f, 0.49f, 1f), -4);

            CreateCloud("CloudA", new Vector3(-2f, 2.9f, 3f), 0.35f, new Color(1f, 1f, 1f, 0.82f));
            CreateCloud("CloudB", new Vector3(5.5f, 1.8f, 3f), 0.28f, new Color(0.92f, 0.97f, 1f, 0.78f));
            CreateCloud("CloudC", new Vector3(12f, 3.45f, 3f), 0.42f, new Color(1f, 1f, 1f, 0.72f));

            CreateScrollingBlock("GroundA", new Vector3(-8f, -4.55f, 1f), new Vector3(16f, 0.9f, 1f), new Color(0.24f, 0.48f, 0.24f, 1f), 4, 2.1f);
            CreateScrollingBlock("GroundB", new Vector3(8f, -4.55f, 1f), new Vector3(16f, 0.9f, 1f), new Color(0.24f, 0.48f, 0.24f, 1f), 4, 2.1f);
            CreateScrollingBlock("GroundStripeA", new Vector3(-8f, -4.08f, 0.8f), new Vector3(16f, 0.12f, 1f), new Color(0.75f, 0.89f, 0.42f, 1f), 5, 2.1f);
            CreateScrollingBlock("GroundStripeB", new Vector3(8f, -4.08f, 0.8f), new Vector3(16f, 0.12f, 1f), new Color(0.75f, 0.89f, 0.42f, 1f), 5, 2.1f);
        }

        private static void CreateScrollingBlock(string name, Vector3 position, Vector3 scale, Color color, int sortingOrder, float speed)
        {
            var block = CreateBlock(name, position, scale, color, sortingOrder);
            block.AddComponent<ScrollingSprite>().Configure(speed, -16f, 16f);
        }

        private static void CreateCloud(string name, Vector3 position, float speed, Color color)
        {
            var cloud = new GameObject(name);
            cloud.transform.position = position;
            cloud.AddComponent<ScrollingSprite>().Configure(speed, -16f, 16f);

            CreateCloudPuff(cloud.transform, "PuffLeft", new Vector3(-0.58f, -0.05f, 0f), new Vector3(0.95f, 0.62f, 1f), color);
            CreateCloudPuff(cloud.transform, "PuffTop", new Vector3(-0.08f, 0.17f, 0f), new Vector3(1.15f, 0.82f, 1f), color);
            CreateCloudPuff(cloud.transform, "PuffRight", new Vector3(0.56f, -0.03f, 0f), new Vector3(1.05f, 0.66f, 1f), color);
            CreateCloudPuff(cloud.transform, "PuffBase", new Vector3(0f, -0.22f, 0f), new Vector3(1.65f, 0.5f, 1f), color);
        }

        private static void CreateCloudPuff(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            var puff = new GameObject(name);
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;
            puff.transform.localScale = scale;

            var renderer = puff.AddComponent<SpriteRenderer>();
            renderer.color = color;
            renderer.sortingOrder = -3;
            puff.AddComponent<RoundSpriteRenderer>();
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color, int sortingOrder)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            block.transform.localScale = scale;

            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            block.AddComponent<SolidSpriteRenderer>();

            return block;
        }

        private static void ConfigureAudio(AudioController audioController)
        {
            var serialized = new SerializedObject(audioController);
            serialized.FindProperty("backgroundMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(BackgroundMusicPath);
            serialized.FindProperty("flapChirp").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(FlapChirpPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureHud(GameManager gameManager, out GameObject canvasObject, out HudController hudController)
        {
            canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvasObject.AddComponent<GraphicRaycaster>();

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var livesText = CreateText(
                "LivesText",
                canvasObject.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -24f),
                30,
                TextAnchor.UpperLeft);

            var scoreText = CreateText(
                "ScoreText",
                canvasObject.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-24f, -24f),
                30,
                TextAnchor.UpperRight);

            var stateText = CreateText(
                "StateText",
                canvasObject.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -24f),
                28,
                TextAnchor.UpperCenter);

            var gameOverText = CreateText(
                "GameOverText",
                canvasObject.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 18f),
                44,
                TextAnchor.MiddleCenter);

            var bestText = CreateText(
                "BestScoreText",
                canvasObject.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-24f, -66f),
                22,
                TextAnchor.UpperRight);

            var hintText = CreateText(
                "HintText",
                canvasObject.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 24f),
                24,
                TextAnchor.LowerCenter);

            var startButton = CreateActionButton(
                "StartButton",
                "Start",
                canvasObject.transform,
                new Vector2(0f, -54f),
                gameManager.StartFromUi);
            var restartButton = CreateActionButton(
                "RestartButton",
                "Restart",
                canvasObject.transform,
                new Vector2(0f, -98f),
                gameManager.RestartFromUi);

            hudController = canvasObject.AddComponent<HudController>();
            SetReference(hudController, "scoreText", scoreText);
            SetReference(hudController, "livesText", livesText);
            SetReference(hudController, "stateText", stateText);
            SetReference(hudController, "gameOverText", gameOverText);
            SetReference(hudController, "bestScoreText", bestText);
            SetReference(hudController, "hintText", hintText);
            SetReference(hudController, "startButton", startButton);
            SetReference(hudController, "restartButton", restartButton);
        }

        private static GameObject CreateActionButton(
            string name,
            string labelText,
            Transform parent,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(220f, 58f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.58f, 0.34f, 1f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddPersistentListener(button.onClick, action);

            var label = CreateText(
                $"{name}Label",
                buttonObject.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                26,
                TextAnchor.MiddleCenter);
            label.rectTransform.sizeDelta = Vector2.zero;
            label.text = labelText;

            buttonObject.SetActive(false);
            return buttonObject;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            int fontSize,
            TextAnchor anchor)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(700f, 90f);

            var text = textObject.AddComponent<Text>();
            text.font = AssetDatabase.LoadAssetAtPath<Font>(UiFontPath);
            text.material = AssetDatabase.LoadAssetAtPath<Material>(UiTextMaterialPath);
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = string.Empty;

            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return text;
        }

        private static void EnsureUiTextMaterial()
        {
            var shader = Shader.Find("BirdGame/UI/Text");
            if (shader == null)
            {
                throw new System.InvalidOperationException("BirdGame/UI/Text shader could not be found.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(UiTextMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, UiTextMaterialPath);
            }
            else
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
        }

        private static void AddScenesToBuildSettings()
        {
            var existing = EditorBuildSettings.scenes.ToList();
            AddSceneIfMissing(existing, BootstrapScenePath);
            AddSceneIfMissing(existing, GameScenePath);
            EditorBuildSettings.scenes = existing.ToArray();
        }

        private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            var alreadyAdded = scenes.Any(scene => scene.path == scenePath);
            if (!alreadyAdded)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        private static void SetReference(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning($"Could not find serialized field '{fieldName}' on {target.name}.");
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
