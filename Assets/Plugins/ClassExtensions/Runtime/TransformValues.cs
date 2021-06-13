using UnityEngine;


namespace OakFramework2.Utils
{
    /// <summary>
    /// For saving or loading Transform values
    /// </summary>
    public struct TransformValues
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LocalScale;

        public void SetFromLocalTransformValues(Transform dst)
        {
            dst.SetFromLocalTransformValues(this);
        }

        public void SetFromWorldTransformValuesExceptScale(Transform dst)
        {
            dst.SetFromWorldTransformValuesExceptScale(this);
        }
    }    

}
