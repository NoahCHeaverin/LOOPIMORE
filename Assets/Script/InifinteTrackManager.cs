using UnityEngine;
using System.Collections.Generic;

public class InfiniteTrackManager : MonoBehaviour
{
    public GameObject trackPrefab;       // Le prefab du segment de piste
    public int segmentCount = 3;         // Combien de segments à instancier
    public float segmentLength = 10f;    // Longueur d’un segment (en X)
    public float moveSpeed = 2f;         // Vitesse de défilement

    // Rendre public la liste des segments pour pouvoir y accéder de l'extérieur
    public List<GameObject> segments = new List<GameObject>();

    void Start()
    {
        // Créer les segments de piste alignés horizontalement
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = new Vector3(i * segmentLength, 0f, 0f); // aligné sur X
            GameObject segment = Instantiate(trackPrefab, pos, Quaternion.identity, transform);
            segments.Add(segment);
        }
    }

    void Update()
    {
        foreach (GameObject segment in segments)
        {
            segment.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }

        // Recyclage : si un segment sort à gauche, le replacer à la fin
        for (int i = 0; i < segments.Count; i++)
        {
            GameObject seg = segments[i];
            if (seg.transform.position.x <= -segmentLength)
            {
                // Trouver le segment le plus à droite
                float maxX = float.MinValue;
                foreach (var s in segments)
                {
                    if (s.transform.position.x > maxX)
                        maxX = s.transform.position.x;
                }

                // Replacer ce segment à droite du plus loin
                seg.transform.position = new Vector3(maxX + segmentLength, seg.transform.position.y, seg.transform.position.z);
            }
        }
    }
}
