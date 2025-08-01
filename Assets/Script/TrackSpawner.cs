using UnityEngine;
using System.Collections.Generic;

public class TrackSpawner : MonoBehaviour
{
    public GameObject prefab;
    public TrackSegmentLooper trackLooper; // référence au script qui gère les segments
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnOnRandomSegment();
            timer = 0f;
        }
    }

    void SpawnOnRandomSegment()
    {
        if (trackLooper == null || trackLooper.segments.Count == 0) return;

        // Choisir un segment aléatoire (ou logique, selon ton besoin)
        GameObject segment = trackLooper.segments[Random.Range(0, trackLooper.segments.Count)];

        // Spawn le prefab en position locale du segment (au centre par ex.)
        Vector3 spawnPos = segment.transform.position;

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Faire de ce prefab un enfant du segment pour qu'il suive le segment en mouvement
        obj.transform.SetParent(segment.transform, true);
    }
}
