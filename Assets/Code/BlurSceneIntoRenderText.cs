using System;
using UnityEngine;

namespace DefaultNamespace
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class BlurSceneIntoRenderText : MonoBehaviour
    {
        public Material _material;

        public bool doubleBlur;
        
        private Camera _camera;

        private RenderTexture _intermediaryTexture;

        private void OnEnable()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            if (_intermediaryTexture == null)
                _intermediaryTexture = new RenderTexture(_camera.activeTexture);
        }
        
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (doubleBlur)
            {
                Graphics.Blit(source, _intermediaryTexture, _material);
                Graphics.Blit(_intermediaryTexture, destination, _material);
            }
            else
                Graphics.Blit(source, destination, _material);
        }

        private void OnDisable()
        {
            _intermediaryTexture?.Release();
            _intermediaryTexture = null;
        }
    }
}