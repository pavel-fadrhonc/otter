    using System;
    using System.Linq;
    using UnityEngine;

namespace DefaultNamespace
{
    public class Locator : MonoBehaviour
    {
        public GameControls GameControls { get; private set; }

        public OtterController Otter1 { get; private set; }
        public OtterController Otter2 { get; private set; }
        
        public CustomAudioManager AudioManager { get; private set; }
        
        public ChillStat ChillStat { get; private set; }

        private static Locator _instance;
        public static Locator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Locator>();
                    
                    if (_instance == null)
                        Debug.LogError("No Locator in scene.");
                }
                    

                return _instance;
            }
        }

        private void Awake()
        {
            GameControls = FindObjectOfType<GameControls>();
        }

        private void Start()
        {
            Otter1 = FindObjectsOfType<OtterController>().First(o => o.otterControlType == EOtterControlType.Otter1);
            Otter2 = FindObjectsOfType<OtterController>().First(o => o.otterControlType == EOtterControlType.Otter2);
            AudioManager = FindObjectOfType<CustomAudioManager>();
            ChillStat = FindObjectOfType<ChillStat>();
        }
    }
}