#pragma warning disable 649

using System;
using UnityEngine;

namespace OakFramework2.Utils
{
    /// <summary>
    /// Keeps values set on start or set referenced
    /// </summary>
    [ExecuteInEditMode]
    public class TransformOverrider : MonoBehaviour
    {
        public enum EOverrideMode
        {
            None,
            KeepStart,
            SetPrefedefined
        }

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        
        [Header("Position")]
        [SerializeField] private EOverrideMode positionOverrideMode;
        [SerializeField] private Space positionSpace;
        [DrawIf("positionOverrideMode", EOverrideMode.SetPrefedefined)]
        [SerializeField] private Vector3 positionReferenceValue;
        
        [Header("Rotation")]
        [SerializeField] private EOverrideMode rotationOverrideMode;
        [SerializeField] private Space rotationSpace;
        [DrawIf("rotationOverrideMode", EOverrideMode.SetPrefedefined)]
        [SerializeField] private Vector3 rotationReferenceValueEuler;
        
        [Header("Scale")]
        [SerializeField] private EOverrideMode scaleOverrideMode;
        [SerializeField] private Space scaleSpace;
        [DrawIf("scaleOverrideMode", EOverrideMode.SetPrefedefined)]
        [SerializeField] private Vector3 scaleReferenceValue;

        private void Awake()
        {
            startPosition = positionSpace == Space.World ? transform.position : transform.localPosition;
            startRotation = rotationSpace == Space.World ? transform.rotation : transform.localRotation;
            startScale = scaleSpace == Space.World ? transform.lossyScale : transform.localScale;
        }

        private void LateUpdate()
        {
            //// POSITION
            Vector3 position;
            switch (positionOverrideMode)
            {
                case EOverrideMode.None:
                    position = positionSpace == Space.World ? transform.position : transform.localPosition;
                    break;
                case EOverrideMode.KeepStart:
                    position = startPosition;
                    break;
                case EOverrideMode.SetPrefedefined:
                    position = positionReferenceValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (positionSpace == Space.World)
            {
                transform.position = position;
            }
            else
            {
                transform.localPosition = position;
            }
            
            ///// ROTATION
            Quaternion rotation;
            switch (rotationOverrideMode)
            {
                case EOverrideMode.None:
                    rotation = rotationSpace == Space.World ? transform.rotation : transform.localRotation;
                    break;
                case EOverrideMode.KeepStart:
                    rotation = startRotation;
                    break;
                case EOverrideMode.SetPrefedefined:
                    rotation = Quaternion.Euler(rotationReferenceValueEuler);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (rotationSpace == Space.World)
            {
                transform.rotation = rotation;
            }
            else
            {
                transform.localRotation = rotation;
            }
            
            ///// SCALE
            Vector3 scale;
            switch (scaleOverrideMode)
            {
                case EOverrideMode.None:
                    scale = scaleSpace == Space.World ? transform.lossyScale : transform.localScale;
                    break;
                case EOverrideMode.KeepStart:
                    scale = startScale;
                    break;
                case EOverrideMode.SetPrefedefined:
                    scale = scaleReferenceValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (scaleSpace == Space.World)
            {
                //transform.lossyScale = scale;
            }
            else
            {
                transform.localScale = scale;
            } 
        }
    }
}