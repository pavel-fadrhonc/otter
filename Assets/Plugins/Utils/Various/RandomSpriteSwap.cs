using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.Animation
{
    public class RandomSpriteSwap : MonoBehaviour
    {
        public Vector2 randomSpan;

        public List<Sprite> sprites;
        
        private float _nextSpawnTime;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            GenerateNextSpawnTime();

            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad > _nextSpawnTime)
            {
                _spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
                
                GenerateNextSpawnTime();
            }
        }


        private void GenerateNextSpawnTime()
        {
            _nextSpawnTime = _nextSpawnTime + Random.Range(randomSpan.x, randomSpan.y);
        }        
    }
}