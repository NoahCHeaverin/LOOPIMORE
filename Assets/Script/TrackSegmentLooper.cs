using UnityEngine;
using System.Collections.Generic;

public class TrackSegmentLooper : MonoBehaviour
{
    public GameObject trackSegmentPrefab;   // Prefab d�un segment de piste
    public int segmentCount = 3;             // Nombre de segments sur la piste
    public float segmentLength = 10f;        // Longueur d�un segment en X
    public float moveSpeed = 2f;             // Vitesse de d�filement
    public Transform endPoint;               // Point o� la piste doit se r�initialiser

    public Material materialA;               // Mat�riau pour segments pairs
    public Material materialB;               // Mat�riau pour segments impairs

    public List<GameObject> segments = new List<GameObject>();

    void Start()
    {
        // Instancier les segments align�s sur X � partir de la position du GameObject parent
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = transform.position + Vector3.right * segmentLength * i;
            GameObject segment = Instantiate(trackSegmentPrefab, pos, Quaternion.identity, transform);

            // Appliquer le mat�riau selon la parit� de l'index
            Renderer rend = segment.GetComponent<Renderer>();
            if (rend != null)
            {
                if (i % 2 == 0)
                    rend.material = materialA;
                else
                    rend.material = materialB;
            }

            segments.Add(segment);
        }
    }

    void Update()
    {
        // D�placer chaque segment vers la gauche
        foreach (var segment in segments)
        {
            segment.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }

        // Recycler les segments qui ont d�pass� l'endPoint
        for (int i = 0; i < segments.Count; i++)
        {
            GameObject seg = segments[i];

            // Si le segment est pass� � gauche de l'endPoint
            if (seg.transform.position.x + segmentLength / 2f <= endPoint.position.x)
            {
                // Trouver le segment le plus � droite
                float maxX = float.MinValue;
                foreach (var s in segments)
                {
                    if (s.transform.position.x > maxX)
                        maxX = s.transform.position.x;
                }

                // Replacer ce segment � droite du segment le plus � droite
                seg.transform.position = new Vector3(maxX + segmentLength, seg.transform.position.y, seg.transform.position.z);
            }
        }
    }
}
