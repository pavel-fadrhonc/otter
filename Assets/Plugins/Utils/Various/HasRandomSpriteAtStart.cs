using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DefaultNamespace
{
    public class HasRandomSpriteAtStart : MonoBehaviour
    {
        public List<Sprite> spriteList;

        private SpriteRenderer _spriteRenderer;
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.sprite = spriteList[Random.Range(0, spriteList.Count)];
        }
    }
}