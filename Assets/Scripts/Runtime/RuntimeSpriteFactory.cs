using UnityEngine;

namespace BirdGame.Runtime
{
    public static class RuntimeSpriteFactory
    {
        private static Sprite cachedWhiteSquare;
        private static Sprite cachedWhiteCircle;

        public static Sprite WhiteSquare
        {
            get
            {
                if (cachedWhiteSquare == null)
                {
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    texture.filterMode = FilterMode.Point;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    cachedWhiteSquare = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        1f);
                }

                return cachedWhiteSquare;
            }
        }

        public static Sprite WhiteCircle
        {
            get
            {
                if (cachedWhiteCircle == null)
                {
                    const int size = 64;
                    var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                    var radius = size * 0.48f;

                    for (var y = 0; y < size; y++)
                    {
                        for (var x = 0; x < size; x++)
                        {
                            var distance = Vector2.Distance(new Vector2(x, y), center);
                            var alpha = Mathf.Clamp01(radius - distance);
                            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                        }
                    }

                    texture.Apply();
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    cachedWhiteCircle = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, size, size),
                        new Vector2(0.5f, 0.5f),
                        size);
                }

                return cachedWhiteCircle;
            }
        }
    }
}
