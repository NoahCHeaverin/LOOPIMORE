using UnityEngine;

[ExecuteAlways]
public class SegmentConfig : MonoBehaviour
{
    [Header("Zone intérieure autorisée (local)")]
    public Vector3 innerSize = new Vector3(1f, 1f, 1f);  // largeur (X), hauteur (Y), profondeur (Z) en LOCAL
    public Vector3 innerCenterOffset = Vector3.zero;     // offset local du centre de la zone
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.25f);

    public Bounds GetWorldInnerBounds()
    {
        Vector3 worldCenter = transform.TransformPoint(innerCenterOffset);
        Vector3 worldSize = Vector3.Scale(innerSize, transform.lossyScale);
        return new Bounds(worldCenter, worldSize);
    }

    void OnDrawGizmosSelected()
    {
        var b = GetWorldInnerBounds();
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
