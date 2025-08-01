using UnityEngine;

public class DestroyWhenPastEndpoint : MonoBehaviour
{
    public Transform endPoint;

    void Update()
    {
        if (endPoint == null) return;

        // Si la position en X de l'objet est plus petite que celle du endPoint, le détruire
        if (transform.position.x < endPoint.position.x)
        {
            Destroy(gameObject);
        }
    }
}
