using UnityEngine;
using System.Collections.Generic;

public class DragAndDropUI : MonoBehaviour
{
    [Tooltip("Les 5 (ou plus) prefabs disponibles pour le joueur.")]
    public List<GameObject> availablePrefabs = new List<GameObject>();

    [Header("Spawn")]
    public float defaultSpawnY = 0f;

    public void OnClickSpawn(int index)
    {
        if (index < 0 || index >= availablePrefabs.Count)
        {
            Debug.LogWarning($"[DragAndDropUI] Index {index} invalide.");
            return;
        }

        GameObject prefab = availablePrefabs[index];
        GameObject obj = Instantiate(prefab);

        Camera cam = Camera.main;
        Vector3 spawnPos = GetMouseWorldPosition(cam, defaultSpawnY);
        obj.transform.position = spawnPos;

        var mb = obj.GetComponent<MusicBlock>();
        if (mb == null) mb = obj.AddComponent<MusicBlock>();

        // Démarrer le drag immédiatement (et ne pas détruire si le drop échoue)
        mb.destroyIfDropFailed = false;  // <-- important pour éviter la "disparition"
        mb.BeginDragFromUI();
    }

    Vector3 GetMouseWorldPosition(Camera cam, float planeY)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        if (plane.Raycast(ray, out float d)) return ray.GetPoint(d);
        return Vector3.zero;
    }
}
