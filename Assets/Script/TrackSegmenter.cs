using UnityEngine;

public class TrackSegmenter : MonoBehaviour
{
    public int segmentCount = 5;
    public float segmentLength = 2f;
    public GameObject segmentPrefab;

    [HideInInspector]
    public Transform[] segments;

    void Start()
    {
        segments = new Transform[segmentCount];

        // Nettoyer anciens enfants (segments) si besoin
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = new Vector3(i * segmentLength, 0, 0); // Alignement sur X
            GameObject seg = Instantiate(segmentPrefab, transform);
            seg.transform.localPosition = pos;
            seg.transform.localRotation = Quaternion.identity;
            seg.transform.localScale = new Vector3(segmentLength, 1f, 1f); // Ajuste selon besoin
            seg.name = "Segment_" + i;
            segments[i] = seg.transform;
        }
    }
}
