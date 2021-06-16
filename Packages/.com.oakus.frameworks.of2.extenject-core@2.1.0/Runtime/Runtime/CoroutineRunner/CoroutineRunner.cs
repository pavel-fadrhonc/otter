using System.Collections;
using UnityEngine;

namespace Zenject
{
    public class CoroutineRunner : MonoBehaviour
    {
        [Inject]
        private ofCoroutine.Factory _coroutineFactory;
        
        public ofCoroutine RunCoroutine(IEnumerator routine, float delay = 0)
        {
            return RunCoroutineCoroutineWithDelay(routine, delay);
        }

        public void PauseCoroutine(ofCoroutine coroutine)
        {
            coroutine.Paused = true;
        }
        
        public void PauseCoroutineFor(ofCoroutine coroutine, float pauseTime)
        {
            coroutine.PauseFor(pauseTime);
        }

        public void ResumeCoroutine(ofCoroutine coroutine)
        {
            coroutine.Paused = false;
        }

        public void StopCoroutine(ofCoroutine coroutine)
        {
            base.StopCoroutine(coroutine);
            
            coroutine.Dispose();
        }

        private ofCoroutine RunCoroutineCoroutineWithDelay(IEnumerator routine, float delay = 0)
        {
            var coroutine = _coroutineFactory.Create(routine);
            if (delay > 0)
                coroutine.PauseFor(delay);

            StartCoroutine(coroutine);

            return coroutine;
        }
    }
}