using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    public GameObject segmentPrefab;
    public int segmentCount = 10;
    public float segmentLength = 1f;

    void Start()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = new Vector3(i * segmentLength, 0f, 0f);
            GameObject segment = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
            segment.name = $"Segment_{i}";
        }
    }
}
