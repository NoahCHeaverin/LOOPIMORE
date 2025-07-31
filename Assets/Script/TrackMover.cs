using UnityEngine;

public class TrackMover : MonoBehaviour
{
    public Transform endPoint;
    public float speed = 2f;

    void Update()
    {
        if (endPoint == null) return;

        // Déplacement vers endPoint
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);

        // Optionnel : détruire quand arrivé à la fin
        if (transform.position == endPoint.position)
        {
            Destroy(gameObject);
        }
    }
}
