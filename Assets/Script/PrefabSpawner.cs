using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    public float speed = 2f; // même que la track

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        obj.AddComponent<AutoMover>().speed = speed;
    }
}
