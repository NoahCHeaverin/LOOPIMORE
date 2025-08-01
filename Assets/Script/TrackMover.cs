using UnityEngine;

public class TrackMover : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2f;

    private float fixedY;
    private float fixedZ;

    private float trackLength = 10f; // � adapter selon ta track (ex: nombre de segments * taille d�un segment)

    void Start()
    {
        if (startPoint != null)
        {
            fixedY = startPoint.position.y;
            fixedZ = startPoint.position.z;

            transform.position = new Vector3(startPoint.position.x, fixedY, fixedZ);
        }

        // Calculer la longueur r�elle de la track si possible
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            trackLength = rend.bounds.size.x;
        }
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        // D�placer vers la gauche uniquement sur X
        Vector3 targetPos = new Vector3(endPoint.position.x, fixedY, fixedZ);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Calculer la position du bord droit de la track
        float rightEdgeX = transform.position.x + (trackLength / 2f);

        // V�rifie si le bord droit est pass� derri�re le endPoint
        if (rightEdgeX <= endPoint.position.x)
        {
            // Repositionner au startPoint avec Y/Z fixes
            transform.position = new Vector3(startPoint.position.x, fixedY, fixedZ);
        }
    }
}
