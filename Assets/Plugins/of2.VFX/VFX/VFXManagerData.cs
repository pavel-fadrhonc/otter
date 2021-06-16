using System;
using System.Collections;
using System.Collections.Generic;
using of2.VFX;
using UnityEngine;

namespace of2.VFX
{
    [Serializable]
    public class VFXData
    {
        public string Name;
        public int GUID;
        public VFXHolder VFXPrefab;
    }

    public class VFXManagerData : ScriptableObject
    {
        [SerializeField]
        private List<VFXData> _VFXs = new List<VFXData>();

        public List<VFXData> VFXs
        {
            get
            {
                return _VFXs;
            }
        }
    }
}