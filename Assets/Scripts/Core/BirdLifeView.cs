using System;
using UnityEngine;

namespace BirdGame.Core
{
    [Serializable]
    public struct BirdLifeView
    {
        public Sprite sprite;
        public Sprite[] flapFrames;
        public Vector2 colliderSize;
    }
}
