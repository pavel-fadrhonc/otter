using System;
using System.Collections.Generic;
using System.Linq;
using of2.DebugTools;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public class RiverGenerator : MonoBehaviour
    {
        public List<GameObject> shorePrefabs = new List<GameObject>();

        public int generateAtStart;

        public float checkPeriod = 0.3f; 
        
        [Tooltip("Generate new shore when camera top closer that this distance.")]
        public float lookAhead;
        [Tooltip("Recycle old shore when camera top further that this distance.")]
        public float lookBehind;

        private List<GameObject> generatedShores = new List<GameObject>();

        private Camera _mainCamera;

        private float _nextCheckTime;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            for (int i = 0; i < generateAtStart; i++)
            {
                GenerateShore();
            }
        }

        private void Update()
        {
            // check generating of next segment
            if (Time.time < _nextCheckTime)
                return;
            
            var cameraTopY = _mainCamera.transform.position.y + _mainCamera.orthographicSize;
            var furthestPoint = GetFurthestPoint();

            if (furthestPoint.y - lookAhead <  cameraTopY)
                GenerateShore();

            _nextCheckTime = Time.time + checkPeriod;
        }

        private void GenerateShore()
        {
            var furthestPoint = GetFurthestPoint();
            
            DebugDraw.DrawMarker(furthestPoint, 0.5f, Color.red,  5f);

            var shoreInstance = Instantiate(shorePrefabs.Random(), furthestPoint, quaternion.identity);

            generatedShores.Add(shoreInstance);
        }

        private Vector3 GetFurthestPoint()
        {
            if (generatedShores.Count == 0)
                return transform.position;
            
            var furthestShore = generatedShores.Last();
            var rends = furthestShore.GetComponentsInChildren<Renderer>();

            var totalBound = new Bounds(furthestShore.transform.position, Vector3.zero);
            foreach (var rend in rends)
            {
                totalBound.Encapsulate(rend.bounds);
            }

            var furthestPoint = furthestShore.transform.position + totalBound.size.y * Vector3.up;

            return furthestPoint;
        }
    }
}