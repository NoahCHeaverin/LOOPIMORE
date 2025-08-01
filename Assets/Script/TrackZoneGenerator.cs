using UnityEngine;
using System.Collections.Generic;

public class TrackZoneGenerator : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject zonePrefab;
    public float zoneSize = 1f;

    [HideInInspector]
    public List<GameObject> zones = new List<GameObject>();

    void Start()
    {
        GenerateZones();
    }

    public void GenerateZones()
    {
        // Supprimer anciennes zones
        foreach (var zone in zones)
        {
            if (zone != null)
                Destroy(zone);
        }
        zones.Clear();

        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float trackLength = Vector3.Distance(startPoint.position, endPoint.position);
        int zoneCount = Mathf.FloorToInt(trackLength / zoneSize);

        for (int i = 0; i < zoneCount; i++)
        {
            // Position en monde centrée dans chaque zone
            Vector3 worldPos = startPoint.position + direction * (zoneSize * i + zoneSize / 2f);

            // Instancier zone à la position calculée et en faire enfant
            GameObject zone = Instantiate(zonePrefab, worldPos, Quaternion.identity, transform);
            zone.name = "Zone_" + i;

            zones.Add(zone);
        }
    }
}
