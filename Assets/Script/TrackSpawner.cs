using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform startPoint;
    public Transform endPoint;
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnPrefab();
            timer = 0f;
        }
    }

    void SpawnPrefab()
    {
        GameObject obj = Instantiate(prefab, startPoint.position, Quaternion.identity);
        TrackMover mover = obj.AddComponent<TrackMover>();
        mover.endPoint = endPoint;
    }
}
