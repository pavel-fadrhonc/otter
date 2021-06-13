using UnityEngine;
using System.Collections;

public static class BoxColliderOverlapsPoint
{
    public static bool OverlapsPoint(this BoxCollider col, Vector3 point)
    {
        var pointLocal = col.transform.InverseTransformPoint(point);

        return pointLocal.x > col.center.x - (col.size.x * 0.5f) && pointLocal.x < col.center.x + (col.size.x * 0.5f) &&
            pointLocal.y > col.center.y - (col.size.y * 0.5f) && pointLocal.y < col.center.y + (col.size.y * 0.5f) &&
            pointLocal.z > col.center.z - (col.size.z * 0.5f) && pointLocal.z < col.center.z + (col.size.z * 0.5f);
    }
}
